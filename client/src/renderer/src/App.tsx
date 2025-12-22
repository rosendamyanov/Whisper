import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import Login from './pages/Login'
import Register from './pages/Register'
import { Home } from './pages/Home'
import { JSX } from 'react'

function App(): JSX.Element {
  return (
    <Router>
      <div className="h-screen w-screen overflow-hidden">
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/home" element={<Home />} />
          <Route path="/register" element={<Register/>} />
          <Route path="/" element={<Navigate to="/login" replace />} />
        </Routes>
      </div>
    </Router>
  )
}

export default App