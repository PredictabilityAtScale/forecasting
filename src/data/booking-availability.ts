export type BookingSlot = {
  id: string
  start: string
  end: string
  durationMinutes: BookingDurationMinutes
}

const BOOKING_TIME_ZONE = 'America/Los_Angeles'
const BOOKING_HORIZON_DAYS = 42
const SLOT_MINUTES = 30
const BOOKING_DAY_START_MINUTES = 8 * 60 + 30
const BOOKING_DAY_END_MINUTES = 17 * 60
const WEEKDAYS = new Set([1, 2, 3, 4, 5])
export const bookingDurationOptions = [30, 60] as const
export type BookingDurationMinutes = (typeof bookingDurationOptions)[number]

type LocalDateTime = {
  year: number
  month: number
  day: number
  hour: number
  minute: number
}

// Source of truth: recurring Seattle business hours. Concrete UTC slots are generated from this.
export const bookingAvailability = getBookingAvailability()

export function getBookingAvailability({
  durationMinutes = 30,
  now = new Date(),
}: {
  durationMinutes?: BookingDurationMinutes
  now?: Date
} = {}): BookingSlot[] {
  const today = getZonedDate(now, BOOKING_TIME_ZONE)
  const slots: BookingSlot[] = []

  for (let dayOffset = 0; dayOffset < BOOKING_HORIZON_DAYS; dayOffset += 1) {
    const date = addCalendarDays(today, dayOffset)
    const weekday = new Date(Date.UTC(date.year, date.month - 1, date.day)).getUTCDay()

    if (!WEEKDAYS.has(weekday)) {
      continue
    }

    for (
      let minutes = BOOKING_DAY_START_MINUTES;
      minutes + durationMinutes <= BOOKING_DAY_END_MINUTES;
      minutes += SLOT_MINUTES
    ) {
      const start = zonedTimeToUtc({
        ...date,
        hour: Math.floor(minutes / 60),
        minute: minutes % 60,
      })
      const endMinutes = minutes + durationMinutes
      const end = zonedTimeToUtc({
        ...date,
        hour: Math.floor(endMinutes / 60),
        minute: endMinutes % 60,
      })

      if (start.getTime() > now.getTime()) {
        const startIso = start.toISOString()
        slots.push({
          id: `${startIso}/${durationMinutes}`,
          start: startIso,
          end: end.toISOString(),
          durationMinutes,
        })
      }
    }
  }

  return slots
}

function getZonedDate(date: Date, timeZone: string) {
  const parts = getZonedParts(date, timeZone)
  return { year: parts.year, month: parts.month, day: parts.day }
}

function addCalendarDays(date: Pick<LocalDateTime, 'year' | 'month' | 'day'>, days: number) {
  const next = new Date(Date.UTC(date.year, date.month - 1, date.day + days))
  return {
    year: next.getUTCFullYear(),
    month: next.getUTCMonth() + 1,
    day: next.getUTCDate(),
  }
}

function zonedTimeToUtc(local: LocalDateTime) {
  let utc = Date.UTC(local.year, local.month - 1, local.day, local.hour, local.minute)

  for (let index = 0; index < 2; index += 1) {
    const offset = getTimeZoneOffsetMs(new Date(utc), BOOKING_TIME_ZONE)
    utc = Date.UTC(local.year, local.month - 1, local.day, local.hour, local.minute) - offset
  }

  return new Date(utc)
}

function getTimeZoneOffsetMs(date: Date, timeZone: string) {
  const parts = getZonedParts(date, timeZone)
  const zonedAsUtc = Date.UTC(
    parts.year,
    parts.month - 1,
    parts.day,
    parts.hour,
    parts.minute,
    parts.second,
  )

  return zonedAsUtc - date.getTime()
}

function getZonedParts(date: Date, timeZone: string) {
  const formatter = new Intl.DateTimeFormat('en-US', {
    timeZone,
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    hourCycle: 'h23',
  })
  const values = Object.fromEntries(
    formatter.formatToParts(date).map((part) => [part.type, part.value]),
  )

  return {
    year: Number(values.year),
    month: Number(values.month),
    day: Number(values.day),
    hour: Number(values.hour),
    minute: Number(values.minute),
    second: Number(values.second),
  }
}
