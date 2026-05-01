import { createFileRoute } from '@tanstack/react-router'
import { createServerFn } from '@tanstack/react-start'
import { useMemo, useState } from 'react'
import { bookingAvailability } from '#/data/booking-availability'
import { bookingSchema } from '#/data/booking-schema'

const submitBooking = createServerFn({ method: 'POST' })
  .inputValidator((data: unknown) => bookingSchema.parse(data))
  .handler(async ({ data }) => {
    const selectedSlot = bookingAvailability.find((slot) => slot.id === data.slotId)

    if (!selectedSlot) {
      throw new Error('The selected slot is no longer available.')
    }

    const { createGoogleCalendarInvite, createZoomMeeting } = await import('#/server/booking')

    const zoomJoinUrl = await createZoomMeeting({
      topic: data.topic,
      start: selectedSlot.start,
      end: selectedSlot.end,
      attendeeName: data.name,
      attendeeEmail: data.email,
    })

    await createGoogleCalendarInvite({
      slot: selectedSlot,
      attendee: data,
      zoomJoinUrl,
    })

    return { ok: true as const }
  })

export const Route = createFileRoute('/book')({
  component: BookPage,
})

function BookPage() {
  const [slotId, setSlotId] = useState(bookingAvailability[0]?.id ?? '')
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [company, setCompany] = useState('')
  const [topic, setTopic] = useState('')
  const [status, setStatus] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const timezone = useMemo(() => Intl.DateTimeFormat().resolvedOptions().timeZone, [])

  async function onSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setStatus(null)
    setIsSubmitting(true)

    try {
      await submitBooking({ data: { slotId, name, email, company, topic, timezone } })
      setStatus('Thanks — your request is in. You will receive a calendar invite with a Zoom link shortly.')
      setTopic('')
    } catch (error) {
      setStatus(error instanceof Error ? error.message : 'Could not submit your booking request.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="mx-auto max-w-3xl px-4 pb-16 pt-10 sm:px-6 lg:px-8">
      <section className="island-shell rounded-3xl px-6 py-8 sm:px-8 sm:py-10">
        <p className="island-kicker mb-3">Book time</p>
        <h1 className="display-title mb-3 text-3xl leading-tight font-semibold tracking-tight text-[var(--sea-ink)] sm:text-4xl">
          Schedule a Zoom call
        </h1>
        <p className="mb-6 max-w-2xl text-sm leading-relaxed text-[var(--sea-ink-soft)]">
          Pick a time and share context. This page creates a Zoom meeting and sends a Google Calendar invite directly, without adding a database to this site.
        </p>

        <form onSubmit={onSubmit} className="space-y-4">
          <label className="block text-sm font-medium text-[var(--sea-ink)]">
            Available time
            <select
              required
              value={slotId}
              onChange={(event) => setSlotId(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--line)] bg-[var(--surface)] px-3 py-2"
            >
              {bookingAvailability.map((slot) => (
                <option key={slot.id} value={slot.id}>
                  {new Date(slot.start).toLocaleString([], {
                    weekday: 'short',
                    month: 'short',
                    day: 'numeric',
                    hour: 'numeric',
                    minute: '2-digit',
                    timeZoneName: 'short',
                  })}
                </option>
              ))}
            </select>
          </label>

          <label className="block text-sm font-medium text-[var(--sea-ink)]">
            Name
            <input required value={name} onChange={(event) => setName(event.target.value)} className="mt-1 w-full rounded-md border border-[var(--line)] bg-[var(--surface)] px-3 py-2" />
          </label>

          <label className="block text-sm font-medium text-[var(--sea-ink)]">
            Email
            <input required type="email" value={email} onChange={(event) => setEmail(event.target.value)} className="mt-1 w-full rounded-md border border-[var(--line)] bg-[var(--surface)] px-3 py-2" />
          </label>

          <label className="block text-sm font-medium text-[var(--sea-ink)]">
            Company (optional)
            <input value={company} onChange={(event) => setCompany(event.target.value)} className="mt-1 w-full rounded-md border border-[var(--line)] bg-[var(--surface)] px-3 py-2" />
          </label>

          <label className="block text-sm font-medium text-[var(--sea-ink)]">
            What should we cover?
            <textarea required value={topic} onChange={(event) => setTopic(event.target.value)} className="mt-1 min-h-28 w-full rounded-md border border-[var(--line)] bg-[var(--surface)] px-3 py-2" />
          </label>

          <button
            type="submit"
            disabled={isSubmitting}
            className="inline-flex items-center rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.14)] px-4 py-2 text-sm font-semibold text-[var(--lagoon-deep)] transition hover:-translate-y-0.5 hover:bg-[rgba(79,184,178,0.24)] disabled:opacity-50"
          >
            {isSubmitting ? 'Submitting…' : 'Request meeting'}
          </button>

          {status ? <p className="text-sm text-[var(--sea-ink-soft)]">{status}</p> : null}
        </form>
      </section>
    </main>
  )
}
