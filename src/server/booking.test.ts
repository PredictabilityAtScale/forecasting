import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import {
  cancelBookingFromToken,
  createGoogleCalendarInvite,
  createZoomMeeting,
  getOpenBookingSlotsWithinRange,
} from './booking'
import { createBookingLinkToken } from './booking-links'

const ENV_KEYS = [
  'BOOKING_HOST_EMAIL',
  'BOOKING_LINK_SECRET',
  'BOOKING_PUBLIC_BASE_URL',
  'BOOKING_GOOGLE_BUSY_CALENDAR_IDS',
  'BOOKING_GOOGLE_CALENDAR_ID',
  'GOOGLE_OAUTH_CLIENT_ID',
  'GOOGLE_OAUTH_CLIENT_SECRET',
  'GOOGLE_OAUTH_REFRESH_TOKEN',
  'ZOOM_ACCOUNT_ID',
  'ZOOM_CLIENT_ID',
  'ZOOM_CLIENT_SECRET',
] as const

describe('booking availability', () => {
  const originalEnv = new Map<string, string | undefined>()

  beforeEach(() => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-05-01T12:00:00-07:00'))

    for (const key of ENV_KEYS) {
      originalEnv.set(key, process.env[key])
      delete process.env[key]
    }

    process.env.GOOGLE_OAUTH_CLIENT_ID = 'client-id'
    process.env.GOOGLE_OAUTH_CLIENT_SECRET = 'client-secret'
    process.env.GOOGLE_OAUTH_REFRESH_TOKEN = 'refresh-token'
  })

  afterEach(() => {
    vi.useRealTimers()
    vi.unstubAllGlobals()

    for (const key of ENV_KEYS) {
      const value = originalEnv.get(key)
      if (value === undefined) {
        delete process.env[key]
      } else {
        process.env[key] = value
      }
    }

    originalEnv.clear()
  })

  it('removes slots blocked by any configured busy calendar', async () => {
    process.env.BOOKING_GOOGLE_BUSY_CALENDAR_IDS =
      'primary,secondary@example.com'

    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(jsonResponse({ access_token: 'google-token' }))
      .mockResolvedValueOnce(
        jsonResponse({
          calendars: {
            primary: {
              busy: [
                {
                  start: '2026-05-04T17:00:00.000Z',
                  end: '2026-05-04T21:00:00.000Z',
                },
              ],
            },
            'secondary@example.com': {
              busy: [
                {
                  start: '2026-05-04T22:00:00.000Z',
                  end: '2026-05-04T23:00:00.000Z',
                },
              ],
            },
          },
        }),
      )
    vi.stubGlobal('fetch', fetchMock)

    const slots = await getOpenBookingSlotsWithinRange({
      durationMinutes: 30,
      timeMin: '2026-05-04T15:30:00.000Z',
      timeMax: '2026-05-05T00:00:00.000Z',
    })

    const requestBody = JSON.parse(
      fetchMock.mock.calls[1][1].body as string,
    ) as {
      items: Array<{ id: string }>
    }
    const startTimes = slots.map((slot) => slot.start)

    expect(requestBody.items).toEqual([
      { id: 'primary' },
      { id: 'secondary@example.com' },
    ])
    expect(startTimes).toContain('2026-05-04T16:00:00.000Z')
    expect(startTimes).not.toContain('2026-05-04T17:00:00.000Z')
    expect(startTimes).not.toContain('2026-05-04T22:00:00.000Z')
    expect(startTimes).toContain('2026-05-04T23:30:00.000Z')
  })

  it('fails closed when Google reports a per-calendar freeBusy error', async () => {
    process.env.BOOKING_GOOGLE_CALENDAR_ID = 'missing@example.com'

    vi.stubGlobal(
      'fetch',
      vi
        .fn()
        .mockResolvedValueOnce(jsonResponse({ access_token: 'google-token' }))
        .mockResolvedValueOnce(
          jsonResponse({
            calendars: {
              'missing@example.com': {
                errors: [{ domain: 'global', reason: 'notFound' }],
                busy: [],
              },
            },
          }),
        ),
    )

    await expect(
      getOpenBookingSlotsWithinRange({
        durationMinutes: 30,
        timeMin: '2026-05-04T15:30:00.000Z',
        timeMax: '2026-05-05T00:00:00.000Z',
      }),
    ).rejects.toThrow(
      'Google freeBusy calendar error: missing@example.com: notFound',
    )
  })
})

describe('booking invites', () => {
  const originalEnv = new Map<string, string | undefined>()

  beforeEach(() => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-05-01T12:00:00-07:00'))

    for (const key of ENV_KEYS) {
      originalEnv.set(key, process.env[key])
      delete process.env[key]
    }

    process.env.BOOKING_HOST_EMAIL = 'host@example.com'
    process.env.BOOKING_LINK_SECRET = 'booking-secret'
    process.env.BOOKING_PUBLIC_BASE_URL = 'https://focusedobjective.com'
    process.env.GOOGLE_OAUTH_CLIENT_ID = 'client-id'
    process.env.GOOGLE_OAUTH_CLIENT_SECRET = 'client-secret'
    process.env.GOOGLE_OAUTH_REFRESH_TOKEN = 'refresh-token'
  })

  afterEach(() => {
    vi.useRealTimers()
    vi.unstubAllGlobals()

    for (const key of ENV_KEYS) {
      const value = originalEnv.get(key)
      if (value === undefined) {
        delete process.env[key]
      } else {
        process.env[key] = value
      }
    }

    originalEnv.clear()
  })

  it('limits long submitted topics before creating Zoom meetings', async () => {
    process.env.ZOOM_ACCOUNT_ID = 'zoom-account'
    process.env.ZOOM_CLIENT_ID = 'zoom-client'
    process.env.ZOOM_CLIENT_SECRET = 'zoom-secret'

    const longTopic = 'Discuss secure AI connected to legal workflows. '.repeat(
      10,
    )
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(jsonResponse({ access_token: 'zoom-token' }))
      .mockResolvedValueOnce(
        jsonResponse({
          id: 123456789,
          join_url: 'https://us02web.zoom.us/j/123456789?pwd=abc',
        }),
      )
    vi.stubGlobal('fetch', fetchMock)

    await createZoomMeeting({
      topic: longTopic,
      start: '2026-05-04T22:00:00.000Z',
      end: '2026-05-04T23:00:00.000Z',
      attendeeName: 'Dimitri Ponomareff',
      attendeeEmail: 'dimitri@example.com',
    })

    const createBody = JSON.parse(
      fetchMock.mock.calls[1][1].body as string,
    ) as {
      topic?: string
      agenda?: string
    }

    expect(createBody.topic).toHaveLength(200)
    expect(createBody.topic).toMatch(/^Focused Objective: /)
    expect(createBody.topic).toMatch(/\.\.\.$/)
    expect(createBody.agenda).toContain(
      'Requested by Dimitri Ponomareff (dimitri@example.com)',
    )
    expect(createBody.agenda).toContain(`Topic: ${longTopic.trim()}`)
  })

  it('adds Zoom as conference data instead of a map location', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(jsonResponse({ access_token: 'google-token' }))
      .mockResolvedValueOnce(jsonResponse({ id: 'google-event-id' }))
    vi.stubGlobal('fetch', fetchMock)

    await createGoogleCalendarInvite({
      slot: {
        start: '2026-05-04T22:00:00.000Z',
        end: '2026-05-04T22:30:00.000Z',
      },
      attendee: {
        name: 'Troy Magennis',
        email: 'troy@example.com',
        company: 'Focused Objective',
        topic: 'Reduce LLM costs',
        timezone: 'America/Los_Angeles',
      },
      zoomJoinUrl: 'https://us02web.zoom.us/j/123?pwd=abc',
      zoomMeetingId: '123',
    })

    expect(fetchMock.mock.calls[1][0]).toContain('conferenceDataVersion=1')

    const createBody = JSON.parse(
      fetchMock.mock.calls[1][1].body as string,
    ) as {
      description?: string
      location?: string
      id?: string
      conferenceData?: {
        conferenceSolution?: { key?: { type?: string }; name?: string }
        entryPoints?: Array<{ entryPointType?: string; uri?: string }>
      }
    }

    expect(createBody.id).toMatch(/^fo[0-9a-f]{32}$/)
    expect(createBody.location).toBeUndefined()
    expect(createBody.conferenceData?.conferenceSolution).toEqual({
      key: { type: 'addOn' },
      name: 'Zoom',
    })
    expect(createBody.conferenceData?.entryPoints).toEqual([
      {
        entryPointType: 'video',
        label: 'Join Zoom meeting',
        uri: 'https://us02web.zoom.us/j/123?pwd=abc',
      },
    ])
    expect(createBody.description).toContain(
      'Zoom: https://us02web.zoom.us/j/123?pwd=abc',
    )
    expect(createBody.description).toContain(
      'Cancel: <a href="https://focusedobjective.com/book/manage?action=cancel&amp;token=',
    )
    expect(createBody.description).toContain('">Cancel this meeting</a>')
    expect(fetchMock).toHaveBeenCalledTimes(2)
  })

  it('defaults booking management links to the public site URL', async () => {
    delete process.env.BOOKING_PUBLIC_BASE_URL

    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(jsonResponse({ access_token: 'google-token' }))
      .mockResolvedValueOnce(jsonResponse({ id: 'google-event-id' }))
    vi.stubGlobal('fetch', fetchMock)

    await createGoogleCalendarInvite({
      slot: {
        start: '2026-05-04T22:00:00.000Z',
        end: '2026-05-04T22:30:00.000Z',
      },
      attendee: {
        name: 'Troy Magennis',
        email: 'troy@example.com',
        company: 'Focused Objective',
        topic: 'Reduce LLM costs',
        timezone: 'America/Los_Angeles',
      },
      zoomJoinUrl: 'https://us02web.zoom.us/j/123?pwd=abc',
      zoomMeetingId: '123',
    })

    const createBody = JSON.parse(
      fetchMock.mock.calls[1][1].body as string,
    ) as {
      description?: string
    }

    expect(createBody.description).toContain(
      'Cancel: <a href="https://focusedobjective.com/book/manage?action=cancel&amp;token=',
    )
    expect(createBody.description).toContain(
      'Reschedule: <a href="https://focusedobjective.com/book/manage?action=reschedule&amp;token=',
    )
    expect(createBody.description).not.toContain('localhost')
  })

  it('escapes attendee text in the HTML meeting description', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(jsonResponse({ access_token: 'google-token' }))
      .mockResolvedValueOnce(jsonResponse({ id: 'google-event-id' }))
    vi.stubGlobal('fetch', fetchMock)

    await createGoogleCalendarInvite({
      slot: {
        start: '2026-05-04T22:00:00.000Z',
        end: '2026-05-04T22:30:00.000Z',
      },
      attendee: {
        name: 'Troy Magennis',
        email: 'troy@example.com',
        company: '<script>alert("x")</script>',
        topic: 'Discuss <b>forecasting</b>',
        timezone: 'America/Los_Angeles',
      },
      zoomJoinUrl: 'https://us02web.zoom.us/j/123?pwd=abc',
      zoomMeetingId: '123',
    })

    const createBody = JSON.parse(
      fetchMock.mock.calls[1][1].body as string,
    ) as {
      description?: string
    }

    expect(createBody.description).toContain(
      'Discuss &lt;b&gt;forecasting&lt;/b&gt;',
    )
    expect(createBody.description).toContain(
      'Company: &lt;script&gt;alert(&quot;x&quot;)&lt;/script&gt;',
    )
    expect(createBody.description).not.toContain('<script>')
  })
})

describe('booking cancellation', () => {
  const originalEnv = new Map<string, string | undefined>()

  beforeEach(() => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-05-01T12:00:00-07:00'))

    for (const key of ENV_KEYS) {
      originalEnv.set(key, process.env[key])
      delete process.env[key]
    }

    process.env.BOOKING_LINK_SECRET = 'booking-secret'
    process.env.BOOKING_GOOGLE_CALENDAR_ID = 'primary'
    process.env.GOOGLE_OAUTH_CLIENT_ID = 'client-id'
    process.env.GOOGLE_OAUTH_CLIENT_SECRET = 'client-secret'
    process.env.GOOGLE_OAUTH_REFRESH_TOKEN = 'refresh-token'
    process.env.ZOOM_ACCOUNT_ID = 'zoom-account'
    process.env.ZOOM_CLIENT_ID = 'zoom-client'
    process.env.ZOOM_CLIENT_SECRET = 'zoom-secret'
  })

  afterEach(() => {
    vi.useRealTimers()
    vi.unstubAllGlobals()

    for (const key of ENV_KEYS) {
      const value = originalEnv.get(key)
      if (value === undefined) {
        delete process.env[key]
      } else {
        process.env[key] = value
      }
    }

    originalEnv.clear()
  })

  it('deletes the Zoom meeting before deleting the Google event', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(jsonResponse({ access_token: 'google-token' }))
      .mockResolvedValueOnce(jsonResponse({ access_token: 'zoom-token' }))
      .mockResolvedValueOnce(emptyResponse(204))
      .mockResolvedValueOnce(emptyResponse(204))
    vi.stubGlobal('fetch', fetchMock)

    await cancelBookingFromToken(createManageToken())

    expect(fetchMock.mock.calls[2][0]).toBe(
      'https://api.zoom.us/v2/meetings/123456789',
    )
    expect(fetchMock.mock.calls[3][0]).toBe(
      'https://www.googleapis.com/calendar/v3/calendars/primary/events/google-event-id?sendUpdates=all',
    )
  })

  it('does not delete the Google event when Zoom cancellation fails', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(jsonResponse({ access_token: 'google-token' }))
      .mockResolvedValueOnce(jsonResponse({ access_token: 'zoom-token' }))
      .mockResolvedValueOnce(
        jsonResponse({ code: 300, message: 'Meeting has started.' }, 400),
      )
    vi.stubGlobal('fetch', fetchMock)

    await expect(cancelBookingFromToken(createManageToken())).rejects.toThrow(
      'Zoom cancel failed (400): {"code":300,"message":"Meeting has started."}.',
    )

    expect(fetchMock).toHaveBeenCalledTimes(3)
  })

  it('retries Zoom cancellation with a digits-only meeting ID', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(jsonResponse({ access_token: 'google-token' }))
      .mockResolvedValueOnce(jsonResponse({ access_token: 'zoom-token' }))
      .mockResolvedValueOnce(
        jsonResponse({ code: 300, message: 'Validation failed.' }, 400),
      )
      .mockResolvedValueOnce(emptyResponse(204))
      .mockResolvedValueOnce(emptyResponse(204))
    vi.stubGlobal('fetch', fetchMock)

    await cancelBookingFromToken(
      createManageToken({ zoomMeetingId: '123 456 789' }),
    )

    expect(fetchMock.mock.calls[2][0]).toBe(
      'https://api.zoom.us/v2/meetings/123%20456%20789',
    )
    expect(fetchMock.mock.calls[3][0]).toBe(
      'https://api.zoom.us/v2/meetings/123456789',
    )
  })
})

function createManageToken({
  zoomMeetingId = '123456789',
}: {
  zoomMeetingId?: string
} = {}) {
  return createBookingLinkToken({
    purpose: 'booking-manage-v1',
    issuedAt: '2026-05-01T19:00:00.000Z',
    email: 'customer@example.com',
    googleEventId: 'google-event-id',
    zoomMeetingId,
    expiresAt: '2026-05-04T22:30:00.000Z',
  })
}

function jsonResponse(body: unknown, status = 200) {
  return {
    ok: status >= 200 && status < 300,
    status,
    json: async () => body,
    text: async () => JSON.stringify(body),
  } as Response
}

function emptyResponse(status: number) {
  return {
    ok: status >= 200 && status < 300,
    status,
    text: async () => '',
  } as Response
}
