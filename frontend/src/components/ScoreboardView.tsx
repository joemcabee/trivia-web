import { useEffect, useMemo, useState } from 'react'
import { eventApi, EventDetails } from '../services/api'

interface ScoreboardViewProps {
  eventId: number
}

function ScoreboardView({ eventId }: ScoreboardViewProps) {
  const [event, setEvent] = useState<EventDetails | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    loadEvent()
  }, [eventId])

  const loadEvent = async () => {
    try {
      setLoading(true)
      const data = await eventApi.getEventDetails(eventId)
      setEvent(data)
    } catch (error) {
      console.error('Failed to load scoreboard:', error)
    } finally {
      setLoading(false)
    }
  }

  const teamRoundPointLookup = useMemo(() => {
    const lookup = new Map<string, number>()
    if (!event) return lookup
    event.teamPoints.forEach((teamPoint) => {
      lookup.set(`${teamPoint.teamId}:${teamPoint.roundId}`, teamPoint.points)
    })
    return lookup
  }, [event])

  const scoreboardRows = useMemo(() => {
    if (!event) return []

    return event.teams
      .map((team) => {
        const roundPoints = event.rounds.map(
          (round) => teamRoundPointLookup.get(`${team.id}:${round.id}`) ?? 0
        )
        const totalPoints = roundPoints.reduce((sum, points) => sum + points, 0)
        return {
          teamId: team.id,
          teamName: team.name,
          roundPoints,
          totalPoints,
        }
      })
      .sort((a, b) => b.totalPoints - a.totalPoints || a.teamName.localeCompare(b.teamName))
  }, [event, teamRoundPointLookup])

  if (loading) {
    return (
      <div className="flex-grow bg-gray-100 dark:bg-gray-900 flex items-center justify-center py-10">
        <div className="text-xl text-gray-900 dark:text-white">Loading scoreboard...</div>
      </div>
    )
  }

  if (!event) {
    return (
      <div className="flex-grow bg-gray-100 dark:bg-gray-900 flex items-center justify-center py-10">
        <div className="text-xl text-gray-900 dark:text-white">Scoreboard not found</div>
      </div>
    )
  }

  return (
    <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
      <h2 className="text-2xl font-semibold mb-4">Scoreboard</h2>

      {event.teams.length === 0 || event.rounds.length === 0 ? (
        <p className="text-gray-500 dark:text-gray-400">
          Add teams and rounds to view the scoreboard.
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
                <th className="px-4 py-2 border border-gray-200 dark:border-gray-600 text-right">
                  Total
                </th>
              </tr>
            </thead>
            <tbody>
              {scoreboardRows.map((row) => (
                <tr key={row.teamId} className="odd:bg-white even:bg-gray-50 dark:odd:bg-gray-800 dark:even:bg-gray-700/50">
                  <td className="px-4 py-2 border border-gray-200 dark:border-gray-600 font-medium">
                    {row.teamName}
                  </td>
                  {row.roundPoints.map((roundPoints, index) => (
                    <td
                      key={`${row.teamId}-${event.rounds[index].id}`}
                      className="px-4 py-2 border border-gray-200 dark:border-gray-600 text-right"
                    >
                      {roundPoints}
                    </td>
                  ))}
                  <td className="px-4 py-2 border border-gray-200 dark:border-gray-600 text-right font-bold">
                    {row.totalPoints}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}

export default ScoreboardView