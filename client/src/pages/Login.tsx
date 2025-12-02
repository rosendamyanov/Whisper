import { JSX, useState } from 'react'
import { useNavigate } from 'react-router-dom'

function Login(): JSX.Element {
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    // TODO: Add actual authentication later
    console.log('Login attempt:', { username, password })
    
    // For now, just navigate to home
    navigate('/home')
  }

  return (
    <div className="flex h-screen items-center justify-center bg-dark-950">
      <div className="w-full max-w-md rounded-lg bg-dark-800 p-8 shadow-xl">
        <div className="mb-8 text-center">
          <h1 className="text-3xl font-bold text-white mb-2">
            Welcome to Whisper
          </h1>
          <p className="text-gray-400 text-sm">
            Sign in to continue
          </p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-300 mb-2">
              Username or Email
            </label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="w-full px-4 py-2 bg-dark-900 border border-dark-700 rounded-lg text-white 
                       focus:outline-none focus:border-primary-500 transition-colors"
              placeholder="Enter your username"
              required
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-300 mb-2">
              Password
            </label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full px-4 py-2 bg-dark-900 border border-dark-700 rounded-lg text-white 
                       focus:outline-none focus:border-primary-500 transition-colors"
              placeholder="Enter your password"
              required
            />
          </div>

          <button
            type="submit"
            className="w-full bg-primary-500 hover:bg-primary-600 text-white font-semibold 
                     py-3 px-4 rounded-lg transition-colors"
          >
            Login
          </button>
        </form>

        <div className="mt-6 text-center">
          <p className="text-gray-400 text-sm">
            Don't have an account?{' '}
            <button className="text-primary-500 hover:text-primary-400 transition-colors font-medium">
              Register
            </button>
          </p>
        </div>
      </div>
    </div>
  )
}

export default Login