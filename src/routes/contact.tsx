import { createFileRoute } from '@tanstack/react-router'
import { useEffect, useRef } from 'react'
import NewsletterSignup from '#/components/NewsletterSignup'

const SCHEDULE_URL = 'https://freebusy.io/data'
const DIRECT_EMAIL = 'troy.magennis@focusedobjective.com'
const LINKEDIN_URL = 'https://www.linkedin.com/in/troymagennis/'

export const Route = createFileRoute('/contact')({
  component: ContactPage,
})

function ContactPage() {
  const freebusyContainerRef = useRef<HTMLDivElement | null>(null)

  useEffect(() => {
    document
      .querySelectorAll<HTMLScriptElement>('script[src="https://js.freebusy.io/embed/v1.js"]')
      .forEach((element) => element.remove())

    const script = document.createElement('script')
    script.src = 'https://js.freebusy.io/embed/v1.js'
    script.type = 'text/javascript'
    script.async = true
    document.body.appendChild(script)

    return () => {
      if (script.parentNode) {
        script.parentNode.removeChild(script)
      }
      if (freebusyContainerRef.current) {
        freebusyContainerRef.current.innerHTML = ''
      }
    }
  }, [])

  return (
    <main className="mx-auto max-w-7xl px-4 pb-16 pt-10 sm:px-6 lg:px-8">
      <section className="island-shell rounded-2xl p-6 sm:p-8">
        <p className="island-kicker mb-2">Contact me directly</p>
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:gap-6">
          <a
            href={`mailto:${DIRECT_EMAIL}`}
            className="text-base font-medium text-[var(--lagoon-deep)] underline-offset-2 transition hover:underline"
          >
            {DIRECT_EMAIL}
          </a>
          <a
            href={LINKEDIN_URL}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center gap-1 text-base font-medium text-[var(--lagoon-deep)] underline-offset-2 transition hover:underline"
          >
            LinkedIn
            <svg className="h-3.5 w-3.5" viewBox="0 0 16 16" fill="currentColor">
              <path d="M4.5 2a.5.5 0 0 0 0 1h6.793L2.146 12.146a.5.5 0 0 0 .708.708L12 3.707V10.5a.5.5 0 0 0 1 0v-9a.5.5 0 0 0-.5-.5h-8Z" />
            </svg>
          </a>
        </div>
      </section>

      <section className="island-shell mt-10 rounded-2xl p-6 sm:p-8">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <p className="island-kicker mb-2">Monthly Forecasting Updates</p>
            <h2 className="mb-2 text-xl font-bold text-[var(--sea-ink)] sm:text-2xl">
              Join the newsletter
            </h2>
            <p className="m-0 max-w-xl text-sm leading-relaxed text-[var(--sea-ink-soft)]">
              Practical forecasting insights, new tools, and workshop announcements once per month.
            </p>
          </div>
          <NewsletterSignup />
        </div>
      </section>

      <section className="island-shell mt-10 rounded-3xl px-6 py-8 sm:px-8 sm:py-10">
        <p className="island-kicker mb-3">Contact</p>
        <h1 className="display-title mb-3 text-3xl leading-tight font-semibold tracking-tight text-[var(--sea-ink)] sm:text-4xl">
          Schedule a meeting
        </h1>
        <div className="mt-5 overflow-hidden rounded-2xl border border-[var(--line)] bg-[var(--surface)] p-3 sm:p-4">
          <div
            ref={freebusyContainerRef}
            className="freebusy-pickatime-widget"
            data-link={SCHEDULE_URL}
            data-embedType="inline"
            data-branding="visible"
            style={{ minWidth: '320px', minHeight: '640px' }}
          />
        </div>
        <a
          href={SCHEDULE_URL}
          target="_blank"
          rel="noopener noreferrer"
          className="mt-4 inline-flex items-center gap-1 text-sm font-semibold text-[var(--lagoon-deep)]"
        >
          Open scheduler in a new tab
          <svg className="h-3.5 w-3.5" viewBox="0 0 16 16" fill="currentColor">
            <path d="M4.5 2a.5.5 0 0 0 0 1h6.793L2.146 12.146a.5.5 0 0 0 .708.708L12 3.707V10.5a.5.5 0 0 0 1 0v-9a.5.5 0 0 0-.5-.5h-8Z" />
          </svg>
        </a>
      </section>
    </main>
  )
}
