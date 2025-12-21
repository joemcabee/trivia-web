import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { eventApi, EventDetails, Round, Category, Question } from '../services/api'

function EventManagement() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [event, setEvent] = useState<EventDetails | null>(null)
  const [loading, setLoading] = useState(true)
  const [selectedRound, setSelectedRound] = useState<number | null>(null)
  const [selectedCategory, setSelectedCategory] = useState<number | null>(null)
  const [showCategoryModal, setShowCategoryModal] = useState(false)
  const [showQuestionModal, setShowQuestionModal] = useState(false)
  const [categoryName, setCategoryName] = useState('')
  const [questionText, setQuestionText] = useState('')
  const [answer, setAnswer] = useState('')
  const [questionImage, setQuestionImage] = useState<File | null>(null)
  const [questionImageUrl, setQuestionImageUrl] = useState<string | null>(null)
  const [uploadingImage, setUploadingImage] = useState(false)

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
      await eventApi.createQuestion(
        questionText,
        answer,
        selectedCategory,
        questionImageUrl || undefined
      )
      setQuestionText('')
      setAnswer('')
      setQuestionImage(null)
      setQuestionImageUrl(null)
      setShowQuestionModal(false)
      loadEvent()
    } catch (error) {
      console.error('Failed to create question:', error)
      alert('Failed to create question')
    }
  }

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (file) {
      setQuestionImage(file)
      handleImageUpload(file)
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

        <h1 className="text-4xl font-bold text-gray-900 mb-2">{event.name}</h1>
        <p className="text-gray-600 mb-6">{event.description}</p>

        <div className="bg-white rounded-lg shadow-md p-6">
          <div className="mb-6">
            <h2 className="text-2xl font-semibold mb-4">Rounds</h2>
            <div className="flex gap-2 flex-wrap">
              {event.rounds.map((round) => (
                <button
                  key={round.id}
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
              ))}
            </div>
          </div>

          {currentRound && (
            <>
              <div className="mb-6">
                <div className="flex justify-between items-center mb-4">
                  <h2 className="text-2xl font-semibold">Categories</h2>
                  <button
                    onClick={() => setShowCategoryModal(true)}
                    className="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
                  >
                    Add Category
                  </button>
                </div>
                <div className="flex gap-2 flex-wrap">
                  {currentRound.categories.map((category) => (
                    <button
                      key={category.id}
                      onClick={() => setSelectedCategory(category.id)}
                      className={`px-4 py-2 rounded ${
                        selectedCategory === category.id
                          ? 'bg-green-600 text-white'
                          : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                      }`}
                    >
                      {category.name}
                    </button>
                  ))}
                </div>
              </div>

              {currentCategory && (
                <div>
                  <div className="flex justify-between items-center mb-4">
                    <h2 className="text-2xl font-semibold">
                      Questions - {currentCategory.name}
                    </h2>
                    <button
                      onClick={() => setShowQuestionModal(true)}
                      className="bg-green-600 hover:bg-green-700 text-white font-bold py-2 px-4 rounded"
                    >
                      Add Question
                    </button>
                  </div>
                  <div className="space-y-4">
                    {currentCategory.questions.map((question) => (
                      <div
                        key={question.id}
                        className="border rounded-lg p-4 bg-gray-50"
                      >
                        <p className="text-lg font-medium mb-2">{question.questionText}</p>
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
            </>
          )}
        </div>

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
              <h2 className="text-2xl font-bold mb-4">Add Question</h2>
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
                      setQuestionImage(null)
                      setQuestionImageUrl(null)
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
                    Create
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

