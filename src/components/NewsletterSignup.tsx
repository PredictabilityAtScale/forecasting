import { useState, type FormEvent } from 'react'

const NEWSLETTER_ENDPOINT = 'https://app.loops.so/api/newsletter-form/cmgcq8csqkh4lzf0i2ohf87o7'
const LOOPS_TIMESTAMP_KEY = 'loops-form-timestamp'
const NEWSLETTER_RATE_LIMIT_MS = 60_000

export default function NewsletterSignup() {
  const [email, setEmail] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [status, setStatus] = useState<'idle' | 'success' | 'error'>('idle')
  const [errorMessage, setErrorMessage] = useState('Oops! Something went wrong, please try again')

  const resetForm = () => {
    setStatus('idle')
    setErrorMessage('Oops! Something went wrong, please try again')
  }

  const submitHandler = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    const timestamp = Date.now()
    const previousTimestamp = localStorage.getItem(LOOPS_TIMESTAMP_KEY)

    if (previousTimestamp && Number(previousTimestamp) + NEWSLETTER_RATE_LIMIT_MS > timestamp) {
      setStatus('error')
      setErrorMessage('Too many signups, please try again in a little while')
      return
    }

    localStorage.setItem(LOOPS_TIMESTAMP_KEY, String(timestamp))
    setIsSubmitting(true)
    setStatus('idle')
    setErrorMessage('Oops! Something went wrong, please try again')

    const formBody = `userGroup=Newsletter&mailingLists=&email=${encodeURIComponent(email)}`

    try {
      const response = await fetch(NEWSLETTER_ENDPOINT, {
        method: 'POST',
        body: formBody,
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
        },
      })

      if (response.ok) {
        setStatus('success')
        setEmail('')
        return
      }

      const data = await response.json().catch(() => null)
      setStatus('error')
      setErrorMessage(data?.message ?? response.statusText)
    } catch (error) {
      const message = error instanceof Error ? error.message : ''
      setStatus('error')

      if (message === 'Failed to fetch') {
        setErrorMessage('Too many signups, please try again in a little while')
      } else if (message) {
        setErrorMessage(message)
        localStorage.setItem(LOOPS_TIMESTAMP_KEY, '')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="w-full">
      {status === 'idle' ? (
        <form onSubmit={submitHandler} className="flex w-full flex-col items-center justify-center gap-2">
          <input
            type="email"
            name="newsletter-form-input"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            placeholder="you@example.com"
            required
            className="h-10 w-full max-w-[300px] rounded-md border border-[var(--line)] bg-white px-3 text-sm text-[var(--sea-ink)] shadow-[0_1px_2px_rgba(0,0,0,0.05)] outline-none transition focus:border-[var(--lagoon)]"
          />
          <button
            type="submit"
            disabled={isSubmitting}
            className="inline-flex h-10 w-full max-w-[300px] items-center justify-center rounded-md bg-[var(--lagoon-deep)] px-4 text-sm font-medium text-center text-white transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-90"
          >
            {isSubmitting ? 'Please wait...' : 'Monthly Newsletter'}
          </button>
        </form>
      ) : (
        <div className="space-y-3 text-center">
          <p
            className={`m-0 text-sm ${status === 'success' ? 'text-[var(--sea-ink)]' : 'text-[#b91c1c]'}`}
          >
            {status === 'success' ? "Thanks! We'll be in touch!" : errorMessage}
          </p>
          <button
            type="button"
            onClick={resetForm}
            className="bg-transparent px-1 py-1 text-sm text-[#6b7280] transition hover:underline"
          >
            &larr; Back
          </button>
        </div>
      )}
    </div>
  )
}
