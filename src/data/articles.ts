export type ArticleType = 'article' | 'newsletter'
export type ArticleStatus = 'draft' | 'published'

export interface ArticleEntry {
  slug: string
  title: string
  summary: string
  publishedAt: string
  type: ArticleType
  status: ArticleStatus
  heroImage?: string
  markdown: string
}

type Frontmatter = Partial<{
  title: string
  summary: string
  publishedAt: string
  type: ArticleType
  status: ArticleStatus
  heroImage: string
}>

function parseFrontmatter(raw: string): { frontmatter: Frontmatter; body: string } {
  const content = raw.replace(/^\uFEFF/, '')
  const match = content.match(/^---\r?\n([\s\S]*?)\r?\n---(?:\r?\n|$)/)
  if (!match) {
    return { frontmatter: {}, body: raw }
  }

  const fmBlock = match[1]
  const body = content.slice(match[0].length)

  const frontmatter: Frontmatter = {}
  for (const line of fmBlock.split(/\r?\n/)) {
    const m = line.match(/^([A-Za-z0-9_]+)\s*:\s*(.*)$/)
    if (!m) continue
    const key = m[1]
    const value = m[2].trim().replace(/^"|"$/g, '').replace(/^'|'$/g, '')
    if (!value) continue

    if (key === 'title') frontmatter.title = value
    if (key === 'summary') frontmatter.summary = value
    if (key === 'publishedAt') frontmatter.publishedAt = value
    if (key === 'type' && (value === 'article' || value === 'newsletter')) frontmatter.type = value
    if (key === 'status' && (value === 'draft' || value === 'published')) frontmatter.status = value
    if (key === 'heroImage') frontmatter.heroImage = value
  }

  return { frontmatter, body }
}

function slugFromPath(path: string) {
  const base = path.split('/').pop() || ''
  const noExt = base.replace(/\.(md|mdx)$/i, '')
  // Strip leading YYYY-MM-DD-
  return noExt.replace(/^\d{4}-\d{2}-\d{2}-/, '')
}

function entryFromFile(path: string, raw: string): ArticleEntry {
  const { frontmatter, body } = parseFrontmatter(raw)
  const slug = slugFromPath(path)

  const publishedAt = frontmatter.publishedAt || path.split('/').pop()?.slice(0, 10) || '1970-01-01'
  const type: ArticleType = frontmatter.type || (path.includes('newsletter') ? 'newsletter' : 'article')
  const status: ArticleStatus = frontmatter.status || (path.includes('/drafts/') ? 'draft' : 'published')

  const title = frontmatter.title || slug.replace(/-/g, ' ').replace(/\b\w/g, (c) => c.toUpperCase())
  const summary = frontmatter.summary || ''

  return {
    slug,
    title,
    summary,
    publishedAt,
    type,
    status,
    heroImage: frontmatter.heroImage,
    markdown: body.trim(),
  }
}

// Load markdown from both published + drafts directories.
const modules = import.meta.glob('../content/articles/**/*.{md,mdx}', {
  eager: true,
  query: '?raw',
  import: 'default',
})

export const ARTICLE_ENTRIES: ArticleEntry[] = Object.entries(modules)
  .map(([path, raw]) => entryFromFile(path, raw as string))
  .sort((a, b) => (a.publishedAt < b.publishedAt ? 1 : -1))

export function getEntryBySlug(slug: string) {
  return ARTICLE_ENTRIES.find((entry) => entry.slug === slug)
}
