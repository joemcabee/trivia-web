import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
  type ReactNode,
} from 'react'

const TOKEN_KEY = 'trivia_app_token'
const USER_KEY = 'trivia_app_user'

export interface AuthUser {
  userId: string
  userName: string
}

interface AuthContextValue {
  token: string | null
  user: AuthUser | null
  isLoading: boolean
  login: (token: string, user: AuthUser) => void
  logout: () => void
  setToken: (token: string | null) => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setTokenState] = useState<string | null>(() =>
    localStorage.getItem(TOKEN_KEY)
  )
  const [user, setUser] = useState<AuthUser | null>(() => {
    const raw = localStorage.getItem(USER_KEY)
    if (!raw) return null
    try {
      return JSON.parse(raw) as AuthUser
    } catch {
      return null
    }
  })
  const [isLoading, setIsLoading] = useState(true)

  const setToken = useCallback((value: string | null) => {
    setTokenState(value)
    if (value === null) {
      localStorage.removeItem(TOKEN_KEY)
      localStorage.removeItem(USER_KEY)
      setUser(null)
    }
  }, [])

  const login = useCallback((newToken: string, newUser: AuthUser) => {
    setTokenState(newToken)
    setUser(newUser)
    localStorage.setItem(TOKEN_KEY, newToken)
    localStorage.setItem(USER_KEY, JSON.stringify(newUser))
  }, [])

  const logout = useCallback(() => {
    setTokenState(null)
    setUser(null)
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
  }, [])

  useEffect(() => {
    if (!token) {
      setIsLoading(false)
      return
    }
    setIsLoading(false)
  }, [token])

  const value: AuthContextValue = {
    token,
    user,
    isLoading,
    login,
    logout,
    setToken,
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
