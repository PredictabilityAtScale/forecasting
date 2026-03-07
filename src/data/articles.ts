import newsletterKickoff from '#/content/articles/2026-01-10-newsletter-archive-kickoff.md?raw'
import throughputPatterns from '#/content/articles/2026-01-12-throughput-patterns.md?raw'
import newsletterConfidenceIntervals from '#/content/articles/2026-01-17-newsletter-confidence-intervals.md?raw'
import slicingWork from '#/content/articles/2026-01-19-slicing-work-for-confidence.md?raw'
import riskAdjustmentRitual from '#/content/articles/2026-01-26-risk-adjustment-ritual.md?raw'

export type ArticleType = 'article' | 'newsletter'

export interface ArticleEntry {
  slug: string
  title: string
  summary: string
  publishedAt: string
  type: ArticleType
  markdown: string
}

export const ARTICLE_ENTRIES: ArticleEntry[] = [
  {
    slug: 'risk-adjustment-ritual',
    title: 'A weekly risk-adjustment ritual for delivery plans',
    summary: 'A fast 20-minute weekly routine to recalibrate delivery ranges with transparent assumptions.',
    publishedAt: '2026-01-26',
    type: 'article',
    markdown: riskAdjustmentRitual,
  },
  {
    slug: 'slicing-work-for-confidence',
    title: 'Slice work smaller to tighten forecast confidence',
    summary: 'Practical ways to reduce forecast spread by making work sizing and splitting more consistent.',
    publishedAt: '2026-01-19',
    type: 'article',
    markdown: slicingWork,
  },
  {
    slug: 'throughput-patterns',
    title: 'Throughput patterns: what changed this week?',
    summary: 'A weekly checklist to convert throughput data into useful communication for stakeholders.',
    publishedAt: '2026-01-12',
    type: 'article',
    markdown: throughputPatterns,
  },
  {
    slug: 'newsletter-confidence-intervals',
    title: 'Newsletter #2 — Confidence intervals people can actually use',
    summary: 'How to present confidence intervals in a way that supports better product decisions.',
    publishedAt: '2026-01-17',
    type: 'newsletter',
    markdown: newsletterConfidenceIntervals,
  },
  {
    slug: 'newsletter-archive-kickoff',
    title: 'Newsletter #1 — Forecasting archive kickoff',
    summary: 'The first archived issue and the new weekly publishing rhythm for article + newsletter updates.',
    publishedAt: '2026-01-10',
    type: 'newsletter',
    markdown: newsletterKickoff,
  },
]

export function getEntryBySlug(slug: string) {
  return ARTICLE_ENTRIES.find((entry) => entry.slug === slug)
}
