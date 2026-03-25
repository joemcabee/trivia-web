import { useEffect } from 'react'
import { BrowserRouter as Router, Routes, Route, Navigate, useLocation } from 'react-router-dom'
import { AuthProvider, useAuth } from './contexts/AuthContext'
import { setAuthUnauthorizedHandler } from './services/api'
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
  const location = useLocation()
  const hideFooter = /^\/event\/[^/]+\/present$/.test(location.pathname)

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
          </div>
        </footer>
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
