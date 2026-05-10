import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { eventApi, EventDetails } from '../services/api'
import ContentView from './ContentView'
import TeamsView from './TeamsView'
import TeamPointsView from './TeamPointsView'
import ScoreboardView from './ScoreboardView'

function EventManagement() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [event, setEvent] = useState<EventDetails | null>(null)
  const [loading, setLoading] = useState(true)
  const [showEditEventModal, setShowEditEventModal] = useState(false)
  const [eventName, setEventName] = useState('')
  const [eventDescription, setEventDescription] = useState('')
  const [selectedView, setSelectedView] = useState<'content' | 'teams' | 'points' | 'scoreboard'>('content')

  useEffect(() => {
    if (id) {
      loadEvent()
    }
  }, [id])

  const loadEvent = async () => {
    try {
      setLoading(true)
      const data = await eventApi.getEventDetails(parseInt(id!))
      setEvent(data)
    } catch (error) {
      console.error('Failed to load event:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleEditEvent = () => {
    if (event) {
      setEventName(event.name)
      setEventDescription(event.description)
      setShowEditEventModal(true)
    }
  }

  const handleUpdateEvent = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!id || !event) return

    try {
      await eventApi.updateEvent(parseInt(id), eventName, eventDescription)
      setShowEditEventModal(false)
      loadEvent()
    } catch (error) {
      console.error('Failed to update event:', error)
      alert('Failed to update event')
    }
  }


  if (loading) {
    return (
      <div className="relative flex-grow bg-gray-100 dark:bg-gray-900 flex items-center justify-center">
        <div className="text-xl text-gray-900 dark:text-white">Loading...</div>
      </div>
    )
  }

  if (!event) {
    return (
      <div className="flex-grow bg-gray-100 dark:bg-gray-900 flex items-center justify-center">
        <div className="text-xl text-gray-900 dark:text-white">Event not found</div>
      </div>
    )
  }

  return (
    <div className="flex-grow bg-gray-100 dark:bg-gray-900 py-8 text-gray-900 dark:text-white">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="mb-6 flex justify-between items-center">
          <button
            onClick={() => navigate('/')}
            className="text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-300 font-medium"
          >
            ← Back to Dashboard
          </button>
          <button
            onClick={() => navigate(`/event/${id}/present`)}
            className="bg-green-600 hover:bg-green-700 text-white font-bold py-2 px-4 rounded-lg"
          >
            Present Event
          </button>
        </div>

        <div className="mb-6">
          <div className="flex items-center gap-3 mb-2">
            <h1 className="text-4xl font-bold text-gray-900 dark:text-white">{event.name}</h1>
            <button
              onClick={handleEditEvent}
              className="text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-300 text-sm font-medium"
              title="Edit event name and description"
            >
              ✏️ Edit
            </button>
          </div>
          <p className="text-gray-600 dark:text-gray-400">{event.description}</p>
        </div>

        <div className="mb-6 flex gap-2">
          <button
            onClick={() => setSelectedView('content')}
            className={`px-4 py-2 rounded ${
              selectedView === 'content'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-200 hover:bg-gray-300 dark:hover:bg-gray-600'
            }`}
          >
            Content
          </button>
          <button
            onClick={() => setSelectedView('teams')}
            className={`px-4 py-2 rounded ${
              selectedView === 'teams'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-200 hover:bg-gray-300 dark:hover:bg-gray-600'
            }`}
          >
            Teams
          </button>
          <button
            onClick={() => setSelectedView('points')}
            className={`px-4 py-2 rounded ${
              selectedView === 'points'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-200 hover:bg-gray-300 dark:hover:bg-gray-600'
            }`}
          >
            Points
          </button>
          <button
            onClick={() => setSelectedView('scoreboard')}
            className={`px-4 py-2 rounded ${
              selectedView === 'scoreboard'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-200 hover:bg-gray-300 dark:hover:bg-gray-600'
            }`}
          >
            Scoreboard
          </button>
        </div>

        {selectedView === 'content' && (
          <ContentView eventId={parseInt(id!)} />
        )}

        {selectedView === 'teams' && <TeamsView eventId={parseInt(id!)} />}
        {selectedView === 'points' && <TeamPointsView eventId={parseInt(id!)} />}
        {selectedView === 'scoreboard' && <ScoreboardView eventId={parseInt(id!)} />}

        {showEditEventModal && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white dark:bg-gray-800 rounded-lg p-6 max-w-md w-full mx-4">
              <h2 className="text-2xl font-bold mb-4 text-gray-900 dark:text-white">Edit Event</h2>
              <form onSubmit={handleUpdateEvent}>
                <div className="mb-4">
                  <label className="block text-gray-700 dark:text-gray-300 text-sm font-bold mb-2">
                    Event Name
                  </label>
                  <input
                    type="text"
                    value={eventName}
                    onChange={(e) => setEventName(e.target.value)}
                    className="shadow appearance-none border dark:border-gray-700 rounded w-full py-2 px-3 text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-700 leading-tight focus:outline-none focus:shadow-outline"
                    required
                  />
                </div>
                <div className="mb-4">
                  <label className="block text-gray-700 dark:text-gray-300 text-sm font-bold mb-2">
                    Description
                  </label>
                  <textarea
                    value={eventDescription}
                    onChange={(e) => setEventDescription(e.target.value)}
                    className="shadow appearance-none border dark:border-gray-700 rounded w-full py-2 px-3 text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-700 leading-tight focus:outline-none focus:shadow-outline"
                    rows={3}
                  />
                </div>
                <div className="flex justify-end gap-2">
                  <button
                    type="button"
                    onClick={() => {
                      setShowEditEventModal(false)
                      setEventName('')
                      setEventDescription('')
                    }}
                    className="bg-gray-300 dark:bg-gray-600 hover:bg-gray-400 dark:hover:bg-gray-500 text-gray-800 dark:text-white font-bold py-2 px-4 rounded"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    className="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
                  >
                    Save
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}



      </div>
    </div>
  )
}

export default EventManagement

