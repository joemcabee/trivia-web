import axios from 'axios'

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'
const TOKEN_KEY = 'trivia_app_token'

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true,
})

let onUnauthorized: (() => void) | null = null

export function setAuthUnauthorizedHandler(handler: () => void) {
  onUnauthorized = handler
}

api.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_KEY)
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) onUnauthorized?.()
    return Promise.reject(error)
  }
)

export interface LoginResponse {
  token: string
  userName: string
  userId: string
}

export const authApi = {
  register: async (email: string, password: string): Promise<LoginResponse> => {
    const { data } = await api.post<LoginResponse>('/auth/register', {
      email: email.trim(),
      password,
    })
    return data
  },

  login: async (email: string, password: string): Promise<LoginResponse> => {
    const { data } = await api.post<LoginResponse>('/auth/login', {
      email: email.trim(),
      password,
    })
    return data
  },

  getMe: async (): Promise<{ userId: string; userName: string }> => {
    const { data } = await api.get<{ userId: string; userName: string }>('/auth/me')
    return data
  },
}

export interface Event {
  id: number
  name: string
  description: string
  createdAt: string
  updatedAt: string
  categoryCount: number
  questionCount: number
}

export interface Round {
  id: number
  name: string
  eventId: number
  order: number
  categories: Category[]
}

export interface Category {
  id: number
  name: string
  roundId: number
  order: number
  questions: Question[]
}

export interface Question {
  id: number
  questionText: string
  answer: string
  imageUrl: string | null
  categoryId: number
  order: number
}

export interface EventDetails {
  id: number
  name: string
  description: string
  createdAt: string
  updatedAt: string
  rounds: Round[]
}

export interface PresentationSlide {
  type: 'question' | 'answer'
  roundName: string
  categoryName: string
  questionText: string
  answer: string | null
  imageUrl: string | null
  questionId: number
  questionNumber: number
}

export interface PresentationData {
  eventId: number
  eventName: string
  slides: PresentationSlide[]
}

export const eventApi = {
  getAllEvents: async (): Promise<Event[]> => {
    const response = await api.get<Event[]>('/events')
    return response.data
  },

  getEvent: async (id: number): Promise<Event> => {
    const response = await api.get<Event>(`/events/${id}`)
    return response.data
  },

  getEventDetails: async (id: number): Promise<EventDetails> => {
    const response = await api.get<EventDetails>(`/events/${id}/details`)
    return response.data
  },

  createEvent: async (name: string, description: string): Promise<Event> => {
    const response = await api.post<Event>('/events', { name, description })
    return response.data
  },

  updateEvent: async (id: number, name: string, description: string): Promise<Event> => {
    const response = await api.put<Event>(`/events/${id}`, { name, description })
    return response.data
  },

  deleteEvent: async (id: number): Promise<void> => {
    await api.delete(`/events/${id}`)
  },

  cloneEvent: async (id: number): Promise<Event> => {
    const response = await api.post<Event>(`/events/${id}/clone`)
    return response.data
  },

  createRound: async (name: string, eventId: number): Promise<Round> => {
    const response = await api.post<Round>('/events/rounds', { name, eventId })
    return response.data
  },

  deleteRound: async (id: number): Promise<void> => {
    await api.delete(`/events/rounds/${id}`)
  },

  createCategory: async (name: string, roundId: number): Promise<Category> => {
    const response = await api.post<Category>('/events/categories', { name, roundId })
    return response.data
  },

  deleteCategory: async (id: number): Promise<void> => {
    await api.delete(`/events/categories/${id}`)
  },

  createQuestion: async (
    questionText: string,
    answer: string,
    categoryId: number,
    imageUrl?: string
  ): Promise<Question> => {
    const response = await api.post<Question>('/events/questions', {
      questionText,
      answer,
      categoryId,
      imageUrl,
    })
    return response.data
  },

  updateQuestion: async (
    id: number,
    questionText: string,
    answer: string,
    imageUrl?: string
  ): Promise<Question> => {
    const response = await api.put<Question>(`/events/questions/${id}`, {
      questionText,
      answer,
      imageUrl,
    })
    return response.data
  },

  deleteQuestion: async (id: number): Promise<void> => {
    await api.delete(`/events/questions/${id}`)
  },

  getPresentationData: async (eventId: number): Promise<PresentationData> => {
    const response = await api.get<PresentationData>(`/presentation/event/${eventId}`)
    return response.data
  },

  uploadImage: async (file: File): Promise<string> => {
    const formData = new FormData()
    formData.append('file', file)
    const response = await api.post<{ imageUrl: string }>('/images/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    })
    return response.data.imageUrl
  },
}

export default api

