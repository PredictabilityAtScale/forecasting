import { existsSync, readFileSync } from 'node:fs'
import { dirname, join, resolve } from 'node:path'
import { fileURLToPath } from 'node:url'
import { bookingAvailability } from '#/data/booking-availability'
import type { BookingRequest } from '#/data/booking-schema'
import { createBookingLinkToken, parseBookingLinkToken } from '#/server/booking-links'

let bundledEnvLoaded = false

export async function getOpenBookingSlots() {
  return getOpenBookingSlotsWithinRange({
    timeMin: new Date(Math.min(...bookingAvailability.map((slot) => new Date(slot.start).getTime()))).toISOString(),
    timeMax: new Date(Math.max(...bookingAvailability.map((slot) => new Date(slot.end).getTime()))).toISOString(),
  })
}

export async function getOpenBookingSlotsWithinRange({
  timeMin,
  timeMax,
}: {
  timeMin: string
  timeMax: string
}) {
  const googleAccessToken = await getGoogleAccessToken()
  const calendarId = getEnv('BOOKING_GOOGLE_CALENDAR_ID') ?? 'primary'

  const response = await fetch('https://www.googleapis.com/calendar/v3/freeBusy', {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${googleAccessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      timeMin,
      timeMax,
      items: [{ id: calendarId }],
    }),
  })

  if (!response.ok) {
    const message = await response.text()
    throw new Error(`Google freeBusy failed (${response.status}): ${message}`)
  }

  const payload = (await response.json()) as {
    calendars?: Record<string, { busy?: Array<{ start: string; end: string }> }>
  }
  const busyRanges = payload.calendars?.[calendarId]?.busy ?? []

  return bookingAvailability.filter((slot) => {
    const slotStart = new Date(slot.start).getTime()
    const slotEnd = new Date(slot.end).getTime()
    return !busyRanges.some((busy) => {
      const busyStart = new Date(busy.start).getTime()
      const busyEnd = new Date(busy.end).getTime()
      return slotStart < busyEnd && busyStart < slotEnd
    })
  })
}

export async function createZoomMeeting({
  topic,
  start,
  end,
  attendeeName,
  attendeeEmail,
}: {
  topic: string
  start: string
  end: string
  attendeeName: string
  attendeeEmail: string
}) {
  const zoomAccessToken = await getZoomAccessToken()
  const durationMinutes = Math.max(
    15,
    Math.round((new Date(end).getTime() - new Date(start).getTime()) / 60000),
  )

  const response = await fetch('https://api.zoom.us/v2/users/me/meetings', {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${zoomAccessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      topic: `Focused Objective: ${topic}`,
      type: 2,
      start_time: start,
      duration: durationMinutes,
      timezone: 'UTC',
      agenda: `Requested by ${attendeeName} (${attendeeEmail})`,
      settings: {
        join_before_host: false,
        waiting_room: true,
      },
    }),
  })

  if (!response.ok) {
    const message = await response.text()
    throw new Error(`Zoom meeting creation failed (${response.status}): ${message}`)
  }

  const payload = (await response.json()) as { id?: number; join_url?: string }
  if (!payload.join_url || !payload.id) {
    throw new Error('Zoom API did not return meeting identifiers.')
  }

  return { joinUrl: payload.join_url, meetingId: String(payload.id) }
}

export async function createGoogleCalendarInvite({
  slot,
  attendee,
  zoomJoinUrl,
  zoomMeetingId,
}: {
  slot: { start: string; end: string }
  attendee: BookingRequest
  zoomJoinUrl: string
  zoomMeetingId: string
}) {
  const googleAccessToken = await getGoogleAccessToken()
  const calendarId = getEnv('BOOKING_GOOGLE_CALENDAR_ID') ?? 'primary'
  const hostEmail = getEnv('BOOKING_HOST_EMAIL') ?? 'troy.magennis@focusedobjective.com'

  const createResponse = await fetch(
    `https://www.googleapis.com/calendar/v3/calendars/${encodeURIComponent(calendarId)}/events?sendUpdates=all`,
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${googleAccessToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        summary: `Focused Objective meeting: ${attendee.name}`,
        description: attendee.topic,
        location: zoomJoinUrl,
        start: { dateTime: slot.start },
        end: { dateTime: slot.end },
        attendees: [{ email: attendee.email, displayName: attendee.name }, { email: hostEmail }],
      }),
    },
  )

  if (!createResponse.ok) {
    const message = await createResponse.text()
    throw new Error(`Google Calendar invite failed (${createResponse.status}): ${message}`)
  }

  const createdEvent = (await createResponse.json()) as { id?: string }
  if (!createdEvent.id) {
    throw new Error('Google Calendar API did not return event id.')
  }

  const token = createBookingLinkToken({
    purpose: 'booking-manage-v1',
    issuedAt: new Date().toISOString(),
    email: attendee.email,
    googleEventId: createdEvent.id,
    zoomMeetingId,
    expiresAt: slot.end,
  })

  const patchResponse = await fetch(
    `https://www.googleapis.com/calendar/v3/calendars/${encodeURIComponent(calendarId)}/events/${encodeURIComponent(createdEvent.id)}?sendUpdates=all`,
    {
      method: 'PATCH',
      headers: {
        Authorization: `Bearer ${googleAccessToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        description: buildMeetingDescription({ slot, attendee, zoomJoinUrl, token }),
      }),
    },
  )

  if (!patchResponse.ok) {
    const message = await patchResponse.text()
    throw new Error(`Google Calendar invite update failed (${patchResponse.status}): ${message}`)
  }
}

export async function cancelBookingFromToken(token: string) {
  const payload = parseBookingLinkToken(token)
  const googleAccessToken = await getGoogleAccessToken()
  const zoomAccessToken = await getZoomAccessToken()
  const calendarId = getEnv('BOOKING_GOOGLE_CALENDAR_ID') ?? 'primary'

  const deleteEvent = await fetch(
    `https://www.googleapis.com/calendar/v3/calendars/${encodeURIComponent(calendarId)}/events/${encodeURIComponent(payload.googleEventId)}?sendUpdates=all`,
    {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${googleAccessToken}` },
    },
  )
  if (!deleteEvent.ok && deleteEvent.status !== 404 && deleteEvent.status !== 410) {
    throw new Error(`Google event cancel failed (${deleteEvent.status}).`)
  }

  const deleteZoom = await fetch(
    `https://api.zoom.us/v2/meetings/${encodeURIComponent(payload.zoomMeetingId)}`,
    {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${zoomAccessToken}` },
    },
  )
  if (!deleteZoom.ok && deleteZoom.status !== 404) {
    throw new Error(`Zoom cancel failed (${deleteZoom.status}).`)
  }
}

export async function rescheduleBookingFromToken(token: string, nextSlot: { start: string; end: string }) {
  const payload = parseBookingLinkToken(token)
  const stillOpen = await getOpenBookingSlotsWithinRange({
    timeMin: nextSlot.start,
    timeMax: nextSlot.end,
  })
  if (!stillOpen.some((slot) => slot.start === nextSlot.start && slot.end === nextSlot.end)) {
    throw new Error('That slot is no longer available.')
  }
  const googleAccessToken = await getGoogleAccessToken()
  const zoomAccessToken = await getZoomAccessToken()
  const calendarId = getEnv('BOOKING_GOOGLE_CALENDAR_ID') ?? 'primary'

  const updateEvent = await fetch(
    `https://www.googleapis.com/calendar/v3/calendars/${encodeURIComponent(calendarId)}/events/${encodeURIComponent(payload.googleEventId)}?sendUpdates=all`,
    {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${googleAccessToken}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ start: { dateTime: nextSlot.start }, end: { dateTime: nextSlot.end } }),
    },
  )
  if (!updateEvent.ok) {
    throw new Error(`Google event reschedule failed (${updateEvent.status}).`)
  }

  const duration = Math.max(
    15,
    Math.round((new Date(nextSlot.end).getTime() - new Date(nextSlot.start).getTime()) / 60000),
  )
  const updateZoom = await fetch(
    `https://api.zoom.us/v2/meetings/${encodeURIComponent(payload.zoomMeetingId)}`,
    {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${zoomAccessToken}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ start_time: nextSlot.start, duration, timezone: 'UTC' }),
    },
  )
  if (!updateZoom.ok) {
    throw new Error(`Zoom reschedule failed (${updateZoom.status}).`)
  }
}

async function getZoomAccessToken() {
  const accountId = getEnv('ZOOM_ACCOUNT_ID')
  const clientId = getEnv('ZOOM_CLIENT_ID')
  const clientSecret = getEnv('ZOOM_CLIENT_SECRET')

  if (!accountId || !clientId || !clientSecret) {
    throw new Error('Zoom integration missing: ZOOM_ACCOUNT_ID, ZOOM_CLIENT_ID, ZOOM_CLIENT_SECRET.')
  }

  const basicToken = Buffer.from(`${clientId}:${clientSecret}`).toString('base64')
  const response = await fetch(
    'https://zoom.us/oauth/token?grant_type=account_credentials&account_id=' +
      encodeURIComponent(accountId),
    {
      method: 'POST',
      headers: {
        Authorization: `Basic ${basicToken}`,
      },
    },
  )

  if (!response.ok) {
    const message = await response.text()
    throw new Error(`Zoom auth failed (${response.status}): ${message}`)
  }

  const payload = (await response.json()) as { access_token?: string }
  if (!payload.access_token) {
    throw new Error('Zoom auth did not return access_token.')
  }
  return payload.access_token
}

async function getGoogleAccessToken() {
  const clientId = getEnv('GOOGLE_OAUTH_CLIENT_ID')
  const clientSecret = getEnv('GOOGLE_OAUTH_CLIENT_SECRET')
  const refreshToken = getEnv('GOOGLE_OAUTH_REFRESH_TOKEN')

  if (!clientId || !clientSecret || !refreshToken) {
    throw new Error(
      'Google integration missing: GOOGLE_OAUTH_CLIENT_ID, GOOGLE_OAUTH_CLIENT_SECRET, GOOGLE_OAUTH_REFRESH_TOKEN.',
    )
  }

  const response = await fetch('https://oauth2.googleapis.com/token', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded',
    },
    body: new URLSearchParams({
      client_id: clientId,
      client_secret: clientSecret,
      refresh_token: refreshToken,
      grant_type: 'refresh_token',
    }),
  })

  if (!response.ok) {
    const message = await response.text()
    throw new Error(`Google auth failed (${response.status}): ${message}`)
  }

  const payload = (await response.json()) as { access_token?: string }
  if (!payload.access_token) {
    throw new Error('Google auth did not return access_token.')
  }

  return payload.access_token
}

function getEnv(name: string) {
  const directValue = process.env[name]
  if (directValue) {
    return directValue
  }

  loadBundledEnv()
  return process.env[name]
}

function loadBundledEnv() {
  if (bundledEnvLoaded) {
    return
  }

  bundledEnvLoaded = true

  const moduleDir = dirname(fileURLToPath(import.meta.url))
  const candidateFiles = [
    join(process.cwd(), '.env'),
    join(process.cwd(), '.env.production'),
    join(moduleDir, '.env'),
    join(moduleDir, '.env.production'),
    join(moduleDir, '..', '.env'),
    join(moduleDir, '..', '.env.production'),
    resolve(process.cwd(), '.output/server/.env'),
    resolve(process.cwd(), '.output/server/.env.production'),
  ]

  for (const file of candidateFiles) {
    if (existsSync(file)) {
      applyEnvFile(file)
    }
  }
}

function applyEnvFile(file: string) {
  const lines = readFileSync(file, 'utf8').split(/\r?\n/)

  for (const line of lines) {
    const trimmed = line.trim()
    if (!trimmed || trimmed.startsWith('#')) {
      continue
    }

    const equalsIndex = trimmed.indexOf('=')
    if (equalsIndex === -1) {
      continue
    }

    const key = trimmed.slice(0, equalsIndex).trim()
    const value = unquoteEnvValue(trimmed.slice(equalsIndex + 1).trim())

    if (key && process.env[key] === undefined) {
      process.env[key] = value
    }
  }
}

function unquoteEnvValue(value: string) {
  const quote = value[0]
  if ((quote === '"' || quote === "'") && value.endsWith(quote)) {
    return value.slice(1, -1).replace(/\\n/g, '\n')
  }

  return value
}


function buildMeetingDescription({
  slot,
  attendee,
  zoomJoinUrl,
  token,
}: {
  slot: { start: string; end: string }
  attendee: BookingRequest
  zoomJoinUrl: string
  token: string
}) {
  const startLocal = new Date(slot.start).toLocaleString([], {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
    timeZone: attendee.timezone,
    timeZoneName: 'short',
  })

  const baseUrl = getEnv('BOOKING_PUBLIC_BASE_URL') ?? 'http://localhost:3000'
  const cancelLink = `${baseUrl}/book/manage?action=cancel&token=${encodeURIComponent(token)}`
  const rescheduleLink = `${baseUrl}/book/manage?action=reschedule&token=${encodeURIComponent(token)}`

  return [
    attendee.topic,
    '',
    `Company: ${attendee.company ?? 'N/A'}`,
    `Attendee timezone: ${attendee.timezone}`,
    `Meeting starts: ${startLocal}`,
    `Zoom: ${zoomJoinUrl}`,
    '',
    `Cancel: ${cancelLink}`,
    `Reschedule: ${rescheduleLink}`,
  ].join('\n')
}
