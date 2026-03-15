import { useEffect } from 'react'
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider, useAuth } from './contexts/AuthContext'
import { setAuthUnauthorizedHandler } from './services/api'
import Dashboard from './components/Dashboard'
import EventManagement from './components/EventManagement'
import Presentation from './components/Presentation'
import Login from './components/Login'

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { token, isLoading } = useAuth()
  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-100 flex items-center justify-center">
        <div className="text-xl">Loading...</div>
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

function App() {
  return (
    <Router>
      <AuthProvider>
        <AppRoutes />
      </AuthProvider>
    </Router>
  )
}

export default App
