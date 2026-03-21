import { useEffect, useState, useCallback } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { eventApi, PresentationData } from '../services/api'

function Presentation() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [presentationData, setPresentationData] = useState<PresentationData | null>(null)
  const [currentSlideIndex, setCurrentSlideIndex] = useState(0)
  const [loading, setLoading] = useState(true)
  const [windowHeight, setWindowHeight] = useState(window.innerHeight)

  useEffect(() => {
    if (id) {
      loadPresentationData()
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
      }
    }

    window.addEventListener('keydown', handleKeyPress)
    return () => window.removeEventListener('keydown', handleKeyPress)
  }, [presentationData, navigate, id])

  const loadPresentationData = async () => {
    try {
      setLoading(true)
      const data = await eventApi.getPresentationData(parseInt(id!))
      setPresentationData(data)
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
      <div className="flex-1 flex items-center justify-center p-8">
        <div className="max-w-7xl w-full">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-8 shadow-2xl min-h-[60vh] flex flex-col">
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
                  className={`text-4xl font-bold mb-6 ${
                    isAnswerSlide ? 'text-blue-600 dark:text-blue-400' : 'text-gray-900 dark:text-white'
                  }`}
                  style={{ lineHeight: '1.5' }}
                >
                  {currentSlide.questionText}
                </h2>

                {isAnswerSlide && (
                  <div className="mt-6">
                    <p className="text-3xl font-semibold text-green-600 dark:text-green-400" style={{ lineHeight: '1.5' }}>
                      {currentSlide.answer}
                    </p>
                  </div>
                )}
              </div>

              {showImage ? (
                <div className="mt-6 flex justify-center">
                  <img
                    src={`http://localhost:5000${currentSlide.imageUrl}`}
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
        <button
          onClick={handleNextSlide}
          disabled={currentSlideIndex === presentationData.slides.length - 1}
          className="bg-gray-200 dark:bg-gray-700 hover:bg-gray-300 dark:hover:bg-gray-600 disabled:bg-gray-100 dark:disabled:bg-gray-800 disabled:text-gray-400 dark:disabled:text-gray-500 text-gray-700 dark:text-white font-bold py-2 px-4 rounded"
        >
          Next →
        </button>
      </div>
    </div>
  )
}

export default Presentation

