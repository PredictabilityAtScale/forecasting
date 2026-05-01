import { createHmac, timingSafeEqual } from 'node:crypto'

export type BookingLinkPayload = {
  purpose: string
  issuedAt: string
  email: string
  googleEventId: string
  zoomMeetingId: string
  expiresAt: string
}

export function createBookingLinkToken(payload: BookingLinkPayload) {
  const json = JSON.stringify(payload)
  const body = Buffer.from(json).toString('base64url')
  const sig = sign(body)
  return `${body}.${sig}`
}

export function parseBookingLinkToken(token: string): BookingLinkPayload {
  const [body, signature] = token.split('.')
  if (!body || !signature) {
    throw new Error('Invalid booking link token.')
  }

  const expected = sign(body)
  const signatureBuffer = Buffer.from(signature)
  const expectedBuffer = Buffer.from(expected)

  if (
    signatureBuffer.length !== expectedBuffer.length ||
    !timingSafeEqual(signatureBuffer, expectedBuffer)
  ) {
    throw new Error('Booking link token signature mismatch.')
  }

  const payload = JSON.parse(Buffer.from(body, 'base64url').toString('utf8')) as BookingLinkPayload
  if (payload.purpose !== 'booking-manage-v1') {
    throw new Error('Booking link token purpose mismatch.')
  }

  if (!payload.issuedAt) {
    throw new Error('Booking link token missing issued at.')
  }

  if (!payload.expiresAt) {
    throw new Error('Booking link token missing expiry.')
  }

  if (Date.now() > new Date(payload.expiresAt).getTime()) {
    throw new Error('This booking link has expired because the meeting time has passed.')
  }

  return payload
}

function sign(body: string) {
  const secret = process.env.BOOKING_LINK_SECRET
  if (!secret) {
    throw new Error('BOOKING_LINK_SECRET is required for booking action links.')
  }

  return createHmac('sha256', secret).update(body).digest('base64url')
}
