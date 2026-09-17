import { Link, Navigate } from 'react-router-dom'
import { useAuth } from 'react-oidc-context'
import { peekReturnTo } from '../returnTo'

export default function Landing() {
  const auth = useAuth()

  if (auth.isLoading) {
    return (
      <div className="flex-grow bg-gray-100 dark:bg-gray-900 flex items-center justify-center">
        <div className="text-xl text-gray-900 dark:text-white">Loading...</div>
      </div>
    )
  }

  // Already signed in (e.g. just returned from Keycloak with no deep link):
  // forward into the app instead of showing the login page.
  if (auth.isAuthenticated && !peekReturnTo()) {
    return <Navigate to="/events" replace />
  }

  return (
    <div className="flex-grow flex bg-gray-100 dark:bg-gray-900 items-center justify-center px-4 py-12">
      <div className="max-w-2xl w-full text-center">
        <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-4">
          Trivia Night
        </h1>
        <p className="text-lg text-gray-600 dark:text-gray-400 mb-8">
          Create trivia events, organize rounds and questions, track team
          scores, and run the show from a keyboard-driven presentation view.
        </p>
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-8 mb-8 text-left">
          <ul className="space-y-3 text-gray-700 dark:text-gray-300">
            <li>Build events with rounds, categories, and questions</li>
            <li>Import questions from CSV or spreadsheets</li>
            <li>Score teams round by round with live standings</li>
            <li>Present questions then reveal answers on the big screen</li>
          </ul>
        </div>
        {auth.error ? (
          <div>
            <p className="text-red-600 dark:text-red-400 text-sm mb-4">{auth.error.message}</p>
            <button
              onClick={() => auth.signinRedirect()}
              className="bg-blue-600 hover:bg-blue-700 text-white font-bold py-3 px-8 rounded-lg text-lg"
            >
              Try again
            </button>
          </div>
        ) : auth.isAuthenticated ? (
          <Link
            to="/events"
            className="inline-block bg-blue-600 hover:bg-blue-700 text-white font-bold py-3 px-8 rounded-lg text-lg"
          >
            Go to your events
          </Link>
        ) : (
          <div>
            <button
              onClick={() => auth.signinRedirect()}
              className="bg-blue-600 hover:bg-blue-700 text-white font-bold py-3 px-8 rounded-lg text-lg"
            >
              Log in
            </button>
            <p className="text-gray-500 dark:text-gray-400 text-sm mt-4">
              Sign in with your Slacker account to get started.
            </p>
          </div>
        )}
      </div>
    </div>
  )
}
