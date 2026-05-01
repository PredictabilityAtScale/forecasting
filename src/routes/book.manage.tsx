import { createFileRoute } from '@tanstack/react-router'
import { createServerFn } from '@tanstack/react-start'
import { useEffect, useMemo, useState } from 'react'
import { bookingDurationOptions } from '#/data/booking-availability'
import type {
  BookingDurationMinutes,
  BookingSlot,
} from '#/data/booking-availability'

const cancelBooking = createServerFn({ method: 'POST' })
  .inputValidator((data: unknown) => {
    const value = data as { token?: string }
    if (!value.token) throw new Error('Missing token.')
    return { token: value.token }
  })
  .handler(async ({ data }) => {
    const { cancelBookingFromToken } = await import('#/server/booking')
    await cancelBookingFromToken(data.token)
    return { ok: true as const }
  })

const rescheduleBooking = createServerFn({ method: 'POST' })
  .inputValidator((data: unknown) => {
    const value = data as {
      token?: string
      slotId?: string
      durationMinutes?: unknown
    }
    if (!value.token || !value.slotId) throw new Error('Missing token or slot.')
    if (value.durationMinutes !== 30 && value.durationMinutes !== 60) {
      throw new Error('Meeting length must be 30 or 60 minutes.')
    }

    return {
      token: value.token,
      slotId: value.slotId,
      durationMinutes: value.durationMinutes,
    }
  })
  .handler(async ({ data }) => {
    const { getBookingAvailability: getCurrentBookingAvailability } =
      await import('#/data/booking-availability')
    const slot = getCurrentBookingAvailability({
      durationMinutes: data.durationMinutes,
    }).find((item) => item.id === data.slotId)
    if (!slot) throw new Error('Selected slot is no longer available.')
    const { rescheduleBookingFromToken } = await import('#/server/booking')
    await rescheduleBookingFromToken(data.token, {
      start: slot.start,
      end: slot.end,
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

export const Route = createFileRoute('/book/manage')({
  validateSearch: (search: Record<string, unknown>) => ({
    action:
      search.action === 'cancel' || search.action === 'reschedule'
        ? search.action
        : undefined,
    token: typeof search.token === 'string' ? search.token : undefined,
  }),
  component: ManageBookingPage,
})

function ManageBookingPage() {
  const search = Route.useSearch()
  const action = search.action
  const token = search.token ?? ''
  const [durationMinutes, setDurationMinutes] =
    useState<BookingDurationMinutes>(30)
  const [slotId, setSlotId] = useState('')
  const [status, setStatus] = useState<string | null>(null)
  const [openSlots, setOpenSlots] = useState<BookingSlot[]>([])
  const [isLoadingSlots, setIsLoadingSlots] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)

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
            ? slots[0].id
            : current,
        )
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

  async function onCancel() {
    try {
      setIsSubmitting(true)
      await cancelBooking({ data: { token } })
      setStatus('Your meeting has been cancelled and notifications were sent.')
    } catch (error) {
      setStatus(
        error instanceof Error ? error.message : 'Unable to cancel meeting.',
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  async function onReschedule() {
    try {
      setIsSubmitting(true)
      await rescheduleBooking({ data: { token, slotId, durationMinutes } })
      setStatus(
        'Your meeting has been rescheduled and updated invites were sent.',
      )
    } catch (error) {
      setStatus(
        error instanceof Error
          ? error.message
          : 'Unable to reschedule meeting.',
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="mx-auto max-w-3xl px-4 pb-16 pt-10 sm:px-6 lg:px-8">
      <section className="island-shell rounded-3xl px-6 py-8 sm:px-8 sm:py-10">
        <h1 className="display-title mb-4 text-3xl font-semibold text-[var(--sea-ink)]">
          Manage booking
        </h1>
        {!token ? <p className="text-sm">Missing booking token.</p> : null}
        {action !== 'cancel' && action !== 'reschedule' ? (
          <p className="mb-4 text-sm text-[var(--sea-ink-soft)]">
            Unknown action. Please use the cancel/reschedule link from your
            calendar invite.
          </p>
        ) : null}

        {action === 'cancel' ? (
          <button
            onClick={onCancel}
            className="rounded-full border px-4 py-2"
            disabled={!token || isSubmitting}
          >
            {isSubmitting ? 'Cancelling…' : 'Confirm cancellation'}
          </button>
        ) : null}

        {action === 'reschedule' ? (
          <div className="space-y-3">
            <fieldset>
              <legend className="mb-2 block text-sm font-medium">
                Meeting length
              </legend>
              <div className="inline-flex rounded-full border border-[var(--line)] p-1">
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
                      disabled={isSubmitting}
                    >
                      {option} min
                    </button>
                  )
                })}
              </div>
            </fieldset>
            <label className="block text-sm font-medium">
              Pick a new time ({timezone})
              <select
                value={slotId}
                onChange={(event) => setSlotId(event.target.value)}
                className="mt-1 w-full rounded-md border px-3 py-2"
                disabled={
                  isLoadingSlots || openSlots.length === 0 || isSubmitting
                }
              >
                {openSlots.map((slot) => (
                  <option key={slot.id} value={slot.id}>
                    {new Date(slot.start).toLocaleString([], {
                      weekday: 'short',
                      month: 'short',
                      day: 'numeric',
                      hour: 'numeric',
                      minute: '2-digit',
                      timeZone,
                      timeZoneName: 'short',
                    })}
                  </option>
                ))}
              </select>
            </label>
            {isLoadingSlots ? (
              <p className="text-xs text-[var(--sea-ink-soft)]">
                Loading live availability…
              </p>
            ) : null}
            {!isLoadingSlots && openSlots.length === 0 ? (
              <p className="text-xs text-[var(--sea-ink-soft)]">
                No open slots found. Please contact us to coordinate manually.
              </p>
            ) : null}
            <button
              onClick={onReschedule}
              className="rounded-full border px-4 py-2"
              disabled={!token || isSubmitting || openSlots.length === 0}
            >
              {isSubmitting ? 'Rescheduling…' : 'Confirm reschedule'}
            </button>
          </div>
        ) : null}

        {status ? <p className="mt-4 text-sm">{status}</p> : null}
      </section>
    </main>
  )
}
