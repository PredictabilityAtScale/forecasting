import { afterEach, describe, expect, it, vi } from 'vitest'
import { bookingSchema } from '#/data/booking-schema'
import { assertBookingRequestAllowed } from './booking-guard'

const baseBooking = {
  slotId: '2026-05-11T16:00:00.000Z',
  durationMinutes: 30,
  name: 'Troy Magennis',
  email: 'troy@example.com',
  company: 'Focused Objective',
  topic: 'Discuss Monte Carlo forecasting adoption',
  timezone: 'America/Los_Angeles',
} as const

describe('booking spam guard', () => {
  afterEach(() => {
    vi.useRealTimers()
    delete process.env.BOOKING_BLOCKED_EMAILS
    delete process.env.BOOKING_BLOCKED_EMAIL_DOMAINS
    delete process.env.BOOKING_BLOCK_FREE_EMAILS
    delete process.env.BOOKING_ALLOWED_FREE_EMAIL_DOMAINS
    delete process.env.BOOKING_FREE_EMAIL_DOMAINS
  })

  it('allows normal booking requests', () => {
    expect(() =>
      assertBookingRequestAllowed(bookingSchema.parse(baseBooking)),
    ).not.toThrow()
  })

  it('rejects honeypot submissions', () => {
    expect(() =>
      assertBookingRequestAllowed(
        bookingSchema.parse({
          ...baseBooking,
          website: 'https://spam.example',
        }),
      ),
    ).toThrow('We could not accept this booking request.')
  })

  it('rejects random-looking booking text', () => {
    expect(() =>
      assertBookingRequestAllowed(
        bookingSchema.parse({
          ...baseBooking,
          name: 'APeZSZFwuItFNcaOugB',
          company: 'IITsEwkrfyktWnOk',
          topic: 'mLPTccgyjlatCedyoU',
        }),
      ),
    ).toThrow('We could not accept this booking request.')
  })

  it('rejects configured blocked emails and domains', () => {
    process.env.BOOKING_BLOCKED_EMAILS = 'blocked@example.com'
    process.env.BOOKING_BLOCKED_EMAIL_DOMAINS = 'spam.test'

    expect(() =>
      assertBookingRequestAllowed(
        bookingSchema.parse({
          ...baseBooking,
          email: 'blocked@example.com',
        }),
      ),
    ).toThrow('We could not accept this booking request.')

    expect(() =>
      assertBookingRequestAllowed(
        bookingSchema.parse({
          ...baseBooking,
          email: 'person@spam.test',
        }),
      ),
    ).toThrow('We could not accept this booking request.')
  })

  it('rejects free email domains by default', () => {
    expect(() =>
      assertBookingRequestAllowed(
        bookingSchema.parse({
          ...baseBooking,
          email: 'caclifton2002@yahoo.com',
        }),
      ),
    ).toThrow('Please email troy.magennis@focusedobjective.com directly.')
  })

  it('allows outlook.com booking email addresses', () => {
    expect(() =>
      assertBookingRequestAllowed(
        bookingSchema.parse({
          ...baseBooking,
          email: 'person@outlook.com',
        }),
      ),
    ).not.toThrow()
  })

  it('can disable or override free email blocking', () => {
    process.env.BOOKING_ALLOWED_FREE_EMAIL_DOMAINS = 'yahoo.com'

    expect(() =>
      assertBookingRequestAllowed(
        bookingSchema.parse({
          ...baseBooking,
          email: 'legit@yahoo.com',
        }),
      ),
    ).not.toThrow()

    process.env.BOOKING_BLOCK_FREE_EMAILS = 'false'

    expect(() =>
      assertBookingRequestAllowed(
        bookingSchema.parse({
          ...baseBooking,
          email: 'person@gmail.com',
        }),
      ),
    ).not.toThrow()
  })

  it('can add more free email domains from configuration', () => {
    process.env.BOOKING_FREE_EMAIL_DOMAINS = 'consumer.test'

    expect(() =>
      assertBookingRequestAllowed(
        bookingSchema.parse({
          ...baseBooking,
          email: 'person@consumer.test',
        }),
      ),
    ).toThrow('We could not accept this booking request.')
  })

  it('rate limits repeated attempts from the same email', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-05-08T12:00:00.000Z'))

    const request = bookingSchema.parse({
      ...baseBooking,
      email: 'repeat@example.com',
    })

    assertBookingRequestAllowed(request)
    assertBookingRequestAllowed(request)

    expect(() => assertBookingRequestAllowed(request)).toThrow(
      'Too many booking attempts.',
    )
  })
})
