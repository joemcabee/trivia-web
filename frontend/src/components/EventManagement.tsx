import { useEffect, useMemo, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { eventApi, EventDetails, Question } from '../services/api'

function EventManagement() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [event, setEvent] = useState<EventDetails | null>(null)
  const [loading, setLoading] = useState(true)
  const [selectedRound, setSelectedRound] = useState<number | null>(null)
  const [selectedCategory, setSelectedCategory] = useState<number | null>(null)
  const [showEditEventModal, setShowEditEventModal] = useState(false)
  const [showRoundModal, setShowRoundModal] = useState(false)
  const [showCategoryModal, setShowCategoryModal] = useState(false)
  const [showQuestionModal, setShowQuestionModal] = useState(false)
  const [eventName, setEventName] = useState('')
  const [eventDescription, setEventDescription] = useState('')
  const [roundName, setRoundName] = useState('')
  const [categoryName, setCategoryName] = useState('')
  const [questionText, setQuestionText] = useState('')
  const [answer, setAnswer] = useState('')
  const [questionImageUrl, setQuestionImageUrl] = useState<string | null>(null)
  const [uploadingImage, setUploadingImage] = useState(false)
  const [editingQuestionId, setEditingQuestionId] = useState<number | null>(null)
  const [newTeamName, setNewTeamName] = useState('')
  const [editingTeamId, setEditingTeamId] = useState<number | null>(null)
  const [editingTeamName, setEditingTeamName] = useState('')
  const [roundPointsDraft, setRoundPointsDraft] = useState<Record<number, number>>({})

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

      if (data.rounds.length === 0) {
        setSelectedRound(null)
        setSelectedCategory(null)
        return
      }

      const roundToSelect = data.rounds.find((round) => round.id === selectedRound) ?? data.rounds[0]
      setSelectedRound(roundToSelect.id)

      const categoryToSelect =
        roundToSelect.categories.find((category) => category.id === selectedCategory) ??
        roundToSelect.categories[0]
      setSelectedCategory(categoryToSelect?.id ?? null)
    } catch (error) {
      console.error('Failed to load event:', error)
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

  const handleCreateRound = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!id) return

    try {
      const newRound = await eventApi.createRound(roundName, parseInt(id))
      setRoundName('')
      setShowRoundModal(false)
      await loadEvent()
      // Select the newly created round
      setSelectedRound(newRound.id)
      setSelectedCategory(null)
    } catch (error) {
      console.error('Failed to create round:', error)
      alert('Failed to create round')
    }
  }

  const handleCreateCategory = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!selectedRound) return

    try {
      await eventApi.createCategory(categoryName, selectedRound)
      setCategoryName('')
      setShowCategoryModal(false)
      loadEvent()
    } catch (error) {
      console.error('Failed to create category:', error)
      alert('Failed to create category')
    }
  }

  const handleImageUpload = async (file: File) => {
    try {
      setUploadingImage(true)
      const imageUrl = await eventApi.uploadImage(file)
      setQuestionImageUrl(imageUrl)
    } catch (error) {
      console.error('Failed to upload image:', error)
      alert('Failed to upload image')
    } finally {
      setUploadingImage(false)
    }
  }

  const handleCreateQuestion = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!selectedCategory) return

    try {
      if (editingQuestionId) {
        // Update existing question
        await eventApi.updateQuestion(
          editingQuestionId,
          questionText,
          answer,
          questionImageUrl || undefined
        )
      } else {
        // Create new question
        await eventApi.createQuestion(
          questionText,
          answer,
          selectedCategory,
          questionImageUrl || undefined
        )
      }
      setQuestionText('')
      setAnswer('')
      setQuestionImageUrl(null)
      setEditingQuestionId(null)
      setShowQuestionModal(false)
      loadEvent()
    } catch (error) {
      console.error('Failed to save question:', error)
      alert('Failed to save question')
    }
  }

  const handleEditQuestion = (question: Question) => {
    setQuestionText(question.questionText)
    setAnswer(question.answer)
    setQuestionImageUrl(question.imageUrl)
    setEditingQuestionId(question.id)
    setShowQuestionModal(true)
  }

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (file) {
      handleImageUpload(file)
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

  const handleDeleteQuestion = async (questionId: number, e: React.MouseEvent) => {
    e.stopPropagation()
    if (!confirm('Are you sure you want to delete this question?')) {
      return
    }

    try {
      await eventApi.deleteQuestion(questionId)
      loadEvent()
    } catch (error) {
      console.error('Failed to delete question:', error)
      alert('Failed to delete question')
    }
  }

  const handleDeleteRound = async (roundId: number, e: React.MouseEvent) => {
    e.stopPropagation()
    if (!confirm('Are you sure you want to delete this round? This will also delete all categories and questions in this round.')) {
      return
    }

    try {
      await eventApi.deleteRound(roundId)
      setSelectedRound(null)
      setSelectedCategory(null)
      loadEvent()
    } catch (error) {
      console.error('Failed to delete round:', error)
      alert('Failed to delete round')
    }
  }

  const handleDeleteCategory = async (categoryId: number, e: React.MouseEvent) => {
    e.stopPropagation()
    if (!confirm('Are you sure you want to delete this category? This will also delete all questions in this category.')) {
      return
    }

    try {
      await eventApi.deleteCategory(categoryId)
      setSelectedCategory(null)
      loadEvent()
    } catch (error) {
      console.error('Failed to delete category:', error)
      alert('Failed to delete category')
    }
  }

  const handleCreateTeam = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!id || !event) return

    const validationMessage = validateTeamName(newTeamName)
    if (validationMessage) {
      alert(validationMessage)
      return
    }

    try {
      await eventApi.createTeam(newTeamName.trim(), parseInt(id))
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

  const handleRoundPointInputChange = (teamId: number, value: string) => {
    const parsedValue = Number.parseInt(value, 10)
    setRoundPointsDraft((previous) => ({
      ...previous,
      [teamId]: Number.isNaN(parsedValue) ? 0 : parsedValue,
    }))
  }

  const handleSaveRoundPoints = async () => {
    if (!event || !selectedRound || event.teams.length === 0) return

    const teamPoints = event.teams.map((team) => ({
      teamId: team.id,
      points: roundPointsDraft[team.id] ?? 0,
    }))

    try {
      await eventApi.setRoundTeamPoints(selectedRound, teamPoints)
      await loadEvent()
    } catch (error) {
      console.error('Failed to save team points:', error)
      alert(getApiErrorMessage(error, 'Failed to save team points'))
    }
  }

  useEffect(() => {
    if (!event || !selectedRound) {
      setRoundPointsDraft({})
      return
    }

    const nextRoundPointsDraft = event.teams.reduce<Record<number, number>>((accumulator, team) => {
      const currentPoints =
        event.teamPoints.find((teamPoint) => teamPoint.roundId === selectedRound && teamPoint.teamId === team.id)
          ?.points ?? 0
      accumulator[team.id] = currentPoints
      return accumulator
    }, {})

    setRoundPointsDraft(nextRoundPointsDraft)
  }, [event, selectedRound])

  const currentRound = event?.rounds.find((round) => round.id === selectedRound) ?? null
  const currentCategory = currentRound?.categories.find((category) => category.id === selectedCategory) ?? null
  const teamRoundPointLookup = useMemo(() => {
    const lookup = new Map<string, number>()
    if (!event) {
      return lookup
    }

    event.teamPoints.forEach((teamPoint) => {
      lookup.set(`${teamPoint.teamId}:${teamPoint.roundId}`, teamPoint.points)
    })
    return lookup
  }, [event])

  const scoreboardRows = useMemo(() => {
    if (!event) {
      return []
    }

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
      <div className="flex-grow bg-gray-100 dark:bg-gray-900 flex items-center justify-center">
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

        {/* Teams Section */}
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 mb-6">
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

        {/* Rounds Section */}
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 mb-6">
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-2xl font-semibold">Rounds</h2>
            <button
              onClick={() => setShowRoundModal(true)}
              className="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
            >
              Add Round
            </button>
          </div>
          <div className="flex gap-2 flex-wrap">
            {event.rounds.map((round) => (
              <div key={round.id} className="relative inline-block">
                <button
                  onClick={() => {
                    setSelectedRound(round.id)
                    if (round.categories.length > 0) {
                      setSelectedCategory(round.categories[0].id)
                    } else {
                      setSelectedCategory(null)
                    }
                  }}
                  className={`px-4 py-2 rounded ${
                    selectedRound === round.id
                      ? 'bg-blue-600 text-white'
                      : 'bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-200 hover:bg-gray-300 dark:hover:bg-gray-600'
                  }`}
                >
                  {round.name}
                </button>
                <button
                  onClick={(e) => handleDeleteRound(round.id, e)}
                  className="absolute -top-2 -right-2 bg-red-600 hover:bg-red-700 text-white text-xs font-bold rounded-full w-5 h-5 flex items-center justify-center"
                  title="Delete round"
                >
                  ×
                </button>
              </div>
            ))}
          </div>
        </div>

        {/* Categories Section */}
        {currentRound && (
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 mb-6">
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-2xl font-semibold">Categories - {currentRound.name}</h2>
              <button
                onClick={() => setShowCategoryModal(true)}
                className="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
              >
                Add Category
              </button>
            </div>
            <div className="flex gap-2 flex-wrap">
              {currentRound.categories.map((category) => (
                <div key={category.id} className="relative inline-block">
                  <button
                    onClick={() => setSelectedCategory(category.id)}
                    className={`px-4 py-2 rounded ${
                      selectedCategory === category.id
                        ? 'bg-green-600 text-white'
                        : 'bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-200 hover:bg-gray-300 dark:hover:bg-gray-600'
                    }`}
                  >
                    {category.name}
                  </button>
                  <button
                    onClick={(e) => handleDeleteCategory(category.id, e)}
                    className="absolute -top-2 -right-2 bg-red-600 hover:bg-red-700 text-white text-xs font-bold rounded-full w-5 h-5 flex items-center justify-center"
                    title="Delete category"
                  >
                    ×
                  </button>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Questions Section */}
        {currentCategory && (
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 mb-6">
              <div className="flex justify-between items-center mb-4">
                <h2 className="text-2xl font-semibold">
                  Questions - {currentCategory.name}
                </h2>
                <button
                    onClick={() => {
                      setEditingQuestionId(null)
                      setQuestionText('')
                      setAnswer('')
                      setQuestionImageUrl(null)
                      setShowQuestionModal(true)
                    }}
                    className="bg-green-600 hover:bg-green-700 text-white font-bold py-2 px-4 rounded"
                >
                  Add Question
                </button>
              </div>
              <div className="space-y-4">
                {currentCategory.questions.map((question) => (
                    <div
                        key={question.id}
                        className="border dark:border-gray-700 rounded-lg p-4 bg-gray-50 dark:bg-gray-700/50 relative"
                    >
                      <div className="absolute top-2 right-2 flex gap-2">
                        <button
                            onClick={() => handleEditQuestion(question)}
                            className="text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-300 text-sm font-medium"
                            title="Edit question"
                        >
                          Edit
                        </button>
                        <button
                            onClick={(e) => handleDeleteQuestion(question.id, e)}
                            className="text-red-600 dark:text-red-400 hover:text-red-800 dark:hover:text-red-300 text-sm font-medium"
                            title="Delete question"
                        >
                          Delete
                        </button>
                      </div>
                      <p className="text-lg font-medium mb-2 pr-24">{question.questionText}</p>
                      <p className="text-gray-600 dark:text-gray-400">Answer: {question.answer}</p>
                      {question.imageUrl ? (
                          <img
                              src={`http://localhost:5000${question.imageUrl}`}
                              alt="Question"
                              className="mt-2 max-w-xs rounded shadow dark:shadow-gray-900"
                          />
                      ) : (
                          <div className="mt-2 text-gray-400 text-sm">No image</div>
                      )}
                    </div>
                ))}
              </div>
            </div>
        )}

        {/* Round Points Section */}
        {currentRound && (
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 mb-6">
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-2xl font-semibold">Team Points - {currentRound.name}</h2>
              <button
                type="button"
                onClick={handleSaveRoundPoints}
                disabled={event.teams.length === 0}
                className="bg-indigo-600 hover:bg-indigo-700 text-white font-bold py-2 px-4 rounded disabled:bg-gray-400"
              >
                Save Points
              </button>
            </div>

            {event.teams.length === 0 ? (
              <p className="text-gray-500 dark:text-gray-400">
                Add teams to set round points.
              </p>
            ) : (
              <div className="space-y-3">
                {event.teams.map((team) => (
                  <div
                    key={team.id}
                    className="flex items-center justify-between rounded border dark:border-gray-700 px-3 py-2 bg-gray-50 dark:bg-gray-700/50"
                  >
                    <span className="font-medium">{team.name}</span>
                    <input
                      type="number"
                      value={roundPointsDraft[team.id] ?? 0}
                      onChange={(e) => handleRoundPointInputChange(team.id, e.target.value)}
                      className="shadow appearance-none border dark:border-gray-700 rounded w-28 py-1 px-2 text-right text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-700 leading-tight focus:outline-none focus:shadow-outline"
                    />
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {/* Scoreboard Section */}
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

        {showRoundModal && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white dark:bg-gray-800 rounded-lg p-6 max-w-md w-full mx-4">
              <h2 className="text-2xl font-bold mb-4 text-gray-900 dark:text-white">Add Round</h2>
              <form onSubmit={handleCreateRound}>
                <div className="mb-4">
                  <label className="block text-gray-700 dark:text-gray-300 text-sm font-bold mb-2">
                    Round Name
                  </label>
                  <input
                    type="text"
                    value={roundName}
                    onChange={(e) => setRoundName(e.target.value)}
                    className="shadow appearance-none border dark:border-gray-700 rounded w-full py-2 px-3 text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-700 leading-tight focus:outline-none focus:shadow-outline"
                    required
                  />
                </div>
                <div className="flex justify-end gap-2">
                  <button
                    type="button"
                    onClick={() => {
                      setShowRoundModal(false)
                      setRoundName('')
                    }}
                    className="bg-gray-300 dark:bg-gray-600 hover:bg-gray-400 dark:hover:bg-gray-500 text-gray-800 dark:text-white font-bold py-2 px-4 rounded"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    className="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
                  >
                    Create
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}

        {showCategoryModal && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white dark:bg-gray-800 rounded-lg p-6 max-w-md w-full mx-4">
              <h2 className="text-2xl font-bold mb-4 text-gray-900 dark:text-white">Add Category</h2>
              <form onSubmit={handleCreateCategory}>
                <div className="mb-4">
                  <label className="block text-gray-700 dark:text-gray-300 text-sm font-bold mb-2">
                    Category Name
                  </label>
                  <input
                    type="text"
                    value={categoryName}
                    onChange={(e) => setCategoryName(e.target.value)}
                    className="shadow appearance-none border dark:border-gray-700 rounded w-full py-2 px-3 text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-700 leading-tight focus:outline-none focus:shadow-outline"
                    required
                  />
                </div>
                <div className="flex justify-end gap-2">
                  <button
                    type="button"
                    onClick={() => {
                      setShowCategoryModal(false)
                      setCategoryName('')
                    }}
                    className="bg-gray-300 dark:bg-gray-600 hover:bg-gray-400 dark:hover:bg-gray-500 text-gray-800 dark:text-white font-bold py-2 px-4 rounded"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    className="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
                  >
                    Create
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}

        {showQuestionModal && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white dark:bg-gray-800 rounded-lg p-6 max-w-md w-full mx-4 max-h-[90vh] overflow-y-auto">
              <h2 className="text-2xl font-bold mb-4 text-gray-900 dark:text-white">
                {editingQuestionId ? 'Edit Question' : 'Add Question'}
              </h2>
              <form onSubmit={handleCreateQuestion}>
                <div className="mb-4">
                  <label className="block text-gray-700 dark:text-gray-300 text-sm font-bold mb-2">
                    Question
                  </label>
                  <textarea
                    value={questionText}
                    onChange={(e) => setQuestionText(e.target.value)}
                    className="shadow appearance-none border dark:border-gray-700 rounded w-full py-2 px-3 text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-700 leading-tight focus:outline-none focus:shadow-outline"
                    rows={3}
                    required
                  />
                </div>
                <div className="mb-4">
                  <label className="block text-gray-700 dark:text-gray-300 text-sm font-bold mb-2">
                    Answer
                  </label>
                  <textarea
                    value={answer}
                    onChange={(e) => setAnswer(e.target.value)}
                    className="shadow appearance-none border dark:border-gray-700 rounded w-full py-2 px-3 text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-700 leading-tight focus:outline-none focus:shadow-outline"
                    rows={2}
                    required
                  />
                </div>
                <div className="mb-4">
                  <label className="block text-gray-700 dark:text-gray-300 text-sm font-bold mb-2">
                    Image (optional)
                  </label>
                  <input
                    type="file"
                    accept="image/*"
                    onChange={handleImageChange}
                    className="shadow appearance-none border dark:border-gray-700 rounded w-full py-2 px-3 text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-700 leading-tight focus:outline-none focus:shadow-outline"
                  />
                  {uploadingImage && <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">Uploading...</p>}
                  {questionImageUrl && (
                    <img
                      src={`http://localhost:5000${questionImageUrl}`}
                      alt="Preview"
                      className="mt-2 max-w-xs rounded shadow dark:shadow-gray-900"
                    />
                  )}
                </div>
                <div className="flex justify-end gap-2">
                  <button
                    type="button"
                    onClick={() => {
                      setShowQuestionModal(false)
                      setQuestionText('')
                      setAnswer('')
                      setQuestionImageUrl(null)
                      setEditingQuestionId(null)
                    }}
                    className="bg-gray-300 dark:bg-gray-600 hover:bg-gray-400 dark:hover:bg-gray-500 text-gray-800 dark:text-white font-bold py-2 px-4 rounded"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    disabled={uploadingImage}
                    className="bg-green-600 hover:bg-green-700 text-white font-bold py-2 px-4 rounded disabled:bg-gray-400"
                  >
                    {editingQuestionId ? 'Save' : 'Create'}
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

