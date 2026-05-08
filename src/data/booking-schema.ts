import { z } from 'zod'

export const bookingSchema = z.object({
  slotId: z.string().min(1),
  durationMinutes: z.union([z.literal(30), z.literal(60)]),
  name: z.string().trim().min(2).max(120),
  email: z.string().trim().toLowerCase().email(),
  company: z.string().trim().max(120).optional(),
  topic: z.string().trim().max(600).default(''),
  timezone: z.string().trim().min(2).max(80),
  website: z.string().trim().max(200).optional(),
})

export type BookingRequest = z.infer<typeof bookingSchema>
