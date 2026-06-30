import { randomBytes } from 'node:crypto'
import { existsSync, readFileSync } from 'node:fs'
import { dirname, join, resolve } from 'node:path'
import { fileURLToPath } from 'node:url'
import { getBookingAvailability } from '#/data/booking-availability'
import type { BookingDurationMinutes } from '#/data/booking-availability'
import type { BookingRequest } from '#/data/booking-schema'
import { SITE_URL } from '#/lib/site'
import {
  createBookingLinkToken,
  parseBookingLinkToken,
} from '#/server/booking-links'

let bundledEnvLoaded = false
const BOOKING_BUFFER_MS = 30 * 60 * 1000
const ENV_FILE_NAMES = ['.env.local', '.env', '.env.production'] as const
const ZOOM_MEETING_TOPIC_MAX_LENGTH = 200

type FreeBusyCalendar = {
  busy?: Array<{ start: string; end: string }>
  errors?: Array<{ reason?: string; domain?: string }>
}

export async function getOpenBookingSlots(
  durationMinutes: BookingDurationMinutes = 30,
) {
  const bookingAvailability = getBookingAvailability({ durationMinutes })
  const minStart = Math.min(
    ...bookingAvailability.map((slot) => new Date(slot.start).getTime()),
  )
  const maxEnd = Math.max(
    ...bookingAvailability.map((slot) => new Date(slot.end).getTime()),
  )

  return getOpenBookingSlotsWithinRange({
    durationMinutes,
    timeMin: new Date(minStart - BOOKING_BUFFER_MS).toISOString(),
    timeMax: new Date(maxEnd + BOOKING_BUFFER_MS).toISOString(),
  })
}

export async function getOpenBookingSlotsWithinRange({
  durationMinutes = 30,
  timeMin,
  timeMax,
}: {
  durationMinutes?: BookingDurationMinutes
  timeMin: string
  timeMax: string
}) {
  const rangeStart = new Date(timeMin).getTime()
  const rangeEnd = new Date(timeMax).getTime()
  const bookingAvailability = getBookingAvailability({
    durationMinutes,
  }).filter((slot) => {
    const slotStart = new Date(slot.start).getTime()
    const slotEnd = new Date(slot.end).getTime()
    return slotStart < rangeEnd && rangeStart < slotEnd
  })
  const googleAccessToken = await getGoogleAccessToken()
  const busyCalendarIds = getBusyCalendarIds()

  const response = await fetch(
    'https://www.googleapis.com/calendar/v3/freeBusy',
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${googleAccessToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        timeMin: new Date(rangeStart - BOOKING_BUFFER_MS).toISOString(),
        timeMax: new Date(rangeEnd + BOOKING_BUFFER_MS).toISOString(),
        items: busyCalendarIds.map((id) => ({ id })),
      }),
    },
  )

  if (!response.ok) {
    const message = await response.text()
    throw new Error(`Google freeBusy failed (${response.status}): ${message}`)
  }

  const payload = (await response.json()) as {
    calendars?: Record<string, FreeBusyCalendar>
  }
  const calendars: Partial<Record<string, FreeBusyCalendar>> =
    payload.calendars ?? {}
  const calendarErrors = Object.entries(calendars)
    .filter(([, calendar]) => calendar.errors && calendar.errors.length > 0)
    .map(
      ([id, calendar]) =>
        `${id}: ${calendar.errors
          ?.map((error) => error.reason ?? error.domain ?? 'unknown')
          .join(', ')}`,
    )

  if (calendarErrors.length > 0) {
    throw new Error(
      `Google freeBusy calendar error: ${calendarErrors.join('; ')}`,
    )
  }

  const busyRanges = busyCalendarIds.flatMap((id) => {
    const calendar = calendars[id]
    return calendar ? (calendar.busy ?? []) : []
  })

  return bookingAvailability.filter((slot) => {
    const slotStart = new Date(slot.start).getTime()
    const slotEnd = new Date(slot.end).getTime()
    return !busyRanges.some((busy) => {
      const busyStart = new Date(busy.start).getTime() - BOOKING_BUFFER_MS
      const busyEnd = new Date(busy.end).getTime() + BOOKING_BUFFER_MS
      return slotStart < busyEnd && busyStart < slotEnd
    })
  })
}

function getBusyCalendarIds() {
  const rawIds =
    getEnv('BOOKING_GOOGLE_BUSY_CALENDAR_IDS') ??
    getEnv('BOOKING_GOOGLE_CALENDAR_ID') ??
    'primary'
  const ids = rawIds
    .split(',')
    .map((id) => id.trim())
    .filter(Boolean)

  return ids.length > 0 ? ids : ['primary']
}

export async function createZoomMeeting({
  topic,
  start,
  end,
  attendeeName,
  attendeeEmail,
}: {
  topic?: string
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
      topic: getZoomMeetingTopic(topic),
      type: 2,
      start_time: start,
      duration: durationMinutes,
      timezone: 'UTC',
      agenda: getZoomMeetingAgenda({ attendeeName, attendeeEmail, topic }),
      settings: {
        join_before_host: false,
        waiting_room: true,
      },
    }),
  })

  if (!response.ok) {
    const message = await response.text()
    throw new Error(
      `Zoom meeting creation failed (${response.status}): ${message}`,
    )
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
  const hostEmail =
    getEnv('BOOKING_HOST_EMAIL') ?? 'troy.magennis@focusedobjective.com'
  const googleEventId = createGoogleCalendarEventId()
  const token = createBookingLinkToken({
    purpose: 'booking-manage-v1',
    issuedAt: new Date().toISOString(),
    email: attendee.email,
    googleEventId,
    zoomMeetingId,
    expiresAt: slot.end,
  })

  const eventInsertUrl = `https://www.googleapis.com/calendar/v3/calendars/${encodeURIComponent(calendarId)}/events?sendUpdates=all&conferenceDataVersion=1`
  const eventBody = {
    id: googleEventId,
    summary: `Focused Objective meeting: ${attendee.name}`,
    description: buildMeetingDescription({
      slot,
      attendee,
      zoomJoinUrl,
      token,
    }),
    start: { dateTime: slot.start },
    end: { dateTime: slot.end },
    attendees: [
      { email: attendee.email, displayName: attendee.name },
      { email: hostEmail },
    ],
    conferenceData: {
      conferenceId: zoomMeetingId,
      conferenceSolution: {
        key: { type: 'addOn' },
        name: 'Zoom',
      },
      entryPoints: [
        {
          entryPointType: 'video',
          uri: zoomJoinUrl,
          label: 'Join Zoom meeting',
        },
      ],
    },
  }

  let createResponse = await fetch(eventInsertUrl, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${googleAccessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(eventBody),
  })
  let createErrorMessage = ''
  let retriedWithoutConferenceData = false

  if (!createResponse.ok) {
    createErrorMessage = await createResponse.text()
    if (
      shouldRetryWithoutGoogleConferenceData(
        createResponse.status,
        createErrorMessage,
      )
    ) {
      const { conferenceData: _conferenceData, ...eventBodyWithoutConference } =
        eventBody
      retriedWithoutConferenceData = true

      createResponse = await fetch(
        `https://www.googleapis.com/calendar/v3/calendars/${encodeURIComponent(calendarId)}/events?sendUpdates=all`,
        {
          method: 'POST',
          headers: {
            Authorization: `Bearer ${googleAccessToken}`,
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(eventBodyWithoutConference),
        },
      )
    }
  }

  if (!createResponse.ok) {
    const message = retriedWithoutConferenceData
      ? await createResponse.text()
      : createErrorMessage
    throw new Error(
      `Google Calendar invite failed (${createResponse.status}): ${message || createErrorMessage}`,
    )
  }
}

export async function cancelBookingFromToken(token: string) {
  const payload = parseBookingLinkToken(token)
  const googleAccessToken = await getGoogleAccessToken()
  const zoomAccessToken = await getZoomAccessToken()
  const calendarId = getEnv('BOOKING_GOOGLE_CALENDAR_ID') ?? 'primary'

  await deleteZoomMeeting(payload.zoomMeetingId, zoomAccessToken)

  const deleteEvent = await fetch(
    `https://www.googleapis.com/calendar/v3/calendars/${encodeURIComponent(calendarId)}/events/${encodeURIComponent(payload.googleEventId)}?sendUpdates=all`,
    {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${googleAccessToken}` },
    },
  )
  if (
    !deleteEvent.ok &&
    deleteEvent.status !== 404 &&
    deleteEvent.status !== 410
  ) {
    throw new Error(`Google event cancel failed (${deleteEvent.status}).`)
  }
}

export async function rescheduleBookingFromToken(
  token: string,
  nextSlot: { start: string; end: string },
) {
  const payload = parseBookingLinkToken(token)
  const durationMinutes = getSlotDurationMinutes(nextSlot)
  const stillOpen = await getOpenBookingSlotsWithinRange({
    durationMinutes,
    timeMin: nextSlot.start,
    timeMax: nextSlot.end,
  })
  if (
    !stillOpen.some(
      (slot) => slot.start === nextSlot.start && slot.end === nextSlot.end,
    )
  ) {
    throw new Error('That slot is no longer available.')
  }
  const googleAccessToken = await getGoogleAccessToken()
  const zoomAccessToken = await getZoomAccessToken()
  const calendarId = getEnv('BOOKING_GOOGLE_CALENDAR_ID') ?? 'primary'

  const updateEvent = await fetch(
    `https://www.googleapis.com/calendar/v3/calendars/${encodeURIComponent(calendarId)}/events/${encodeURIComponent(payload.googleEventId)}?sendUpdates=all`,
    {
      method: 'PATCH',
      headers: {
        Authorization: `Bearer ${googleAccessToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        start: { dateTime: nextSlot.start },
        end: { dateTime: nextSlot.end },
      }),
    },
  )
  if (!updateEvent.ok) {
    throw new Error(`Google event reschedule failed (${updateEvent.status}).`)
  }

  const duration = Math.max(
    15,
    Math.round(
      (new Date(nextSlot.end).getTime() - new Date(nextSlot.start).getTime()) /
        60000,
    ),
  )
  const updateZoom = await fetch(
    `https://api.zoom.us/v2/meetings/${encodeURIComponent(payload.zoomMeetingId)}`,
    {
      method: 'PATCH',
      headers: {
        Authorization: `Bearer ${zoomAccessToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        start_time: nextSlot.start,
        duration,
        timezone: 'UTC',
      }),
    },
  )
  if (!updateZoom.ok) {
    const message = await readResponseText(updateZoom)
    throw new Error(
      `Zoom reschedule failed (${updateZoom.status})${formatErrorDetail(message)}.`,
    )
  }
}

async function deleteZoomMeeting(meetingId: string, accessToken: string) {
  const meetingIds = getZoomMeetingIdCandidates(meetingId)

  for (const [index, candidate] of meetingIds.entries()) {
    const deleteZoom = await fetch(
      `https://api.zoom.us/v2/meetings/${encodeURIComponent(candidate)}`,
      {
        method: 'DELETE',
        headers: { Authorization: `Bearer ${accessToken}` },
      },
    )
    if (deleteZoom.ok || deleteZoom.status === 404) {
      return
    }

    const message = await readResponseText(deleteZoom)
    if (isZoomMeetingNotFound(deleteZoom.status, message)) {
      return
    }

    if (deleteZoom.status === 400 && index < meetingIds.length - 1) {
      continue
    }

    throw new Error(
      `Zoom cancel failed (${deleteZoom.status})${formatErrorDetail(message)}.`,
    )
  }
}

function getZoomMeetingIdCandidates(meetingId: string) {
  const trimmed = meetingId.trim()
  const digitsOnly = trimmed.replace(/\D/g, '')

  return [trimmed, digitsOnly].filter(
    (candidate, index, candidates) =>
      candidate && candidates.indexOf(candidate) === index,
  )
}

function isZoomMeetingNotFound(status: number, message: string) {
  return (
    status === 404 ||
    (status === 400 &&
      /"code"\s*:\s*(3001|3003)/.test(message) &&
      /meeting/i.test(message))
  )
}

async function readResponseText(response: Response) {
  try {
    return await response.text()
  } catch {
    return ''
  }
}

function formatErrorDetail(message: string) {
  const trimmed = message.trim()
  return trimmed ? `: ${trimmed}` : ''
}

function shouldRetryWithoutGoogleConferenceData(
  status: number,
  message: string,
) {
  return status === 400 && /conference/i.test(message)
}

function getSlotDurationMinutes(slot: {
  start: string
  end: string
}): BookingDurationMinutes {
  const duration = Math.round(
    (new Date(slot.end).getTime() - new Date(slot.start).getTime()) / 60000,
  )
  if (duration !== 30 && duration !== 60) {
    throw new Error('Bookings must be 30 or 60 minutes.')
  }

  return duration
}

function createGoogleCalendarEventId() {
  return `fo${randomBytes(16).toString('hex')}`
}

async function getZoomAccessToken() {
  const accountId = getEnv('ZOOM_ACCOUNT_ID')
  const clientId = getEnv('ZOOM_CLIENT_ID')
  const clientSecret = getEnv('ZOOM_CLIENT_SECRET')

  if (!accountId || !clientId || !clientSecret) {
    throw new Error(
      'Zoom integration missing: ZOOM_ACCOUNT_ID, ZOOM_CLIENT_ID, ZOOM_CLIENT_SECRET.',
    )
  }

  const basicToken = Buffer.from(`${clientId}:${clientSecret}`).toString(
    'base64',
  )
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
    ...ENV_FILE_NAMES.map((file) => join(process.cwd(), file)),
    ...ENV_FILE_NAMES.map((file) => join(moduleDir, file)),
    ...ENV_FILE_NAMES.map((file) => join(moduleDir, '..', file)),
    ...ENV_FILE_NAMES.map((file) =>
      resolve(process.cwd(), '.output/server', file),
    ),
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

  const baseUrl = (getEnv('BOOKING_PUBLIC_BASE_URL') ?? SITE_URL).replace(
    /\/$/,
    '',
  )
  const cancelLink = `${baseUrl}/book/manage?action=cancel&token=${encodeURIComponent(token)}`
  const rescheduleLink = `${baseUrl}/book/manage?action=reschedule&token=${encodeURIComponent(token)}`

  return [
    escapeHtml(attendee.topic),
    '',
    `Company: ${escapeHtml(attendee.company ?? 'N/A')}`,
    `Attendee timezone: ${escapeHtml(attendee.timezone)}`,
    `Meeting starts: ${escapeHtml(startLocal)}`,
    `Zoom: ${escapeHtml(zoomJoinUrl)}`,
    '',
    `Cancel: <a href="${escapeHtml(cancelLink)}">Cancel this meeting</a>`,
    `Reschedule: <a href="${escapeHtml(rescheduleLink)}">Reschedule this meeting</a>`,
  ].join('<br>')
}

function getZoomMeetingTopic(topic?: string) {
  const trimmed = topic?.trim()
  const zoomTopic = trimmed
    ? `Focused Objective: ${trimmed}`
    : 'Focused Objective meeting'

  return truncateText(zoomTopic, ZOOM_MEETING_TOPIC_MAX_LENGTH)
}

function getZoomMeetingAgenda({
  attendeeName,
  attendeeEmail,
  topic,
}: {
  attendeeName: string
  attendeeEmail: string
  topic?: string
}) {
  const lines = [`Requested by ${attendeeName} (${attendeeEmail})`]
  const trimmedTopic = topic?.trim()

  if (trimmedTopic) {
    lines.push('', `Topic: ${trimmedTopic}`)
  }

  return lines.join('\n')
}

function truncateText(value: string, maxLength: number) {
  const characters = Array.from(value)
  if (characters.length <= maxLength) {
    return value
  }

  return `${characters.slice(0, maxLength - 3).join('').trimEnd()}...`
}

function escapeHtml(value: string) {
  return value.replace(/[&<>"']/g, (char) => {
    switch (char) {
      case '&':
        return '&amp;'
      case '<':
        return '&lt;'
      case '>':
        return '&gt;'
      case '"':
        return '&quot;'
      case "'":
        return '&#39;'
      default:
        return char
    }
  })
}
