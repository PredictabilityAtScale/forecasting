import { createFileRoute } from '@tanstack/react-router'
import { createServerFn } from '@tanstack/react-start'
import { useEffect, useMemo, useState } from 'react'
import { bookingAvailability } from '#/data/booking-availability'
import { bookingSchema } from '#/data/booking-schema'

const submitBooking = createServerFn({ method: 'POST' })
  .inputValidator((data: unknown) => bookingSchema.parse(data))
  .handler(async ({ data }) => {
    const { getOpenBookingSlots } = await import('#/server/booking')
    const openSlots = await getOpenBookingSlots()
    const selectedSlot = openSlots.find((slot) => slot.id === data.slotId)

    if (!selectedSlot) {
      throw new Error('The selected slot is no longer available.')
    }

    const { createGoogleCalendarInvite, createZoomMeeting } = await import('#/server/booking')

    const zoomMeeting = await createZoomMeeting({
      topic: data.topic,
      start: selectedSlot.start,
      end: selectedSlot.end,
      attendeeName: data.name,
      attendeeEmail: data.email,
    })

    await createGoogleCalendarInvite({
      slot: selectedSlot,
      attendee: data,
      zoomJoinUrl: zoomMeeting.joinUrl,
      zoomMeetingId: zoomMeeting.meetingId,
    })

    return { ok: true as const }
  })

const fetchOpenSlots = createServerFn({ method: 'GET' }).handler(async () => {
  const { getOpenBookingSlots } = await import('#/server/booking')
  return getOpenBookingSlots()
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
  const [openSlots, setOpenSlots] = useState(bookingAvailability)

  const timezone = useMemo(() => Intl.DateTimeFormat().resolvedOptions().timeZone, [])

  useEffect(() => {
    fetchOpenSlots()
      .then((slots) => {
        setOpenSlots(slots)
        setSlotId((current) =>
          slots.length > 0 && !slots.some((slot) => slot.id === current) ? slots[0].id : current,
        )
      })
      .catch(() => {
        setStatus('Could not refresh live availability. Showing preset times.')
      })
  }, [])

  const groupedSlots = useMemo(() => {
    const formatter = new Intl.DateTimeFormat([], {
      weekday: 'short',
      month: 'short',
      day: 'numeric',
      timeZone: timezone,
    })

    return openSlots.reduce(
      (groups, slot) => {
        const dateKey = formatter.format(new Date(slot.start))
        const existingGroup = groups.find((group) => group.dateKey === dateKey)

        if (existingGroup) {
          existingGroup.slots.push(slot)
        } else {
          groups.push({ dateKey, slots: [slot] })
        }

        return groups
      },
      [] as Array<{ dateKey: string; slots: typeof openSlots }>,
    )
  }, [openSlots, timezone])

  async function onSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setStatus(null)
    setIsSubmitting(true)

    try {
      await submitBooking({ data: { slotId, name, email, company, topic, timezone } })
      setStatus(
        'Thanks — your meeting is booked. Check the invite for direct cancel/reschedule links and the Zoom join URL.',
      )
      setTopic('')
    } catch (error) {
      setStatus(error instanceof Error ? error.message : 'Could not submit your booking request.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="mx-auto max-w-4xl px-4 pb-16 pt-10 sm:px-6 lg:px-8">
      <section className="island-shell rounded-3xl px-6 py-8 sm:px-8 sm:py-10">
        <p className="island-kicker mb-3">Book time</p>
        <h1 className="display-title mb-3 text-3xl leading-tight font-semibold tracking-tight text-[var(--sea-ink)] sm:text-4xl">
          Schedule a Zoom call
        </h1>
        <p className="mb-2 max-w-2xl text-sm leading-relaxed text-[var(--sea-ink-soft)]">
          Pick a slot from a calendar-style grid and share context. This page creates a Zoom meeting and a Google Calendar invite without adding a database.
        </p>
        <p className="mb-6 text-xs text-[var(--sea-ink-soft)]">
          Times are shown in your local timezone: <span className="font-semibold">{timezone}</span>
        </p>

        <form onSubmit={onSubmit} className="space-y-4">
          <fieldset>
            <legend className="mb-2 block text-sm font-medium text-[var(--sea-ink)]">Available times</legend>
            <div className="grid gap-3 md:grid-cols-2">
              {groupedSlots.map((group) => (
                <div
                  key={group.dateKey}
                  className="rounded-xl border border-[var(--line)] bg-[var(--surface)] p-3"
                >
                  <p className="mb-2 text-sm font-semibold text-[var(--sea-ink)]">{group.dateKey}</p>
                  <div className="flex flex-wrap gap-2">
                    {group.slots.map((slot) => {
                      const selected = slot.id === slotId
                      return (
                        <button
                          key={slot.id}
                          type="button"
                          onClick={() => setSlotId(slot.id)}
                          className={`rounded-full border px-3 py-1.5 text-sm transition ${
                            selected
                              ? 'border-[rgba(50,143,151,0.7)] bg-[rgba(79,184,178,0.2)] text-[var(--lagoon-deep)]'
                              : 'border-[var(--line)] text-[var(--sea-ink-soft)] hover:border-[rgba(50,143,151,0.35)] hover:text-[var(--lagoon-deep)]'
                          }`}
                        >
                          {new Date(slot.start).toLocaleTimeString([], {
                            hour: 'numeric',
                            minute: '2-digit',
                            timeZone: timezone,
                          })}
                        </button>
                      )
                    })}
                  </div>
                </div>
              ))}
            </div>
            <p className="mt-2 text-xs text-[var(--sea-ink-soft)]">
              Selected:{' '}
              {slotId
                ? new Date(slotId).toLocaleString([], {
                    weekday: 'short',
                    month: 'short',
                    day: 'numeric',
                    hour: 'numeric',
                    minute: '2-digit',
                    timeZoneName: 'short',
                    timeZone: timezone,
                  })
                : 'Pick a time to continue'}
            </p>
          </fieldset>

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
            disabled={isSubmitting || !slotId}
            className="inline-flex items-center rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.14)] px-4 py-2 text-sm font-semibold text-[var(--lagoon-deep)] transition hover:-translate-y-0.5 hover:bg-[rgba(79,184,178,0.24)] disabled:opacity-50"
          >
            {isSubmitting ? 'Submitting…' : 'Book meeting'}
          </button>

          {status ? <p className="text-sm text-[var(--sea-ink-soft)]">{status}</p> : null}
        </form>
      </section>
    </main>
  )
}
