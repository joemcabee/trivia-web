import { useEffect, useState, useCallback } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { eventApi, EventDetails, PresentationData, PresentationMode, resolveImageUrl } from '../services/api'

function Presentation() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [eventDetails, setEventDetails] = useState<EventDetails | null>(null)
  const [presentationData, setPresentationData] = useState<PresentationData | null>(null)
  const [currentSlideIndex, setCurrentSlideIndex] = useState(0)
  const [loading, setLoading] = useState(true)
  const [isPresenting, setIsPresenting] = useState(false)
  const [selectedRoundId, setSelectedRoundId] = useState<number | null>(null)
  const [mode, setMode] = useState<PresentationMode>('QuestionsOnly')
  const [windowHeight, setWindowHeight] = useState(window.innerHeight)

  useEffect(() => {
    if (id) {
      loadEventDetails()
    }
  }, [id])

  useEffect(() => {
    const handleResize = () => {
      setWindowHeight(window.innerHeight)
    }
    window.addEventListener('resize', handleResize)
    return () => window.removeEventListener('resize', handleResize)
  }, [])

  useEffect(() => {
    const handleKeyPress = (e: KeyboardEvent) => {
      if (!isPresenting) {
        if (e.key === 'Escape') {
          navigate(`/event/${id}`)
          e.preventDefault()
        }
        return
      }

      if (e.key === 'ArrowRight' || e.key === 'ArrowDown') {
        setCurrentSlideIndex((prev) => {
          if (presentationData && prev < presentationData.slides.length - 1) {
            return prev + 1
          }
          return prev
        })
      } else if (e.key === 'ArrowLeft' || e.key === 'ArrowUp') {
        setCurrentSlideIndex((prev) => {
          if (prev > 0) {
            return prev - 1
          }
          return prev
        })
      } else if (e.key === 'Escape') {
        navigate(`/event/${id}`)
        e.preventDefault()
      }
    }

    window.addEventListener('keydown', handleKeyPress)
    return () => window.removeEventListener('keydown', handleKeyPress)
  }, [presentationData, navigate, id, isPresenting])

  const loadEventDetails = async () => {
    try {
      setLoading(true)
      const details = await eventApi.getEventDetails(parseInt(id!))
      setEventDetails(details)

      const firstRoundId = details.rounds.slice().sort((a, b) => a.order - b.order)[0]?.id ?? null
      setSelectedRoundId(firstRoundId)
    } catch (error) {
      console.error('Failed to load event details:', error)
    } finally {
      setLoading(false)
    }
  }

  const startRoundPresentation = async (roundId: number, presentationMode: PresentationMode) => {
    try {
      setLoading(true)
      const data = await eventApi.getRoundPresentationData(parseInt(id!), roundId, presentationMode)
      setPresentationData(data)
      setCurrentSlideIndex(0)
      setIsPresenting(true)
    } catch (error) {
      console.error('Failed to load presentation data:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleNextSlide = useCallback(() => {
    setCurrentSlideIndex((prev) => {
      if (presentationData && prev < presentationData.slides.length - 1) {
        return prev + 1
      }
      return prev
    })
  }, [presentationData])

  const handlePreviousSlide = useCallback(() => {
    setCurrentSlideIndex((prev) => {
      if (prev > 0) {
        return prev - 1
      }
      return prev
    })
  }, [])

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-100 dark:bg-gray-900 flex items-center justify-center">
        <div className="text-gray-900 dark:text-white text-xl">Loading...</div>
      </div>
    )
  }

  if (!eventDetails || eventDetails.rounds.length === 0) {
    return (
      <div className="min-h-screen bg-gray-100 dark:bg-gray-900 flex items-center justify-center">
        <div className="text-gray-900 dark:text-white text-xl">No rounds available</div>
      </div>
    )
  }

  if (!isPresenting) {
    const roundsSorted = eventDetails.rounds.slice().sort((a, b) => a.order - b.order)

    return (
      <div className="min-h-screen bg-gray-100 dark:bg-gray-900 text-gray-900 dark:text-white flex flex-col">
        <div className="flex-1 flex items-center justify-center p-4 md:p-6">
          <div className="w-full max-w-2xl">
            <div className="bg-white dark:bg-gray-800 rounded-lg p-6 md:p-10 shadow-2xl">
              <div className="text-2xl font-bold mb-2">Presentation</div>
              <div className="text-sm text-gray-500 dark:text-gray-400 mb-8">{eventDetails.name}</div>

              <div className="mb-6">
                <label className="block text-sm font-medium mb-2" htmlFor="presentation-round">
                  Round
                </label>
                <select
                  id="presentation-round"
                  className="w-full bg-white dark:bg-gray-900 border border-gray-300 dark:border-gray-700 rounded-lg px-3 py-2.5 shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                  value={selectedRoundId ?? ''}
                  onChange={(e) => setSelectedRoundId(e.target.value ? Number(e.target.value) : null)}
                >
                  {roundsSorted.map((r) => (
                    <option key={r.id} value={r.id}>
                      {r.name}
                    </option>
                  ))}
                </select>
              </div>

              <div className="mb-8">
                <fieldset>
                  <legend className="text-sm font-medium mb-2">Mode</legend>
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                    <label
                      className={`cursor-pointer rounded-lg border px-4 py-3 transition shadow-sm ${
                        mode === 'QuestionsOnly'
                          ? 'border-blue-500 bg-blue-50 dark:bg-blue-950/30'
                          : 'border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-900 hover:bg-gray-50 dark:hover:bg-gray-800'
                      }`}
                    >
                      <div className="flex items-start gap-3">
                        <input
                          type="radio"
                          name="mode"
                          className="mt-1"
                          checked={mode === 'QuestionsOnly'}
                          onChange={() => setMode('QuestionsOnly')}
                        />
                        <div>
                          <div className="font-semibold">Questions Only</div>
                          <div className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">Question, question, question…</div>
                        </div>
                      </div>
                    </label>
                    <label
                      className={`cursor-pointer rounded-lg border px-4 py-3 transition shadow-sm ${
                        mode === 'QuestionAnswer'
                          ? 'border-blue-500 bg-blue-50 dark:bg-blue-950/30'
                          : 'border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-900 hover:bg-gray-50 dark:hover:bg-gray-800'
                      }`}
                    >
                      <div className="flex items-start gap-3">
                        <input
                          type="radio"
                          name="mode"
                          className="mt-1"
                          checked={mode === 'QuestionAnswer'}
                          onChange={() => setMode('QuestionAnswer')}
                        />
                        <div>
                          <div className="font-semibold">Questions + Answers</div>
                          <div className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">Question, answer, question, answer…</div>
                        </div>
                      </div>
                    </label>
                  </div>
                </fieldset>
              </div>

              <div className="flex gap-3">
                <button
                  onClick={() => selectedRoundId && startRoundPresentation(selectedRoundId, mode)}
                  disabled={!selectedRoundId}
                  className="bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white font-bold py-2 px-4 rounded"
                >
                  Start
                </button>
                <button
                  onClick={() => navigate(`/event/${id}`)}
                  className="bg-gray-200 dark:bg-gray-700 hover:bg-gray-300 dark:hover:bg-gray-600 text-gray-700 dark:text-white font-bold py-2 px-4 rounded"
                >
                  Back
                </button>
              </div>
              <div className="text-xs text-gray-400 dark:text-gray-500 mt-6">Tip: press Esc to exit.</div>
            </div>
          </div>
        </div>
      </div>
    )
  }

  if (!presentationData || presentationData.slides.length === 0) {
    return (
      <div className="min-h-screen bg-gray-100 dark:bg-gray-900 flex items-center justify-center">
        <div className="text-gray-900 dark:text-white text-xl">No slides available</div>
      </div>
    )
  }

  const currentSlide = presentationData.slides[currentSlideIndex]
  const isAnswerSlide = currentSlide.type === 'answer'

  // Show image if we have the URL and enough screen space
  // Hide image if screen is too small (less than 700px) or if answer is showing on smaller screens
  const hasImageUrl = !!currentSlide.imageUrl
  const showImage = hasImageUrl && windowHeight > 700 && (!isAnswerSlide || windowHeight > 900)

  return (
    <div className="min-h-screen bg-gray-100 dark:bg-gray-900 text-gray-900 dark:text-white flex flex-col">
      <div className="flex-1 flex items-center justify-center p-4 md:p-6">
        <div className="w-full max-w-[96vw]">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 md:p-10 shadow-2xl min-h-[75vh] flex flex-col">
            <div className="mb-4">
              <div className="text-sm text-gray-500 dark:text-gray-400 mb-2">
                {currentSlide.roundName || 'Round'} • {currentSlide.categoryName || 'Category'} • Question {currentSlide.questionNumber || 0}
              </div>
              <div className="text-xs text-gray-400 dark:text-gray-500">
                Slide {currentSlideIndex + 1} of {presentationData.slides.length}
              </div>
            </div>

            <div className="flex-1 flex flex-col items-center justify-center">
              <div className="w-full text-center mb-6">
                <h2
                  className={`text-5xl md:text-6xl lg:text-7xl font-bold mb-6 ${
                    isAnswerSlide ? 'text-blue-600 dark:text-blue-400' : 'text-gray-900 dark:text-white'
                  }`}
                  style={{ lineHeight: '1.5' }}
                >
                  {currentSlide.questionText}
                </h2>

                {isAnswerSlide && (
                  <div className="mt-6">
                    <p
                      className="text-4xl md:text-5xl lg:text-6xl font-semibold text-green-600 dark:text-green-400"
                      style={{ lineHeight: '1.5' }}
                    >
                      {currentSlide.answer}
                    </p>
                  </div>
                )}
              </div>

              {showImage ? (
                <div className="mt-6 flex justify-center">
                  <img
                    src={resolveImageUrl(currentSlide.imageUrl) ?? ''}
                    alt="Question"
                    className="max-w-md max-h-64 object-contain rounded-lg shadow-lg"
                  />
                </div>
              ) : !hasImageUrl ? (
                <div className="mt-6 flex justify-center">
                  <div className="text-9xl text-gray-200 dark:text-gray-700">?</div>
                </div>
              ) : null}
            </div>
          </div>
        </div>
      </div>

      <div className="bg-white dark:bg-gray-800 p-4 border-t dark:border-gray-700 flex justify-between items-center">
        <button
          onClick={handlePreviousSlide}
          disabled={currentSlideIndex === 0}
          className="bg-gray-200 dark:bg-gray-700 hover:bg-gray-300 dark:hover:bg-gray-600 disabled:bg-gray-100 dark:disabled:bg-gray-800 disabled:text-gray-400 dark:disabled:text-gray-500 text-gray-700 dark:text-white font-bold py-2 px-4 rounded"
        >
          ← Previous
        </button>
        <div className="flex items-center gap-3">
          <button
            onClick={() => setIsPresenting(false)}
            className="bg-gray-200 dark:bg-gray-700 hover:bg-gray-300 dark:hover:bg-gray-600 text-gray-700 dark:text-white font-bold py-2 px-4 rounded"
          >
            Settings
          </button>
          <button
            onClick={handleNextSlide}
            disabled={currentSlideIndex === presentationData.slides.length - 1}
            className="bg-gray-200 dark:bg-gray-700 hover:bg-gray-300 dark:hover:bg-gray-600 disabled:bg-gray-100 dark:disabled:bg-gray-800 disabled:text-gray-400 dark:disabled:text-gray-500 text-gray-700 dark:text-white font-bold py-2 px-4 rounded"
          >
            Next →
          </button>
        </div>
      </div>
    </div>
  )
}

export default Presentation
