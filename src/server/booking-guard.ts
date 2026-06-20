import type { BookingRequest } from '#/data/booking-schema'

const BOOKING_RATE_LIMIT_WINDOW_MS = 15 * 60 * 1000
const BOOKING_RATE_LIMIT_MAX = 2
const recentBookingAttempts = new Map<string, number[]>()
const FREE_EMAIL_DOMAINS = new Set([
  'aol.com',
  'comcast.net',
  'fastmail.com',
  // 'gmail.com',
  'googlemail.com',
  'hotmail.co.uk',
  'hotmail.com',
  // 'icloud.com',
  'live.com',
  'mail.com',
  'me.com',
  'msn.com',
  'pm.me',
  'proton.me',
  'protonmail.com',
  'yahoo.co.uk',
  'yahoo.com',
  'ymail.com',
])

type BookingGuardRequest = BookingRequest & {
  website?: string
}

export function assertBookingRequestAllowed(
  request: BookingGuardRequest,
  now = new Date(),
) {
  if (request.website?.trim()) {
    throwRejectedBooking()
  }

  const email = request.email.toLowerCase()
  if (isBlockedEmail(email) || isFreeEmail(email)) {
    throwRejectedBooking()
  }

  if (getSpamScore(request) >= 3) {
    throwRejectedBooking()
  }

  recordBookingAttempt(email, now.getTime())
}

function recordBookingAttempt(email: string, now: number) {
  const cutoff = now - BOOKING_RATE_LIMIT_WINDOW_MS
  const attempts = recentBookingAttempts
    .get(email)
    ?.filter((timestamp) => timestamp > cutoff)

  if (attempts && attempts.length >= BOOKING_RATE_LIMIT_MAX) {
    throw new Error(
      'Too many booking attempts. Please wait a few minutes or email troy.magennis@focusedobjective.com directly.',
    )
  }

  recentBookingAttempts.set(email, [...(attempts ?? []), now])
}

function isBlockedEmail(email: string) {
  const [, domain = ''] = email.split('@')
  const blockedEmails = splitEnvList('BOOKING_BLOCKED_EMAILS')
  const blockedDomains = splitEnvList('BOOKING_BLOCKED_EMAIL_DOMAINS')

  return blockedEmails.has(email) || blockedDomains.has(domain)
}

function isFreeEmail(email: string) {
  if (process.env.BOOKING_BLOCK_FREE_EMAILS === 'false') {
    return false
  }

  const [, domain = ''] = email.split('@')
  const allowedDomains = splitEnvList('BOOKING_ALLOWED_FREE_EMAIL_DOMAINS')
  const extraFreeDomains = splitEnvList('BOOKING_FREE_EMAIL_DOMAINS')

  if (allowedDomains.has(domain)) {
    return false
  }

  return FREE_EMAIL_DOMAINS.has(domain) || extraFreeDomains.has(domain)
}

function splitEnvList(name: string) {
  return new Set(
    (process.env[name] ?? '')
      .split(',')
      .map((value) => value.trim().toLowerCase())
      .filter(Boolean),
  )
}

function getSpamScore(request: BookingGuardRequest) {
  const fields = [request.name, request.company ?? '', request.topic]
  let score = 0

  for (const field of fields) {
    const tokens = field.match(/[A-Za-z0-9]{8,}/g) ?? []
    if (tokens.some(isRandomLookingToken)) {
      score += 2
    }
  }

  if (isSingleLongToken(request.name)) {
    score += 1
  }

  if (
    (request.company && isSingleLongToken(request.company)) ||
    (request.topic && isSingleLongToken(request.topic))
  ) {
    score += 1
  }

  return score
}

function isSingleLongToken(value: string) {
  const trimmed = value.trim()
  return /^[A-Za-z0-9]{12,}$/.test(trimmed)
}

function isRandomLookingToken(token: string) {
  if (token.length < 14 || !/^[A-Za-z0-9]+$/.test(token)) {
    return false
  }

  const uppercaseCount = (token.match(/[A-Z]/g) ?? []).length
  const lowercaseCount = (token.match(/[a-z]/g) ?? []).length
  const digitCount = (token.match(/\d/g) ?? []).length
  const hasMixedCase = uppercaseCount >= 3 && lowercaseCount >= 3
  const consonantRun = /[bcdfghjklmnpqrstvwxyz]{6,}/i.test(token)
  const vowelRatio =
    (token.match(/[aeiou]/gi) ?? []).length / Math.max(token.length, 1)

  return (
    (hasMixedCase && vowelRatio < 0.4) ||
    (digitCount >= 4 && token.length >= 16) ||
    consonantRun
  )
}

function throwRejectedBooking(): never {
  throw new Error(
    'We could not accept this booking request. Please email troy.magennis@focusedobjective.com directly.',
  )
}
