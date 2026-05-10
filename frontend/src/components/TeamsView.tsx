import { useEffect, useState } from 'react'
import { eventApi, EventDetails } from '../services/api'

interface TeamsViewProps {
  eventId: number
}

function TeamsView({ eventId }: TeamsViewProps) {
  const [event, setEvent] = useState<EventDetails | null>(null)
  const [loading, setLoading] = useState(true)
  const [newTeamName, setNewTeamName] = useState('')
  const [editingTeamId, setEditingTeamId] = useState<number | null>(null)
  const [editingTeamName, setEditingTeamName] = useState('')

  useEffect(() => {
    loadEvent()
  }, [eventId])

  const loadEvent = async () => {
    try {
      setLoading(true)
      const data = await eventApi.getEventDetails(eventId)
      setEvent(data)
    } catch (error) {
      console.error('Failed to load teams:', error)
    } finally {
      setLoading(false)
    }
  }

  const getApiErrorMessage = (error: unknown, fallbackMessage: string) => {
    const responseData = (error as { response?: { data?: unknown } })?.response?.data
    if (typeof responseData === 'string' && responseData.trim().length > 0) {
      return responseData
    }

    return fallbackMessage
  }

  const normalizeTeamName = (name: string) => name.trim().toLowerCase()

  const validateTeamName = (name: string, ignoreTeamId?: number): string | null => {
    const normalizedInput = normalizeTeamName(name)
    if (!normalizedInput) {
      return 'Team name is required.'
    }

    if (
      event?.teams.some(
        (team) => team.id !== ignoreTeamId && normalizeTeamName(team.name) === normalizedInput
      )
    ) {
      return 'Team name must be unique within this event.'
    }

    return null
  }

  const handleCreateTeam = async (e: React.FormEvent) => {
    e.preventDefault()

    const validationMessage = validateTeamName(newTeamName)
    if (validationMessage) {
      alert(validationMessage)
      return
    }

    try {
      await eventApi.createTeam(newTeamName.trim(), eventId)
      setNewTeamName('')
      await loadEvent()
    } catch (error) {
      console.error('Failed to create team:', error)
      alert(getApiErrorMessage(error, 'Failed to create team'))
    }
  }

  const handleStartEditTeam = (teamId: number, teamName: string) => {
    setEditingTeamId(teamId)
    setEditingTeamName(teamName)
  }

  const handleSaveTeam = async (teamId: number) => {
    const validationMessage = validateTeamName(editingTeamName, teamId)
    if (validationMessage) {
      alert(validationMessage)
      return
    }

    try {
      await eventApi.updateTeam(teamId, editingTeamName.trim())
      setEditingTeamId(null)
      setEditingTeamName('')
      await loadEvent()
    } catch (error) {
      console.error('Failed to update team:', error)
      alert(getApiErrorMessage(error, 'Failed to update team'))
    }
  }

  const handleDeleteTeam = async (teamId: number) => {
    if (!confirm('Are you sure you want to delete this team?')) {
      return
    }

    try {
      await eventApi.deleteTeam(teamId)
      if (editingTeamId === teamId) {
        setEditingTeamId(null)
        setEditingTeamName('')
      }
      await loadEvent()
    } catch (error) {
      console.error('Failed to delete team:', error)
      alert('Failed to delete team')
    }
  }

  if (loading) {
    return (
      <div className="flex-grow bg-gray-100 dark:bg-gray-900 flex items-center justify-center py-10">
        <div className="text-xl text-gray-900 dark:text-white">Loading teams...</div>
      </div>
    )
  }

  if (!event) {
    return (
      <div className="flex-grow bg-gray-100 dark:bg-gray-900 flex items-center justify-center py-10">
        <div className="text-xl text-gray-900 dark:text-white">Teams not found</div>
      </div>
    )
  }

  return (
    <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-2xl font-semibold">Teams</h2>
      </div>

      <form onSubmit={handleCreateTeam} className="mb-4 flex gap-2">
        <input
          type="text"
          value={newTeamName}
          onChange={(e) => setNewTeamName(e.target.value)}
          className="shadow appearance-none border dark:border-gray-700 rounded w-full py-2 px-3 text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-700 leading-tight focus:outline-none focus:shadow-outline"
          placeholder="Enter team name"
          required
        />
        <button
          type="submit"
          className="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded whitespace-nowrap"
        >
          Add Team
        </button>
      </form>

      {event.teams.length === 0 ? (
        <p className="text-gray-500 dark:text-gray-400">No teams added yet.</p>
      ) : (
        <div className="space-y-2">
          {event.teams.map((team) => (
            <div
              key={team.id}
              className="flex items-center justify-between rounded border dark:border-gray-700 px-3 py-2 bg-gray-50 dark:bg-gray-700/50"
            >
              {editingTeamId === team.id ? (
                <div className="flex items-center gap-2 w-full">
                  <input
                    type="text"
                    value={editingTeamName}
                    onChange={(e) => setEditingTeamName(e.target.value)}
                    className="shadow appearance-none border dark:border-gray-700 rounded w-full py-1 px-2 text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-700 leading-tight focus:outline-none focus:shadow-outline"
                    required
                  />
                  <button
                    type="button"
                    onClick={() => handleSaveTeam(team.id)}
                    className="bg-green-600 hover:bg-green-700 text-white font-semibold px-3 py-1 rounded"
                  >
                    Save
                  </button>
                  <button
                    type="button"
                    onClick={() => {
                      setEditingTeamId(null)
                      setEditingTeamName('')
                    }}
                    className="bg-gray-300 dark:bg-gray-600 hover:bg-gray-400 dark:hover:bg-gray-500 text-gray-800 dark:text-white font-semibold px-3 py-1 rounded"
                  >
                    Cancel
                  </button>
                </div>
              ) : (
                <>
                  <span className="font-medium">{team.name}</span>
                  <div className="flex items-center gap-3">
                    <button
                      type="button"
                      onClick={() => handleStartEditTeam(team.id, team.name)}
                      className="text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-300 text-sm font-medium"
                    >
                      Edit
                    </button>
                    <button
                      type="button"
                      onClick={() => handleDeleteTeam(team.id)}
                      className="text-red-600 dark:text-red-400 hover:text-red-800 dark:hover:text-red-300 text-sm font-medium"
                    >
                      Delete
                    </button>
                  </div>
                </>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  )
}

export default TeamsView