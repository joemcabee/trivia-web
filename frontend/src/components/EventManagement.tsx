import { useEffect, useState } from 'react'
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
      if (data.rounds.length > 0 && !selectedRound) {
        setSelectedRound(data.rounds[0].id)
        if (data.rounds[0].categories.length > 0) {
          setSelectedCategory(data.rounds[0].categories[0].id)
        }
      }
    } catch (error) {
      console.error('Failed to load event:', error)
    } finally {
      setLoading(false)
    }
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

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-100 flex items-center justify-center">
        <div className="text-xl">Loading...</div>
      </div>
    )
  }

  if (!event) {
    return (
      <div className="min-h-screen bg-gray-100 flex items-center justify-center">
        <div className="text-xl">Event not found</div>
      </div>
    )
  }

  const currentRound = event.rounds.find((r) => r.id === selectedRound)
  const currentCategory = currentRound?.categories.find((c) => c.id === selectedCategory)

  return (
    <div className="min-h-screen bg-gray-100 py-8">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="mb-6 flex justify-between items-center">
          <button
            onClick={() => navigate('/')}
            className="text-blue-600 hover:text-blue-800 font-medium"
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
            <h1 className="text-4xl font-bold text-gray-900">{event.name}</h1>
            <button
              onClick={handleEditEvent}
              className="text-blue-600 hover:text-blue-800 text-sm font-medium"
              title="Edit event name and description"
            >
              ✏️ Edit
            </button>
          </div>
          <p className="text-gray-600">{event.description}</p>
        </div>

        {/* Rounds Section */}
        <div className="bg-white rounded-lg shadow-md p-6 mb-6">
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
                      : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
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
          <div className="bg-white rounded-lg shadow-md p-6 mb-6">
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
                        : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
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
          <div className="bg-white rounded-lg shadow-md p-6">
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
                  className="border rounded-lg p-4 bg-gray-50 relative"
                >
                  <div className="absolute top-2 right-2 flex gap-2">
                    <button
                      onClick={() => handleEditQuestion(question)}
                      className="text-blue-600 hover:text-blue-800 text-sm font-medium"
                      title="Edit question"
                    >
                      Edit
                    </button>
                    <button
                      onClick={(e) => handleDeleteQuestion(question.id, e)}
                      className="text-red-600 hover:text-red-800 text-sm font-medium"
                      title="Delete question"
                    >
                      Delete
                    </button>
                  </div>
                  <p className="text-lg font-medium mb-2 pr-24">{question.questionText}</p>
                  <p className="text-gray-600">Answer: {question.answer}</p>
                  {question.imageUrl ? (
                    <img
                      src={`http://localhost:5000${question.imageUrl}`}
                      alt="Question"
                      className="mt-2 max-w-xs rounded"
                    />
                  ) : (
                    <div className="mt-2 text-gray-400 text-sm">No image</div>
                  )}
                </div>
              ))}
            </div>
          </div>
        )}

        {showEditEventModal && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
              <h2 className="text-2xl font-bold mb-4">Edit Event</h2>
              <form onSubmit={handleUpdateEvent}>
                <div className="mb-4">
                  <label className="block text-gray-700 text-sm font-bold mb-2">
                    Event Name
                  </label>
                  <input
                    type="text"
                    value={eventName}
                    onChange={(e) => setEventName(e.target.value)}
                    className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
                    required
                  />
                </div>
                <div className="mb-4">
                  <label className="block text-gray-700 text-sm font-bold mb-2">
                    Description
                  </label>
                  <textarea
                    value={eventDescription}
                    onChange={(e) => setEventDescription(e.target.value)}
                    className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
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
                    className="bg-gray-300 hover:bg-gray-400 text-gray-800 font-bold py-2 px-4 rounded"
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
            <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
              <h2 className="text-2xl font-bold mb-4">Add Round</h2>
              <form onSubmit={handleCreateRound}>
                <div className="mb-4">
                  <label className="block text-gray-700 text-sm font-bold mb-2">
                    Round Name
                  </label>
                  <input
                    type="text"
                    value={roundName}
                    onChange={(e) => setRoundName(e.target.value)}
                    className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
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
                    className="bg-gray-300 hover:bg-gray-400 text-gray-800 font-bold py-2 px-4 rounded"
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
            <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
              <h2 className="text-2xl font-bold mb-4">Add Category</h2>
              <form onSubmit={handleCreateCategory}>
                <div className="mb-4">
                  <label className="block text-gray-700 text-sm font-bold mb-2">
                    Category Name
                  </label>
                  <input
                    type="text"
                    value={categoryName}
                    onChange={(e) => setCategoryName(e.target.value)}
                    className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
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
                    className="bg-gray-300 hover:bg-gray-400 text-gray-800 font-bold py-2 px-4 rounded"
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
            <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4 max-h-[90vh] overflow-y-auto">
              <h2 className="text-2xl font-bold mb-4">
                {editingQuestionId ? 'Edit Question' : 'Add Question'}
              </h2>
              <form onSubmit={handleCreateQuestion}>
                <div className="mb-4">
                  <label className="block text-gray-700 text-sm font-bold mb-2">
                    Question
                  </label>
                  <textarea
                    value={questionText}
                    onChange={(e) => setQuestionText(e.target.value)}
                    className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
                    rows={3}
                    required
                  />
                </div>
                <div className="mb-4">
                  <label className="block text-gray-700 text-sm font-bold mb-2">
                    Answer
                  </label>
                  <textarea
                    value={answer}
                    onChange={(e) => setAnswer(e.target.value)}
                    className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
                    rows={2}
                    required
                  />
                </div>
                <div className="mb-4">
                  <label className="block text-gray-700 text-sm font-bold mb-2">
                    Image (optional)
                  </label>
                  <input
                    type="file"
                    accept="image/*"
                    onChange={handleImageChange}
                    className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
                  />
                  {uploadingImage && <p className="text-sm text-gray-500 mt-1">Uploading...</p>}
                  {questionImageUrl && (
                    <img
                      src={`http://localhost:5000${questionImageUrl}`}
                      alt="Preview"
                      className="mt-2 max-w-xs rounded"
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
                    className="bg-gray-300 hover:bg-gray-400 text-gray-800 font-bold py-2 px-4 rounded"
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

