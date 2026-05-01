import { createFileRoute } from '@tanstack/react-router'
import { createServerFn } from '@tanstack/react-start'
import { useEffect, useMemo, useState } from 'react'
import { bookingAvailability } from '#/data/booking-availability'

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
    const value = data as { token?: string; slotId?: string }
    if (!value.token || !value.slotId) throw new Error('Missing token or slot.')
    return { token: value.token, slotId: value.slotId }
  })
  .handler(async ({ data }) => {
    const slot = bookingAvailability.find((item) => item.id === data.slotId)
    if (!slot) throw new Error('Selected slot is no longer available.')
    const { rescheduleBookingFromToken } = await import('#/server/booking')
    await rescheduleBookingFromToken(data.token, { start: slot.start, end: slot.end })
    return { ok: true as const }
  })

const fetchOpenSlots = createServerFn({ method: 'GET' }).handler(async () => {
  const { getOpenBookingSlots } = await import('#/server/booking')
  return getOpenBookingSlots()
})

export const Route = createFileRoute('/book/manage')({
  validateSearch: (search: Record<string, unknown>) => ({
    action:
      search.action === 'cancel' || search.action === 'reschedule' ? search.action : undefined,
    token: typeof search.token === 'string' ? search.token : undefined,
  }),
  component: ManageBookingPage,
})

function ManageBookingPage() {
  const search = Route.useSearch()
  const action = search.action
  const token = search.token ?? ''
  const [slotId, setSlotId] = useState(bookingAvailability[0]?.id ?? '')
  const [status, setStatus] = useState<string | null>(null)
  const [openSlots, setOpenSlots] = useState(bookingAvailability)
  const [isLoadingSlots, setIsLoadingSlots] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)

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
        setStatus('Could not refresh availability. Please try again.')
      })
      .finally(() => {
        setIsLoadingSlots(false)
      })
  }, [])

  async function onCancel() {
    try {
      setIsSubmitting(true)
      await cancelBooking({ data: { token } })
      setStatus('Your meeting has been cancelled and notifications were sent.')
    } catch (error) {
      setStatus(error instanceof Error ? error.message : 'Unable to cancel meeting.')
    } finally {
      setIsSubmitting(false)
    }
  }

  async function onReschedule() {
    try {
      setIsSubmitting(true)
      await rescheduleBooking({ data: { token, slotId } })
      setStatus('Your meeting has been rescheduled and updated invites were sent.')
    } catch (error) {
      setStatus(error instanceof Error ? error.message : 'Unable to reschedule meeting.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="mx-auto max-w-3xl px-4 pb-16 pt-10 sm:px-6 lg:px-8">
      <section className="island-shell rounded-3xl px-6 py-8 sm:px-8 sm:py-10">
        <h1 className="display-title mb-4 text-3xl font-semibold text-[var(--sea-ink)]">Manage booking</h1>
        {!token ? <p className="text-sm">Missing booking token.</p> : null}
        {action !== 'cancel' && action !== 'reschedule' ? (
          <p className="mb-4 text-sm text-[var(--sea-ink-soft)]">
            Unknown action. Please use the cancel/reschedule link from your calendar invite.
          </p>
        ) : null}

        {action === 'cancel' ? (
          <button onClick={onCancel} className="rounded-full border px-4 py-2" disabled={!token || isSubmitting}>
            {isSubmitting ? 'Cancelling…' : 'Confirm cancellation'}
          </button>
        ) : null}

        {action === 'reschedule' ? (
          <div className="space-y-3">
            <label className="block text-sm font-medium">
              Pick a new time ({timezone})
              <select value={slotId} onChange={(event) => setSlotId(event.target.value)} className="mt-1 w-full rounded-md border px-3 py-2" disabled={isLoadingSlots || openSlots.length === 0 || isSubmitting}>
                {openSlots.map((slot) => (
                  <option key={slot.id} value={slot.id}>
                    {new Date(slot.start).toLocaleString([], { weekday: 'short', month: 'short', day: 'numeric', hour: 'numeric', minute: '2-digit', timeZone, timeZoneName: 'short' })}
                  </option>
                ))}
              </select>
            </label>
            {isLoadingSlots ? <p className="text-xs text-[var(--sea-ink-soft)]">Loading live availability…</p> : null}
            {!isLoadingSlots && openSlots.length === 0 ? (
              <p className="text-xs text-[var(--sea-ink-soft)]">No open slots found. Please contact us to coordinate manually.</p>
            ) : null}
            <button onClick={onReschedule} className="rounded-full border px-4 py-2" disabled={!token || isSubmitting || openSlots.length === 0}>
              {isSubmitting ? 'Rescheduling…' : 'Confirm reschedule'}
            </button>
          </div>
        ) : null}

        {status ? <p className="mt-4 text-sm">{status}</p> : null}
      </section>
    </main>
  )
}
