import { SIMML_SCHEMA, flattenSchema, type SimMLSchemaAttribute, type SimMLSchemaElement } from '#/data/simml-schema'

export type SimMLEditorSeverity = 'error' | 'warning'

export interface SimMLEditorDiagnostic {
  severity: SimMLEditorSeverity
  message: string
  line: number
  column: number
  from: number
  to: number
}

const FLAT_SCHEMA = flattenSchema(SIMML_SCHEMA)
const TAG_HELP = new Map<string, SimMLSchemaElement>()

for (const { element } of FLAT_SCHEMA) {
  if (!TAG_HELP.has(element.tag)) {
    TAG_HELP.set(element.tag, element)
  }
}

function hasAttributeOrAlias(node: Element, schemaNode: SimMLSchemaElement, attributeName: string) {
  if (node.hasAttribute(attributeName)) return true

  if (schemaNode.tag === 'phase' && attributeName === 'start') return node.hasAttribute('startPercentage')
  if (schemaNode.tag === 'phase' && attributeName === 'end') return node.hasAttribute('endPercentage')
  if (schemaNode.tag === 'column' && attributeName === 'columnId') return node.hasAttribute('id')

  return false
}

function isKnownAttribute(schemaNode: SimMLSchemaElement, attributeName: string) {
  const knownAttributes = new Set(schemaNode.attributes.map((attribute) => attribute.name))
  if (knownAttributes.has(attributeName)) return true

  if (schemaNode.tag === 'phase' && (attributeName === 'startPercentage' || attributeName === 'endPercentage')) {
    return true
  }

  if (schemaNode.tag === 'column' && attributeName === 'id') {
    return knownAttributes.has('columnId') || knownAttributes.has('id')
  }

  return false
}

function getLineStarts(source: string) {
  const starts = [0]
  for (let i = 0; i < source.length; i += 1) {
    if (source[i] === '\n') starts.push(i + 1)
  }
  return starts
}

function offsetToLineColumn(source: string, offset: number) {
  const starts = getLineStarts(source)
  let line = 0
  while (line + 1 < starts.length && starts[line + 1] <= offset) line += 1
  return { line: line + 1, column: offset - starts[line] + 1 }
}

function lineColumnToOffset(source: string, line: number, column: number) {
  const starts = getLineStarts(source)
  const lineStart = starts[Math.max(0, Math.min(starts.length - 1, line - 1))] ?? 0
  return Math.max(0, Math.min(source.length, lineStart + Math.max(0, column - 1)))
}

function firstTagOffset(source: string, tag: string, from = 0) {
  const escaped = tag.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
  const matcher = new RegExp(`<${escaped}(?=\\s|>|/>)`, 'g')
  matcher.lastIndex = from
  const match = matcher.exec(source)
  return match?.index ?? -1
}

function extractParserError(errorText: string, source: string): SimMLEditorDiagnostic | null {
  const found = /line\s+(\d+)\s+at\s+column\s+(\d+)/i.exec(errorText) ?? /line\s+(\d+).*column\s+(\d+)/i.exec(errorText)
  if (!found) return null
  const line = Number(found[1])
  const column = Number(found[2])
  const from = lineColumnToOffset(source, line, column)
  return {
    severity: 'error',
    message: errorText.trim() || 'XML parsing error',
    line,
    column,
    from,
    to: Math.min(source.length, from + 1),
  }
}

function validateElement(
  source: string,
  diagnostics: SimMLEditorDiagnostic[],
  node: Element,
  schemaNode: SimMLSchemaElement,
  scanStart: number,
) {
  const tagOffset = firstTagOffset(source, node.tagName, scanStart)
  const pos = tagOffset >= 0 ? offsetToLineColumn(source, tagOffset) : { line: 1, column: 1 }

  for (const requiredAttribute of schemaNode.attributes.filter((attribute) => attribute.mandatory)) {
    if (!hasAttributeOrAlias(node, schemaNode, requiredAttribute.name)) {
      diagnostics.push({
        severity: 'error',
        message: `<${schemaNode.tag}> is missing required attribute \"${requiredAttribute.name}\".`,
        line: pos.line,
        column: pos.column,
        from: tagOffset,
        to: Math.min(source.length, tagOffset + schemaNode.tag.length + 1),
      })
    }
  }

  for (const attribute of node.getAttributeNames()) {
    if (!isKnownAttribute(schemaNode, attribute)) {
      diagnostics.push({
        severity: 'warning',
        message: `Unknown attribute \"${attribute}\" on <${schemaNode.tag}>.`,
        line: pos.line,
        column: pos.column,
        from: tagOffset,
        to: Math.min(source.length, tagOffset + schemaNode.tag.length + 1),
      })
      continue
    }

    const attributeOffset = tagOffset >= 0 ? source.indexOf(`${attribute}=`, tagOffset) : -1
    const attributeSchema = schemaNode.attributes.find((item) => item.name === attribute)
    const attributeValue = node.getAttribute(attribute)

    if (attributeSchema?.validValues?.length && attributeValue && !attributeSchema.validValues.includes(attributeValue)) {
      const attrPos = attributeOffset >= 0 ? offsetToLineColumn(source, attributeOffset) : pos
      diagnostics.push({
        severity: 'warning',
        message: `Value \"${attributeValue}\" is not one of [${attributeSchema.validValues.join(', ')}] for ${attribute}.`,
        line: attrPos.line,
        column: attrPos.column,
        from: attributeOffset,
        to: Math.min(source.length, attributeOffset + attribute.length),
      })
    }
  }

  const children = Array.from(node.children)
  for (const requiredChild of schemaNode.children.filter((child) => child.mandatory)) {
    const hasChild = children.some((child) => child.tagName === requiredChild.tag)
    if (!hasChild) {
      diagnostics.push({
        severity: 'error',
        message: `<${schemaNode.tag}> requires child <${requiredChild.tag}>.`,
        line: pos.line,
        column: pos.column,
        from: tagOffset,
        to: Math.min(source.length, tagOffset + schemaNode.tag.length + 1),
      })
    }
  }

  let childScanStart = Math.max(scanStart, tagOffset)
  for (const child of children) {
    const childSchema = schemaNode.children.find((item) => item.tag === child.tagName)
    const childOffset = firstTagOffset(source, child.tagName, childScanStart)
    if (!childSchema) {
      const childPos = childOffset >= 0 ? offsetToLineColumn(source, childOffset) : pos
      diagnostics.push({
        severity: 'warning',
        message: `<${child.tagName}> is not documented as a valid child of <${schemaNode.tag}>.`,
        line: childPos.line,
        column: childPos.column,
        from: childOffset,
        to: Math.min(source.length, childOffset + child.tagName.length + 1),
      })
    } else {
      validateElement(source, diagnostics, child, childSchema, childScanStart)
    }
    if (childOffset >= 0) childScanStart = childOffset + 1
  }
}

export function validateSimMlSource(source: string): SimMLEditorDiagnostic[] {
  const diagnostics: SimMLEditorDiagnostic[] = []
  const doc = new DOMParser().parseFromString(source, 'application/xml')
  const parserError = doc.querySelector('parsererror')

  if (parserError) {
    const parseMessage = parserError.textContent ?? 'Unable to parse XML.'
    const parsedDiagnostic = extractParserError(parseMessage, source)
    diagnostics.push(
      parsedDiagnostic ?? {
        severity: 'error',
        message: parseMessage,
        line: 1,
        column: 1,
        from: 0,
        to: 1,
      },
    )
    return diagnostics
  }

  const root = doc.querySelector('simulation')
  if (!root) {
    diagnostics.push({
      severity: 'error',
      message: 'SimML must include a <simulation> root element.',
      line: 1,
      column: 1,
      from: 0,
      to: 1,
    })
    return diagnostics
  }

  validateElement(source, diagnostics, root, SIMML_SCHEMA, 0)
  return diagnostics
}

export function resolveTagHelp(tagName: string | null) {
  if (!tagName) return null
  return TAG_HELP.get(tagName) ?? null
}

export function resolveAttributeHelp(tagName: string | null, attributeName: string | null): SimMLSchemaAttribute | null {
  if (!tagName || !attributeName) return null
  const tag = resolveTagHelp(tagName)
  if (!tag) return null
  return tag.attributes.find((attribute) => attribute.name === attributeName) ?? null
}

export function getCursorContext(source: string, cursorOffset: number): {
  activeTag: string | null
  activeAttribute: string | null
  inOpenTag: boolean
  suggestedAttributes: SimMLSchemaAttribute[]
} {
  const safeOffset = Math.max(0, Math.min(source.length, cursorOffset))
  const before = source.slice(0, safeOffset)
  const lastOpen = before.lastIndexOf('<')
  const lastClose = before.lastIndexOf('>')
  const inOpenTag = lastOpen > lastClose

  if (!inOpenTag || lastOpen < 0) {
    return { activeTag: null, activeAttribute: null, inOpenTag: false, suggestedAttributes: [] }
  }

  const fragment = before.slice(lastOpen)
  const tagMatch = fragment.match(/^<\/?([A-Za-z?][\w:-]*)/)
  const activeTag = tagMatch?.[1] ?? null

  const attrNameMatch = fragment.match(/([A-Za-z_][\w:-]*)\s*=\s*["'][^"']*$/) ?? fragment.match(/([A-Za-z_][\w:-]*)$/)
  const activeAttribute = attrNameMatch?.[1] ?? null

  const tag = resolveTagHelp(activeTag)
  return {
    activeTag,
    activeAttribute,
    inOpenTag: true,
    suggestedAttributes: tag?.attributes ?? [],
  }
}
