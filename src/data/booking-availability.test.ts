import { describe, expect, it } from 'vitest'
import { getBookingAvailability } from './booking-availability'

describe('booking availability', () => {
  it('excludes same-day slots', () => {
    const slots = getBookingAvailability({
      now: new Date('2026-05-01T08:00:00-07:00'),
    })

    expect(slots.some((slot) => slot.start.startsWith('2026-05-01'))).toBe(
      false,
    )
    expect(slots[0].start).toBe('2026-05-04T15:30:00.000Z')
  })
})
