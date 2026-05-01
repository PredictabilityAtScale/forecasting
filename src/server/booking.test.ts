import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import {
  createGoogleCalendarInvite,
  getOpenBookingSlotsWithinRange,
} from './booking'

const ENV_KEYS = [
  'BOOKING_HOST_EMAIL',
  'BOOKING_LINK_SECRET',
  'BOOKING_PUBLIC_BASE_URL',
  'BOOKING_GOOGLE_BUSY_CALENDAR_IDS',
  'BOOKING_GOOGLE_CALENDAR_ID',
  'GOOGLE_OAUTH_CLIENT_ID',
  'GOOGLE_OAUTH_CLIENT_SECRET',
  'GOOGLE_OAUTH_REFRESH_TOKEN',
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

  it('adds Zoom as conference data instead of a map location', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(jsonResponse({ access_token: 'google-token' }))
      .mockResolvedValueOnce(jsonResponse({ id: 'google-event-id' }))
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
      location?: string
      conferenceData?: {
        conferenceSolution?: { key?: { type?: string }; name?: string }
        entryPoints?: Array<{ entryPointType?: string; uri?: string }>
      }
    }
    const patchBody = JSON.parse(fetchMock.mock.calls[2][1].body as string) as {
      description?: string
    }

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
    expect(patchBody.description).toContain(
      'Zoom: https://us02web.zoom.us/j/123?pwd=abc',
    )
    expect(patchBody.description).toContain(
      'Cancel: https://focusedobjective.com/book/manage?action=cancel&token=',
    )
  })
})

function jsonResponse(body: unknown) {
  return {
    ok: true,
    json: async () => body,
    text: async () => JSON.stringify(body),
  } as Response
}
