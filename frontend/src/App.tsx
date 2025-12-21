import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import Dashboard from './components/Dashboard'
import EventManagement from './components/EventManagement'
import Presentation from './components/Presentation'

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Dashboard />} />
        <Route path="/event/:id" element={<EventManagement />} />
        <Route path="/event/:id/present" element={<Presentation />} />
      </Routes>
    </Router>
  )
}

export default App

