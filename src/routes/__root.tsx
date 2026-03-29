import {
  HeadContent,
  Scripts,
  createRootRouteWithContext,
} from '@tanstack/react-router'
import Footer from '../components/Footer'
import Header from '../components/Header'
import { SITE_TITLE, SITE_DESCRIPTION } from '../lib/site'
import '../styles.css'

import type { QueryClient } from '@tanstack/react-query'

interface MyRouterContext {
  queryClient: QueryClient
}

const THEME_INIT_SCRIPT = `(function(){try{var stored=window.localStorage.getItem('theme');var mode=(stored==='light'||stored==='dark'||stored==='auto')?stored:'auto';var prefersDark=window.matchMedia('(prefers-color-scheme: dark)').matches;var resolved=mode==='auto'?(prefersDark?'dark':'light'):mode;var root=document.documentElement;root.classList.remove('light','dark');root.classList.add(resolved);if(mode==='auto'){root.removeAttribute('data-theme')}else{root.setAttribute('data-theme',mode)}root.style.colorScheme=resolved;var color=window.localStorage.getItem('color-theme');if(color&&color!=='lagoon'){root.setAttribute('data-color',color)}else{root.removeAttribute('data-color')}}catch(e){}})();`

export const Route = createRootRouteWithContext<MyRouterContext>()({
  head: () => ({
    meta: [
      {
        charSet: 'utf-8',
      },
      {
        name: 'viewport',
        content: 'width=device-width, initial-scale=1',
      },
      {
        title: SITE_TITLE,
      },
      {
        name: 'description',
        content: SITE_DESCRIPTION,
      },
    ],
    links: [
      {
        rel: 'icon',
        href: '/fo.jpg',
        type: 'image/jpeg',
      },
      {
        rel: 'preconnect',
        href: 'https://fonts.googleapis.com',
      },
      {
        rel: 'preconnect',
        href: 'https://fonts.gstatic.com',
        crossOrigin: 'anonymous',
      },
      {
        rel: 'stylesheet',
        href: 'https://fonts.googleapis.com/css2?family=Fraunces:opsz,wght@9..144,500;9..144,700&family=Manrope:wght@400;500;600;700;800&display=swap',
      },
    ],
  }),
  shellComponent: RootDocument,
})

function RootDocument({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en" suppressHydrationWarning>
      <head>
        <script dangerouslySetInnerHTML={{ __html: THEME_INIT_SCRIPT }} />
        <HeadContent />
      </head>
      <body
        className="font-sans antialiased [overflow-wrap:anywhere] selection:bg-[rgba(79,184,178,0.24)]"
        style={{
          backgroundColor: 'var(--bg-base, #e7f3ec)',
          color: 'var(--sea-ink, #173a40)',
          fontFamily: "'Manrope', ui-sans-serif, system-ui, sans-serif",
        }}
      >
        <div
          className="flex min-h-screen flex-col"
          style={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}
        >
          <Header />
          <div className="flex-1" style={{ flex: '1 1 0%' }}>
            {children}
          </div>
          <Footer />
        </div>
        <Scripts />
      </body>
    </html>
  )
}
