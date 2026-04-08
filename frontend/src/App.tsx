import { useEffect, useState } from 'react'
import { BrowserRouter as Router, Routes, Route, Navigate, useLocation } from 'react-router-dom'
import { AuthProvider, useAuth } from './contexts/AuthContext'
import { setAuthUnauthorizedHandler, supportApi } from './services/api'
import Dashboard from './components/Dashboard'
import EventManagement from './components/EventManagement'
import Presentation from './components/Presentation'
import Login from './components/Login'
import SignUp from './components/SignUp'

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { token, isLoading } = useAuth()
  if (isLoading) {
    return (
      <div className="flex-grow bg-gray-100 dark:bg-gray-900 flex items-center justify-center">
        <div className="text-xl text-gray-900 dark:text-white">Loading...</div>
      </div>
    )
  }
  if (!token) return <Navigate to="/login" replace />
  return <>{children}</>
}

function AppRoutes() {
  const { logout } = useAuth()

  useEffect(() => {
    setAuthUnauthorizedHandler(() => logout())
  }, [logout])

  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/signup" element={<SignUp />} />
      <Route
        path="/"
        element={
          <ProtectedRoute>
            <Dashboard />
          </ProtectedRoute>
        }
      />
      <Route
        path="/event/:id"
        element={
          <ProtectedRoute>
            <EventManagement />
          </ProtectedRoute>
        }
      />
      <Route
        path="/event/:id/present"
        element={
          <ProtectedRoute>
            <Presentation />
          </ProtectedRoute>
        }
      />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

function AppContent() {
  const { token } = useAuth()
  const location = useLocation()
  const hideFooter = /^\/event\/[^/]+\/present$/.test(location.pathname)
  const [showSupportModal, setShowSupportModal] = useState(false)
  const [supportMessage, setSupportMessage] = useState('')
  const [isSubmittingSupport, setIsSubmittingSupport] = useState(false)
  const [supportError, setSupportError] = useState<string | null>(null)
  const [showSupportSuccessToast, setShowSupportSuccessToast] = useState(false)

  const openSupportModal = () => {
    setSupportMessage('')
    setSupportError(null)
    setShowSupportModal(true)
  }

  const closeSupportModal = () => {
    setShowSupportModal(false)
    setSupportMessage('')
    setSupportError(null)
  }

  const submitSupportRequest = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!supportMessage.trim()) {
      setSupportError('Please enter a message before submitting.')
      return
    }

    try {
      setIsSubmittingSupport(true)
      setSupportError(null)
      await supportApi.createSupportRequest(supportMessage.trim())
      closeSupportModal()
      setShowSupportSuccessToast(true)
      window.setTimeout(() => {
        setShowSupportSuccessToast(false)
      }, 3000)
    } catch {
      setSupportError('Failed to submit support request. Please try again.')
    } finally {
      setIsSubmittingSupport(false)
    }
  }

  return (
    <div className="min-h-screen flex flex-col">
      <AppRoutes />
      {!hideFooter && (
        <footer className="bg-gray-100 dark:bg-gray-900 text-center py-3 text-sm text-gray-600 dark:text-gray-300">
          <div>
            <a
              className="underline hover:text-gray-800 dark:hover:text-white mx-2"
              href="https://www.slackersoftware.com"
              target="_blank"
              rel="noopener noreferrer"
            >
              Slacker Software
            </a>
            |
            <a
              className="underline hover:text-gray-800 dark:hover:text-white mx-2"
              href="https://www.slackersoftware.com/privacy"
              target="_blank"
              rel="noopener noreferrer"
            >
              Privacy Policy
            </a>
            {token && (
              <>
                |
                <button
                  onClick={openSupportModal}
                  className="underline hover:text-gray-800 dark:hover:text-white mx-2"
                >
                  Support
                </button>
              </>
            )}
          </div>
        </footer>
      )}
      {showSupportModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 max-w-lg w-full mx-4">
            <h2 className="text-2xl font-bold mb-4 text-gray-900 dark:text-white">Contact Support</h2>
            <form onSubmit={submitSupportRequest}>
              <div className="mb-4">
                <label className="block text-gray-700 dark:text-gray-300 text-sm font-bold mb-2">
                  Message
                </label>
                <textarea
                  value={supportMessage}
                  onChange={(e) => setSupportMessage(e.target.value)}
                  className="shadow appearance-none border dark:border-gray-700 rounded w-full py-2 px-3 text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-700 leading-tight focus:outline-none focus:shadow-outline"
                  rows={5}
                  required
                />
              </div>
              {supportError && (
                <p className="text-red-600 dark:text-red-400 text-sm mb-4">
                  {supportError}
                </p>
              )}
              <div className="flex justify-end gap-2">
                <button
                  type="button"
                  onClick={closeSupportModal}
                  className="bg-gray-300 dark:bg-gray-600 hover:bg-gray-400 dark:hover:bg-gray-500 text-gray-800 dark:text-white font-bold py-2 px-4 rounded"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={isSubmittingSupport}
                  className="bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 text-white font-bold py-2 px-4 rounded"
                >
                  {isSubmittingSupport ? 'Submitting...' : 'Submit'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
      {showSupportSuccessToast && (
        <div className="fixed bottom-4 right-4 z-50">
          <div className="bg-green-600 text-white px-4 py-3 rounded-lg shadow-lg">
            Support request submitted successfully.
          </div>
        </div>
      )}
    </div>
  )
}

function App() {
  return (
    <Router>
      <AuthProvider>
        <AppContent />
      </AuthProvider>
    </Router>
  )
}

export default App
