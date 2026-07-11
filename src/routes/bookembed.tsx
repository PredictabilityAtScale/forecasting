import { createFileRoute } from '@tanstack/react-router'
import { BookPage } from './book'

export const Route = createFileRoute('/bookembed')({
  component: BookEmbedPage,
  head: () => ({
    meta: [
      {
        title: 'Book a Zoom call',
      },
      {
        name: 'robots',
        content: 'noindex, nofollow',
      },
    ],
  }),
})

function BookEmbedPage() {
  return <BookPage embedded />
}
