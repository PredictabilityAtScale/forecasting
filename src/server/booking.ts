import { z } from 'zod'

export const bookingSchema = z.object({
  slotId: z.string().min(1),
  name: z.string().min(2).max(120),
  email: z.string().email(),
  company: z.string().max(120).optional(),
  topic: z.string().min(5).max(1000),
  timezone: z.string().min(2).max(80),
})

export type BookingRequest = z.infer<typeof bookingSchema>

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
  const calendarId = process.env.BOOKING_GOOGLE_CALENDAR_ID ?? 'primary'
  const hostEmail = process.env.BOOKING_HOST_EMAIL ?? 'troy.magennis@focusedobjective.com'

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
  const accountId = process.env.ZOOM_ACCOUNT_ID
  const clientId = process.env.ZOOM_CLIENT_ID
  const clientSecret = process.env.ZOOM_CLIENT_SECRET

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
  const clientId = process.env.GOOGLE_OAUTH_CLIENT_ID
  const clientSecret = process.env.GOOGLE_OAUTH_CLIENT_SECRET
  const refreshToken = process.env.GOOGLE_OAUTH_REFRESH_TOKEN

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
