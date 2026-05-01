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
