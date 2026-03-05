import { Navigate, createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/simml-studio')({
  component: SimmlStudioRedirect,
})

function SimmlStudioRedirect() {
  return <Navigate to="/kanban-scrum-sim" />
}
