import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createBookingLinkToken } from './booking-links'

vi.mock('node:fs', () => ({
  existsSync: (path: string) => path.endsWith('.env.local'),
  readFileSync: () => 'BOOKING_LINK_SECRET=existing-booking-secret',
}))

describe('booking management environment loading', () => {
  beforeEach(() => {
    vi.resetModules()
  })

  afterEach(() => {
    vi.unstubAllEnvs()
  })

  it.each(['cancel', 'reschedule'] as const)(
    'loads the existing secret from an env file before verifying a %s link',
    async (action) => {
      vi.stubEnv('BOOKING_LINK_SECRET', 'existing-booking-secret')
      const token = createBookingLinkToken({
        purpose: 'unexpected-purpose',
        issuedAt: new Date().toISOString(),
        email: 'customer@example.com',
        googleEventId: 'event-id',
        zoomMeetingId: '123456789',
        expiresAt: '2099-01-01T00:00:00.000Z',
      })
      vi.stubEnv('BOOKING_LINK_SECRET', undefined)
      const { cancelBookingFromToken, rescheduleBookingFromToken } =
        await import('./booking')

      const result =
        action === 'cancel'
          ? cancelBookingFromToken(token)
          : rescheduleBookingFromToken(token, {
              start: '2026-10-07T18:30:00.000Z',
              end: '2026-10-07T19:00:00.000Z',
            })

      // Purpose validation happens only after the signature was verified.
      await expect(result).rejects.toThrow(
        'Booking link token purpose mismatch.',
      )
      expect(process.env.BOOKING_LINK_SECRET).toBe('existing-booking-secret')
    },
  )
})
