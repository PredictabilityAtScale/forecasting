import { existsSync, readFileSync } from 'node:fs'
import { dirname, join, resolve } from 'node:path'
import { fileURLToPath } from 'node:url'
import type { BookingRequest } from '#/data/booking-schema'

let bundledEnvLoaded = false

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

  const payload = (await response.json()) as { join_url?: string }
  if (!payload.join_url) {
    throw new Error('Zoom API did not return a join URL.')
  }

  return payload.join_url
}

export async function createGoogleCalendarInvite({
  slot,
  attendee,
  zoomJoinUrl,
}: {
  slot: { start: string; end: string }
  attendee: BookingRequest
  zoomJoinUrl: string
}) {
  const googleAccessToken = await getGoogleAccessToken()
  const calendarId = getEnv('BOOKING_GOOGLE_CALENDAR_ID') ?? 'primary'
  const hostEmail = getEnv('BOOKING_HOST_EMAIL') ?? 'troy.magennis@focusedobjective.com'

  const response = await fetch(
    `https://www.googleapis.com/calendar/v3/calendars/${encodeURIComponent(calendarId)}/events?sendUpdates=all`,
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${googleAccessToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        summary: `Focused Objective meeting: ${attendee.name}`,
        description: `${attendee.topic}\n\nCompany: ${attendee.company ?? 'N/A'}\nTimezone: ${attendee.timezone}\nZoom: ${zoomJoinUrl}`,
        location: zoomJoinUrl,
        start: { dateTime: slot.start },
        end: { dateTime: slot.end },
        attendees: [{ email: attendee.email, displayName: attendee.name }, { email: hostEmail }],
      }),
    },
  )

  if (!response.ok) {
    const message = await response.text()
    throw new Error(`Google Calendar invite failed (${response.status}): ${message}`)
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
