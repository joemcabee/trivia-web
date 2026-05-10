import { useEffect, useState } from 'react'
import { eventApi, EventDetails } from '../services/api'

interface TeamPointsViewProps {
  eventId: number
}

function TeamPointsView({ eventId }: TeamPointsViewProps) {
  const [event, setEvent] = useState<EventDetails | null>(null)
  const [roundPointsDraft, setRoundPointsDraft] = useState<Record<string, number>>({})
  const [isSaving, setIsSaving] = useState(false)
  const [loading, setLoading] = useState(true)

  const getApiErrorMessage = (error: unknown, fallbackMessage: string) => {
    const responseData = (error as { response?: { data?: unknown } })?.response?.data
    if (typeof responseData === 'string' && responseData.trim().length > 0) {
      return responseData
    }

    return fallbackMessage
  }

  const handleRoundPointInputChange = (teamId: number, roundId: number, value: string) => {
    const parsedValue = Number.parseInt(value, 10)
    const key = `${teamId}-${roundId}`
    setRoundPointsDraft((previous) => ({
      ...previous,
      [key]: Number.isNaN(parsedValue) ? 0 : parsedValue,
    }))
  }

  const loadEvent = async (showLoading = true) => {
    try {
      if (showLoading) {
        setLoading(true)
      }
      const data = await eventApi.getEventDetails(eventId)
      setEvent(data)
    } catch (error) {
      console.error('Failed to load team points:', error)
    } finally {
      if (showLoading) {
        setLoading(false)
      }
    }
  }

  const handleSaveRoundPoints = async () => {
    if (!event || event.teams.length === 0 || event.rounds.length === 0) return

    const pointsByRound: Record<number, { teamId: number; points: number }[]> = {}

    Object.entries(roundPointsDraft).forEach(([key, points]) => {
      const [teamIdStr, roundIdStr] = key.split('-')
      const teamId = parseInt(teamIdStr, 10)
      const roundId = parseInt(roundIdStr, 10)

      if (!pointsByRound[roundId]) {
        pointsByRound[roundId] = []
      }
      pointsByRound[roundId].push({ teamId, points })
    })

    try {
      setIsSaving(true)
      const savePromises = Object.entries(pointsByRound).map(([roundIdStr, teamPoints]) => {
        const roundId = parseInt(roundIdStr, 10)
        return eventApi.setRoundTeamPoints(roundId, teamPoints)
      })

      const results = await Promise.all(savePromises)
      const updatedTeamPoints = results.flat()

      // Update local event state with the returned team points
      setEvent((prevEvent) => {
        if (!prevEvent) return prevEvent
        return {
          ...prevEvent,
          teamPoints: updatedTeamPoints,
        }
      })

      setIsSaving(false)
    } catch (error) {
      console.error('Failed to save team points:', error)
      alert(getApiErrorMessage(error, 'Failed to save team points'))
      setIsSaving(false)
    }
  }

  useEffect(() => {
    loadEvent()
  }, [eventId])

  useEffect(() => {
    if (!event) {
      setRoundPointsDraft({})
      return
    }

    const nextRoundPointsDraft: Record<string, number> = {}
    event.teams.forEach((team) => {
      event.rounds.forEach((round) => {
        const key = `${team.id}-${round.id}`
        const currentPoints =
          event.teamPoints.find((teamPoint) => teamPoint.roundId === round.id && teamPoint.teamId === team.id)
            ?.points ?? 0
        nextRoundPointsDraft[key] = currentPoints
      })
    })

    setRoundPointsDraft(nextRoundPointsDraft)
  }, [event])

  if (loading) {
    return (
      <div className="flex-grow bg-gray-100 dark:bg-gray-900 flex items-center justify-center py-10">
        <div className="text-xl text-gray-900 dark:text-white">Loading team points...</div>
      </div>
    )
  }

  if (!event) {
    return (
      <div className="flex-grow bg-gray-100 dark:bg-gray-900 flex items-center justify-center py-10">
        <div className="text-xl text-gray-900 dark:text-white">Team points not found</div>
      </div>
    )
  }

  return (
    <div className="relative bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
      {isSaving && (
        <div className="absolute inset-0 z-50 flex items-center justify-center bg-white/75 dark:bg-gray-900/75">
          <div className="text-lg font-semibold text-gray-900 dark:text-white">Saving...</div>
        </div>
      )}
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-2xl font-semibold">Team Points</h2>
        <button
          type="button"
          onClick={handleSaveRoundPoints}
          disabled={isSaving || event.teams.length === 0 || event.rounds.length === 0}
          className="bg-indigo-600 hover:bg-indigo-700 text-white font-bold py-2 px-4 rounded disabled:bg-gray-400"
        >
          Save Points
        </button>
      </div>

      {event.teams.length === 0 || event.rounds.length === 0 ? (
        <p className="text-gray-500 dark:text-gray-400">
          Add teams and rounds to set points.
        </p>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full border border-gray-200 dark:border-gray-700">
            <thead>
              <tr className="bg-gray-100 dark:bg-gray-700">
                <th className="px-4 py-2 border border-gray-200 dark:border-gray-600 text-left">
                  Team
                </th>
                {event.rounds.map((round) => (
                  <th
                    key={round.id}
                    className="px-4 py-2 border border-gray-200 dark:border-gray-600 text-right"
                  >
                    {round.name}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {event.teams.map((team) => (
                <tr key={team.id} className="odd:bg-white even:bg-gray-50 dark:odd:bg-gray-800 dark:even:bg-gray-700/50">
                  <td className="px-4 py-2 border border-gray-200 dark:border-gray-600 font-medium">
                    {team.name}
                  </td>
                  {event.rounds.map((round) => (
                    <td
                      key={`${team.id}-${round.id}`}
                      className="px-4 py-2 border border-gray-200 dark:border-gray-600 text-right"
                    >
                      <input
                        type="number"
                        value={roundPointsDraft[`${team.id}-${round.id}`] ?? 0}
                        onChange={(e) => handleRoundPointInputChange(team.id, round.id, e.target.value)}
                        className="shadow appearance-none border dark:border-gray-700 rounded w-20 py-1 px-2 text-right text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-700 leading-tight focus:outline-none focus:shadow-outline"
                      />
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}

export default TeamPointsView