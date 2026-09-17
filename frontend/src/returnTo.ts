// Survives the Keycloak round-trip (history state does not) so a deep link
// like /event/5/present can be restored after login.
const RETURN_TO_KEY = 'trivia_app_return_to'

export function setReturnTo(path: string): void {
  if (path && path !== '/') sessionStorage.setItem(RETURN_TO_KEY, path)
}

export function takeReturnTo(): string | null {
  const path = sessionStorage.getItem(RETURN_TO_KEY)
  sessionStorage.removeItem(RETURN_TO_KEY)
  return path && path.startsWith('/') ? path : null
}

export function peekReturnTo(): string | null {
  const path = sessionStorage.getItem(RETURN_TO_KEY)
  return path && path.startsWith('/') ? path : null
}
