export type BookingSlot = {
  id: string
  start: string
  end: string
}

// Keep this list in UTC so daylight savings and attendee locale conversion remain predictable.
export const bookingAvailability: BookingSlot[] = [
  { id: '2026-05-04T15:00:00Z', start: '2026-05-04T15:00:00Z', end: '2026-05-04T15:30:00Z' },
  { id: '2026-05-04T16:00:00Z', start: '2026-05-04T16:00:00Z', end: '2026-05-04T16:30:00Z' },
  { id: '2026-05-05T15:00:00Z', start: '2026-05-05T15:00:00Z', end: '2026-05-05T15:30:00Z' },
  { id: '2026-05-05T16:00:00Z', start: '2026-05-05T16:00:00Z', end: '2026-05-05T16:30:00Z' },
  { id: '2026-05-06T15:00:00Z', start: '2026-05-06T15:00:00Z', end: '2026-05-06T15:30:00Z' },
  { id: '2026-05-06T16:00:00Z', start: '2026-05-06T16:00:00Z', end: '2026-05-06T16:30:00Z' },
]
