import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { getOpenBookingSlotsWithinRange } from './booking'

const ENV_KEYS = [
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

function jsonResponse(body: unknown) {
  return {
    ok: true,
    json: async () => body,
    text: async () => JSON.stringify(body),
  } as Response
}
