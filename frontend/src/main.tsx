import React from 'react'
import ReactDOM from 'react-dom/client'
import { AuthProvider } from 'react-oidc-context'
import App from './App.tsx'
import './index.css'

const appOrigin = window.location.origin

const oidcConfig = {
  authority:
    import.meta.env.VITE_OIDC_AUTHORITY ??
    'https://auth-test.slackersoftware.com/realms/slacker-test',
  client_id: import.meta.env.VITE_OIDC_CLIENT_ID ?? 'trivia-web',
  redirect_uri: appOrigin,
  post_logout_redirect_uri: appOrigin,
  // Dedicated silent renew page — prevents the main app from loading inside
  // the hidden iframe, which can cause an infinite reload loop.
  silent_redirect_uri: `${appOrigin}/silent-renew.html`,
  scope: 'openid profile email',
  automaticSilentRenew: true,
  // Clean up the ?code=&state= params after Keycloak redirects back post-login.
  onSigninCallback: () => {
    window.history.replaceState({}, document.title, window.location.pathname)
  },
}

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <AuthProvider {...oidcConfig}>
      <App />
    </AuthProvider>
  </React.StrictMode>,
)
