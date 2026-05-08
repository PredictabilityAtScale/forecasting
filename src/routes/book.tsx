import { createFileRoute, Outlet, useRouterState } from '@tanstack/react-router'
import { createServerFn } from '@tanstack/react-start'
import { CheckCircle2, ChevronLeft, ChevronRight, X } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { bookingDurationOptions } from '#/data/booking-availability'
import { bookingSchema } from '#/data/booking-schema'
import type {
  BookingDurationMinutes,
  BookingSlot,
} from '#/data/booking-availability'

const HOST_BOOKING_TIME_ZONE = 'America/Los_Angeles'
const DEFAULT_SLOT_AFTER_MINUTES = 10 * 60

type BookingConfirmation = {
  start: string
  end: string
  durationMinutes: BookingDurationMinutes
  name: string
  email: string
  company: string
  topic: string
  timezone: string
  website: string
}

const submitBooking = createServerFn({ method: 'POST' })
  .inputValidator((data: unknown) => bookingSchema.parse(data))
  .handler(async ({ data }) => {
    const { assertBookingRequestAllowed } = await import('#/server/booking-guard')
    assertBookingRequestAllowed(data)

    const { getOpenBookingSlots } = await import('#/server/booking')
    const openSlots = await getOpenBookingSlots(data.durationMinutes)
    const selectedSlot = openSlots.find((slot) => slot.id === data.slotId)

    if (!selectedSlot) {
      throw new Error('The selected slot is no longer available.')
    }

    const { createGoogleCalendarInvite, createZoomMeeting } =
      await import('#/server/booking')

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

const fetchOpenSlots = createServerFn({ method: 'POST' })
  .inputValidator((data: unknown) => {
    const value = data as { durationMinutes?: unknown }
    if (value.durationMinutes !== 30 && value.durationMinutes !== 60) {
      throw new Error('Meeting length must be 30 or 60 minutes.')
    }

    return { durationMinutes: value.durationMinutes }
  })
  .handler(async ({ data }) => {
    const { getOpenBookingSlots } = await import('#/server/booking')
    return getOpenBookingSlots(data.durationMinutes)
  })

export const Route = createFileRoute('/book')({
  component: BookRouteShell,
})

function BookRouteShell() {
  const pathname = useRouterState({
    select: (state) => state.location.pathname,
  })

  return pathname === '/book' ? <BookPage /> : <Outlet />
}

function BookPage() {
  const [durationMinutes, setDurationMinutes] =
    useState<BookingDurationMinutes>(30)
  const [slotId, setSlotId] = useState('')
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [company, setCompany] = useState('')
  const [topic, setTopic] = useState('')
  const [website, setWebsite] = useState('')
  const [status, setStatus] = useState<string | null>(null)
  const [confirmation, setConfirmation] = useState<BookingConfirmation | null>(
    null,
  )
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isLoadingSlots, setIsLoadingSlots] = useState(true)
  const [openSlots, setOpenSlots] = useState<BookingSlot[]>([])
  const [visibleWeekStart, setVisibleWeekStart] = useState(() =>
    startOfLocalWeek(new Date()),
  )

  const timezone = useMemo(
    () => Intl.DateTimeFormat().resolvedOptions().timeZone,
    [],
  )

  useEffect(() => {
    setIsLoadingSlots(true)
    setStatus(null)

    fetchOpenSlots({ data: { durationMinutes } })
      .then((slots) => {
        setOpenSlots(slots)
        setSlotId((current) =>
          slots.length > 0 && !slots.some((slot) => slot.id === current)
            ? getDefaultBookingSlot(slots).id
            : current,
        )

        if (slots.length > 0) {
          setVisibleWeekStart(startOfLocalWeek(new Date(slots[0].start)))
        }
      })
      .catch((error) => {
        console.error(error)
        setOpenSlots([])
        setSlotId('')
        setStatus(
          'Could not refresh live availability. Please contact us to coordinate manually.',
        )
      })
      .finally(() => {
        setIsLoadingSlots(false)
      })
  }, [durationMinutes])

  const visibleWeekEnd = useMemo(
    () => addCalendarDays(visibleWeekStart, 7),
    [visibleWeekStart],
  )

  const visibleSlots = useMemo(
    () =>
      openSlots.filter((slot) => {
        const slotStart = new Date(slot.start).getTime()
        return (
          slotStart >= visibleWeekStart.getTime() &&
          slotStart < visibleWeekEnd.getTime()
        )
      }),
    [openSlots, visibleWeekEnd, visibleWeekStart],
  )

  const visibleWeekLabel = useMemo(() => {
    const formatter = new Intl.DateTimeFormat([], {
      month: 'short',
      day: 'numeric',
      timeZone: timezone,
    })
    const rangeEnd = addCalendarDays(visibleWeekEnd, -1)

    return `${formatter.format(visibleWeekStart)} - ${formatter.format(rangeEnd)}`
  }, [timezone, visibleWeekEnd, visibleWeekStart])

  const groupedSlots = useMemo(() => {
    const formatter = new Intl.DateTimeFormat([], {
      weekday: 'short',
      month: 'short',
      day: 'numeric',
      timeZone: timezone,
    })

    return visibleSlots.reduce(
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
      [] as Array<{ dateKey: string; slots: typeof visibleSlots }>,
    )
  }, [timezone, visibleSlots])

  async function onSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setStatus(null)
    setIsSubmitting(true)
    const selectedSlot = openSlots.find((slot) => slot.id === slotId)

    try {
      await submitBooking({
        data: {
          slotId,
          durationMinutes,
          name,
          email,
          company,
          topic,
          timezone,
          website,
        },
      })
      if (selectedSlot) {
        setConfirmation({
          start: selectedSlot.start,
          end: selectedSlot.end,
          durationMinutes,
          name,
          email,
          company,
          topic,
          timezone,
          website,
        })
      }
      setName('')
      setEmail('')
      setCompany('')
      setTopic('')
      setWebsite('')
    } catch (error) {
      setStatus(
        error instanceof Error
          ? error.message
          : 'Could not submit your booking request.',
      )
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

        <form onSubmit={onSubmit} className="space-y-4">
          <div className="grid gap-4 sm:grid-cols-2">
            <label className="block text-sm font-medium text-[var(--sea-ink)]">
              Name
              <input
                required
                value={name}
                onChange={(event) => setName(event.target.value)}
                className="mt-1 w-full rounded-md border border-[var(--line)] bg-[var(--surface)] px-3 py-2"
              />
            </label>

            <label className="block text-sm font-medium text-[var(--sea-ink)]">
              Email
              <input
                required
                type="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                className="mt-1 w-full rounded-md border border-[var(--line)] bg-[var(--surface)] px-3 py-2"
              />
            </label>
          </div>

          <label className="block text-sm font-medium text-[var(--sea-ink)]">
            Company (optional)
            <input
              value={company}
              onChange={(event) => setCompany(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--line)] bg-[var(--surface)] px-3 py-2"
            />
          </label>

          <label className="block text-sm font-medium text-[var(--sea-ink)]">
            What should we cover? (optional)
            <textarea
              value={topic}
              onChange={(event) => setTopic(event.target.value)}
              className="mt-1 min-h-28 w-full rounded-md border border-[var(--line)] bg-[var(--surface)] px-3 py-2"
            />
          </label>

          <label className="sr-only" aria-hidden="true">
            Website
            <input
              tabIndex={-1}
              autoComplete="off"
              value={website}
              onChange={(event) => setWebsite(event.target.value)}
            />
          </label>

          <fieldset>
            <legend className="mb-2 block text-sm font-medium text-[var(--sea-ink)]">
              Meeting length
            </legend>
            <div className="inline-flex rounded-full border border-[var(--line)] bg-[var(--surface)] p-1">
              {bookingDurationOptions.map((option) => {
                const selected = option === durationMinutes
                return (
                  <button
                    key={option}
                    type="button"
                    onClick={() => setDurationMinutes(option)}
                    className={`rounded-full px-3 py-1.5 text-sm font-medium transition ${
                      selected
                        ? 'bg-[rgba(79,184,178,0.2)] text-[var(--lagoon-deep)]'
                        : 'text-[var(--sea-ink-soft)] hover:text-[var(--lagoon-deep)]'
                    }`}
                  >
                    {option} min
                  </button>
                )
              })}
            </div>
          </fieldset>

          <button
            type="submit"
            disabled={isSubmitting || isLoadingSlots || !slotId}
            className="inline-flex items-center rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.14)] px-4 py-2 text-sm font-semibold text-[var(--lagoon-deep)] transition hover:-translate-y-0.5 hover:bg-[rgba(79,184,178,0.24)] disabled:opacity-50"
          >
            {isSubmitting
              ? 'Submitting…'
              : 'Select a time below and then click here to book'}
          </button>

          <fieldset>
            <legend className="mb-2 block text-sm font-medium text-[var(--sea-ink)]">
              Available times
            </legend>
            <p className="mb-3 text-xs text-[var(--sea-ink-soft)]">
              Times are shown in your local timezone:{' '}
              <span className="font-semibold">{timezone}</span>
            </p>
            <div className="mb-3 grid grid-cols-[auto_1fr_auto] items-center gap-2">
              <button
                type="button"
                aria-label="Previous week"
                onClick={() =>
                  setVisibleWeekStart((current) =>
                    startOfLocalWeek(addCalendarDays(current, -7)),
                  )
                }
                className="flex size-8 items-center justify-center rounded-full border border-[var(--line)] text-lg leading-none text-[var(--sea-ink-soft)] transition hover:border-[rgba(50,143,151,0.35)] hover:text-[var(--lagoon-deep)]"
              >
                <ChevronLeft className="size-4" aria-hidden="true" />
              </button>
              <span className="text-center text-xs font-medium text-[var(--sea-ink-soft)]">
                {visibleWeekLabel}
              </span>
              <button
                type="button"
                aria-label="Next week"
                onClick={() =>
                  setVisibleWeekStart((current) =>
                    startOfLocalWeek(addCalendarDays(current, 7)),
                  )
                }
                className="flex size-8 items-center justify-center rounded-full border border-[var(--line)] text-lg leading-none text-[var(--sea-ink-soft)] transition hover:border-[rgba(50,143,151,0.35)] hover:text-[var(--lagoon-deep)]"
              >
                <ChevronRight className="size-4" aria-hidden="true" />
              </button>
            </div>
            <div className="grid gap-3 md:grid-cols-2">
              {isLoadingSlots ? (
                <p className="text-sm text-[var(--sea-ink-soft)]">
                  Loading live availability…
                </p>
              ) : null}
              {!isLoadingSlots && openSlots.length === 0 ? (
                <p className="text-sm text-[var(--sea-ink-soft)]">
                  No live availability is currently available.
                </p>
              ) : null}
              {!isLoadingSlots &&
              openSlots.length > 0 &&
              visibleSlots.length === 0 ? (
                <p className="text-sm text-[var(--sea-ink-soft)]">
                  No open slots in this week.
                </p>
              ) : null}
              {groupedSlots.map((group) => (
                <div
                  key={group.dateKey}
                  className="rounded-xl border border-[var(--line)] bg-[var(--surface)] p-3"
                >
                  <p className="mb-2 text-sm font-semibold text-[var(--sea-ink)]">
                    {group.dateKey}
                  </p>
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
          </fieldset>

          <p className="text-xs text-[var(--sea-ink-soft)]">
            Urgent or times don't work for you? Email me directly at{' '}
            <a
              href="mailto:troy.magennis@focusedobjective.com"
              className="font-semibold text-[var(--lagoon-deep)] underline-offset-4 hover:underline"
            >
              troy.magennis@focusedobjective.com
            </a>
            .
          </p>

          <button
            type="submit"
            disabled={isSubmitting || isLoadingSlots || !slotId}
            className="inline-flex items-center rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.14)] px-4 py-2 text-sm font-semibold text-[var(--lagoon-deep)] transition hover:-translate-y-0.5 hover:bg-[rgba(79,184,178,0.24)] disabled:opacity-50"
          >
            {isSubmitting ? 'Submitting…' : 'Book meeting'}
          </button>

          {status ? (
            <p className="text-sm text-[var(--sea-ink-soft)]">{status}</p>
          ) : null}
        </form>
      </section>
      {confirmation ? (
        <BookingConfirmationDialog
          confirmation={confirmation}
          onClose={() => setConfirmation(null)}
        />
      ) : null}
    </main>
  )
}

function BookingConfirmationDialog({
  confirmation,
  onClose,
}: {
  confirmation: BookingConfirmation
  onClose: () => void
}) {
  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="booking-confirmation-title"
      className="fixed inset-0 z-50 flex items-center justify-center bg-[rgba(16,38,39,0.55)] px-4 py-6"
    >
      <div className="w-full max-w-lg rounded-2xl border border-[rgba(255,255,255,0.55)] bg-[var(--surface)] p-6 shadow-2xl">
        <div className="mb-4 flex items-start justify-between gap-4">
          <div className="flex items-start gap-3">
            <CheckCircle2
              className="mt-1 size-7 shrink-0 text-[var(--lagoon-deep)]"
              aria-hidden="true"
            />
            <div>
              <p className="island-kicker mb-1">Booked</p>
              <h2
                id="booking-confirmation-title"
                className="display-title text-2xl font-semibold text-[var(--sea-ink)]"
              >
                Your meeting is confirmed
              </h2>
            </div>
          </div>
          <button
            type="button"
            onClick={onClose}
            aria-label="Close confirmation"
            className="flex size-9 shrink-0 items-center justify-center rounded-full border border-[var(--line)] text-[var(--sea-ink-soft)] transition hover:border-[rgba(50,143,151,0.35)] hover:text-[var(--lagoon-deep)]"
          >
            <X className="size-4" aria-hidden="true" />
          </button>
        </div>

        <dl className="space-y-3 text-sm">
          <div>
            <dt className="font-semibold text-[var(--sea-ink)]">When</dt>
            <dd className="text-[var(--sea-ink-soft)]">
              {formatSlotRange(confirmation)}
            </dd>
          </div>
          <div>
            <dt className="font-semibold text-[var(--sea-ink)]">
              Invite sent to
            </dt>
            <dd className="break-words text-[var(--sea-ink-soft)]">
              {confirmation.name} &lt;{confirmation.email}&gt;
            </dd>
          </div>
          {confirmation.company ? (
            <div>
              <dt className="font-semibold text-[var(--sea-ink)]">Company</dt>
              <dd className="text-[var(--sea-ink-soft)]">
                {confirmation.company}
              </dd>
            </div>
          ) : null}
          {confirmation.topic.trim() ? (
            <div>
              <dt className="font-semibold text-[var(--sea-ink)]">Topic</dt>
              <dd className="whitespace-pre-wrap text-[var(--sea-ink-soft)]">
                {confirmation.topic}
              </dd>
            </div>
          ) : null}
        </dl>

        <p className="mt-5 text-sm text-[var(--sea-ink-soft)]">
          Check your calendar invite for the Zoom link and the direct cancel or
          reschedule links.
        </p>

        <button
          type="button"
          onClick={onClose}
          className="mt-5 inline-flex items-center rounded-full border border-[rgba(50,143,151,0.3)] bg-[rgba(79,184,178,0.18)] px-4 py-2 text-sm font-semibold text-[var(--lagoon-deep)] transition hover:bg-[rgba(79,184,178,0.28)]"
        >
          Done
        </button>
      </div>
    </div>
  )
}

function formatSlotRange({
  start,
  end,
  timezone,
  durationMinutes,
}: BookingConfirmation) {
  const dateFormatter = new Intl.DateTimeFormat([], {
    weekday: 'long',
    month: 'long',
    day: 'numeric',
    timeZone: timezone,
  })
  const timeFormatter = new Intl.DateTimeFormat([], {
    hour: 'numeric',
    minute: '2-digit',
    timeZone: timezone,
  })

  return `${dateFormatter.format(new Date(start))}, ${timeFormatter.format(
    new Date(start),
  )} - ${timeFormatter.format(new Date(end))} (${durationMinutes} min, ${timezone})`
}

function startOfLocalWeek(date: Date) {
  const next = new Date(date)
  next.setHours(0, 0, 0, 0)
  const day = next.getDay()
  next.setDate(next.getDate() + (day === 0 ? -6 : 1 - day))
  return next
}

function addCalendarDays(date: Date, days: number) {
  const next = new Date(date)
  next.setDate(next.getDate() + days)
  return next
}

function getDefaultBookingSlot(slots: BookingSlot[]) {
  return (
    slots.find(
      (slot) =>
        getSlotStartMinutes(slot, HOST_BOOKING_TIME_ZONE) >
        DEFAULT_SLOT_AFTER_MINUTES,
    ) ?? slots[0]
  )
}

function getSlotStartMinutes(slot: BookingSlot, timeZone: string) {
  const formatter = new Intl.DateTimeFormat('en-US', {
    hour: '2-digit',
    minute: '2-digit',
    hourCycle: 'h23',
    timeZone,
  })
  const parts = Object.fromEntries(
    formatter
      .formatToParts(new Date(slot.start))
      .map((part) => [part.type, part.value]),
  )

  return Number(parts.hour) * 60 + Number(parts.minute)
}
