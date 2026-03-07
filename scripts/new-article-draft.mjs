#!/usr/bin/env node
import fs from 'node:fs/promises'
import path from 'node:path'

function slugify(input) {
  return input
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
}

function todayIso() {
  const d = new Date()
  const yyyy = d.getFullYear()
  const mm = String(d.getMonth() + 1).padStart(2, '0')
  const dd = String(d.getDate()).padStart(2, '0')
  return `${yyyy}-${mm}-${dd}`
}

async function main() {
  const args = process.argv.slice(2)
  const titleArgIndex = args.indexOf('--title')
  const summaryArgIndex = args.indexOf('--summary')
  const typeArgIndex = args.indexOf('--type')
  const dateArgIndex = args.indexOf('--date')

  const title = titleArgIndex >= 0 ? args[titleArgIndex + 1] : ''
  if (!title) {
    console.error('Usage: npm run new:draft -- --title "..." [--summary "..."] [--type article|newsletter] [--date YYYY-MM-DD]')
    process.exit(1)
  }

  const summary = summaryArgIndex >= 0 ? args[summaryArgIndex + 1] : ''
  const type = typeArgIndex >= 0 ? args[typeArgIndex + 1] : 'article'
  const date = dateArgIndex >= 0 ? args[dateArgIndex + 1] : todayIso()

  const slug = slugify(title)
  const dir = path.join(process.cwd(), 'src', 'content', 'articles', 'drafts')
  const filename = `${date}-${slug}.md`
  const filePath = path.join(dir, filename)

  await fs.mkdir(dir, { recursive: true })

  const body = `---\n` +
    `title: "${title.replaceAll('"', '\\"')}"\n` +
    (summary ? `summary: "${summary.replaceAll('"', '\\"')}"\n` : `summary: ""\n`) +
    `publishedAt: "${date}"\n` +
    `type: "${type}"\n` +
    `status: "draft"\n` +
    `---\n\n` +
    `# ${title}\n\n` +
    `<!-- Draft content here -->\n`

  await fs.writeFile(filePath, body, 'utf8')
  console.log(`Created draft: ${filePath}`)
}

main().catch((err) => {
  console.error(err)
  process.exit(1)
})
