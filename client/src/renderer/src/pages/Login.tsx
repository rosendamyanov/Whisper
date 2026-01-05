import { JSX, useState, useRef } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { usersApi } from '../services/api/user'
import { useAuthStore } from '../stores/authStore'
import axios from 'axios'
import { KeyRound, UserRound, MessageSquareText } from 'lucide-react'

export default function Login(): JSX.Element {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [rememberMe, setRememberMe] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [mousePosition, setMousePosition] = useState({ x: 0, y: 0 })
  const [isHovering, setIsHovering] = useState(false)
  const cardRef = useRef<HTMLDivElement>(null)

  const navigate = useNavigate()
  const setSession = useAuthStore((state) => state.setSession)

  const handleMouseMove = (e: React.MouseEvent<HTMLDivElement>): void => {
    if (!cardRef.current) return
    const rect = cardRef.current.getBoundingClientRect()
    setMousePosition({
      x: e.clientX - rect.left,
      y: e.clientY - rect.top
    })
  }

  const handleSubmit = async (e: React.FormEvent): Promise<void> => {
    e.preventDefault()
    setError(null)
    setIsLoading(true)

    try {
      const result = await usersApi.login({ username, password })

      if (result.isSuccess && result.data) {
        setSession(result.data.user)
        navigate('/home')
      } else {
        setError(result.message || 'Login failed unexpectedly.')
      }
    } catch (err: unknown) {
      let errorMessage = 'An unexpected error occurred. Please try again.'

      if (axios.isAxiosError(err) && err.response) {
        const apiResponse = err.response.data

        if (apiResponse && apiResponse.message) {
          errorMessage = apiResponse.message
        } else {
          errorMessage = `Request failed with status ${err.response.status}.`
        }
      } else {
        errorMessage = 'Network connection failed.'
      }

      console.error('Login failed:', err)
      setError(errorMessage)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen w-full flex items-center justify-center relative overflow-hidden bg-slate-950">
      {/* Background Gradients */}
      <div className="absolute inset-0 z-0">
        <div
          className="absolute inset-0"
          style={{
            background:
              'radial-gradient(ellipse 80% 50% at 20% 20%, rgba(6, 182, 212, 0.15), transparent 50%)'
          }}
        />
        <div
          className="absolute inset-0"
          style={{
            background:
              'radial-gradient(ellipse 60% 60% at 80% 80%, rgba(8, 145, 178, 0.12), transparent 50%)'
          }}
        />
      </div>

      {/* Main Content Grid */}
      <div className="relative z-10 w-full max-w-6xl mx-auto flex flex-col lg:flex-row items-center justify-between p-6 lg:p-12">
        {/* LEFT SECTION: Hero Text and Branding */}
        <div className="lg:w-1/2 mb-12 lg:mb-0 text-center lg:text-left">
          <h1 className="text-7xl lg:text-8xl font-extrabold text-white leading-tight mb-4 tracking-tighter">
            The Silent <br className="hidden lg:inline" />
            <span className="text-transparent bg-clip-text bg-gradient-to-r from-cyan-400 to-teal-500">
              Whisper
            </span>
          </h1>
          <p className="text-xl text-gray-400 max-w-md mx-auto lg:mx-0 mb-8">
            Connect instantly and securely with friends and communities around the globe.
          </p>
          {/* CTA to understand the app */}
          <div className="flex justify-center lg:justify-start">
            <Link
              to="/about"
              className="inline-flex items-center gap-2 px-6 py-3 rounded-full bg-cyan-500/10 border border-cyan-500/30 text-cyan-400 font-semibold text-sm hover:bg-cyan-500/20 transition-colors"
            >
              <MessageSquareText className="w-4 h-4" />
              Explore Features
            </Link>
          </div>
        </div>

        {/* RIGHT SECTION: Login Card */}
        <div className="w-full max-w-md">
          {/* Card with glass effect and mouse-reactive glow */}
          <div
            ref={cardRef}
            className="relative rounded-[2rem] overflow-hidden shadow-2xl shadow-black/50"
            onMouseMove={handleMouseMove}
            onMouseEnter={() => setIsHovering(true)}
            onMouseLeave={() => setIsHovering(false)}
          >
            {/* Mouse-reactive glow overlay */}
            <div
              className="absolute inset-0 opacity-0 transition-opacity duration-300 pointer-events-none rounded-[2rem]"
              style={{
                opacity: isHovering ? 1 : 0,
                background: `radial-gradient(600px circle at ${mousePosition.x}px ${mousePosition.y}px, rgba(6, 182, 212, 0.15), transparent 40%)`
              }}
            />

            {/* Mouse-reactive border glow */}
            <div
              className="absolute inset-0 rounded-[2rem] pointer-events-none transition-opacity duration-300"
              style={{
                opacity: isHovering ? 1 : 0,
                background: `radial-gradient(400px circle at ${mousePosition.x}px ${mousePosition.y}px, rgba(6, 182, 212, 0.4), transparent 40%)`,
                mask: 'linear-gradient(#fff 0 0) content-box, linear-gradient(#fff 0 0)',
                maskComposite: 'xor',
                WebkitMaskComposite: 'xor',
                padding: '1px'
              }}
            />

            {/* Glass background */}
            <div className="absolute inset-0 bg-gradient-to-br from-slate-800/80 via-slate-900/90 to-slate-900/95 backdrop-blur-2xl" />
            <div className="absolute inset-0 bg-gradient-to-b from-white/[0.05] to-transparent" />

            {/* Static border */}
            <div className="absolute inset-0 rounded-[2rem] border border-white/[0.08]" />
            <div className="absolute top-0 left-10 right-10 h-px bg-gradient-to-r from-transparent via-cyan-400/30 to-transparent" />

            {/* Content */}
            <div className="relative px-10 py-12">
              {/* Header */}
              <div className="text-center mb-10">
                <h2 className="text-3xl font-bold text-white tracking-tight">Welcome Back</h2>
              </div>

              {/* Error Message */}
              {error && (
                <div className="mb-8 p-4 rounded-xl bg-red-500/15 border border-red-400/25 animate-shake">
                  <p className="text-red-300 text-sm text-center">{error}</p>
                </div>
              )}

              {/* Form */}
              <form onSubmit={handleSubmit} className="space-y-5">
                {/* Username Input */}
                <div className="relative">
                  <div className="absolute left-4 top-1/2 -translate-y-1/2 text-cyan-400/70">
                    <UserRound className="w-5 h-5" />
                  </div>
                  <input
                    id="username"
                    type="text"
                    value={username}
                    onChange={(e): void => setUsername(e.target.value)}
                    className="w-full pl-12 pr-5 py-4 rounded-xl 
                      bg-slate-800/60 backdrop-blur-sm
                      border border-white/[0.08] 
                      text-white text-base placeholder-gray-500
                      focus:outline-none focus:border-cyan-500/50 focus:bg-slate-800/80
                      transition-all duration-200"
                    placeholder="Username"
                    required
                    disabled={isLoading}
                  />
                </div>

                {/* Password Input */}
                <div className="relative">
                  <div className="absolute left-4 top-1/2 -translate-y-1/2 text-cyan-400/70">
                    <KeyRound className="w-5 h-5" />
                  </div>
                  <input
                    id="password"
                    type="password"
                    value={password}
                    onChange={(e): void => setPassword(e.target.value)}
                    className="w-full pl-12 pr-5 py-4 rounded-xl 
                      bg-slate-800/60 backdrop-blur-sm
                      border border-white/[0.08] 
                      text-white text-base placeholder-gray-500
                      focus:outline-none focus:border-cyan-500/50 focus:bg-slate-800/80
                      transition-all duration-200"
                    placeholder="Password"
                    required
                    disabled={isLoading}
                  />
                </div>

                {/* Remember me */}
                <div className="flex items-center gap-3 pt-2 justify-between">
                  <div className="flex items-center gap-3">
                    <button
                      type="button"
                      onClick={() => setRememberMe(!rememberMe)}
                      className={`w-5 h-5 rounded flex items-center justify-center transition-all duration-200 flex-shrink-0 ${
                        rememberMe
                          ? 'bg-cyan-500 border-transparent'
                          : 'bg-slate-800/60 border border-white/20 hover:border-white/40'
                      }`}
                    >
                      {rememberMe && (
                        <svg
                          className="w-3 h-3 text-white"
                          fill="none"
                          stroke="currentColor"
                          viewBox="0 0 24 24"
                        >
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={3}
                            d="M5 13l4 4L19 7"
                          />
                        </svg>
                      )}
                    </button>
                    <span className="text-gray-300 text-sm">Remember me</span>
                  </div>
                  <button
                    type="button"
                    className="text-gray-400 text-sm hover:text-cyan-400 transition-colors"
                  >
                    Forgot Password?
                  </button>
                </div>

                {/* Submit Button */}
                <button
                  type="submit"
                  disabled={isLoading}
                  className={`w-full py-4 px-6 mt-4 rounded-xl font-semibold text-white text-sm uppercase tracking-widest
                    transition-all duration-300 transform ${
                      isLoading
                        ? 'bg-cyan-600/50 cursor-not-allowed'
                        : 'bg-gradient-to-r from-cyan-500 to-cyan-600 hover:from-cyan-400 hover:to-cyan-500 hover:shadow-lg hover:shadow-cyan-500/30 active:scale-[0.98]'
                    }`}
                >
                  {isLoading ? (
                    <span className="flex items-center justify-center gap-3">
                      <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                      Signing In...
                    </span>
                  ) : (
                    'Login'
                  )}
                </button>
              </form>

              {/* Register Link */}
              <div className="text-center mt-8 pt-6 border-t border-white/[0.06]">
                <p className="text-gray-400 text-sm">
                  New to Whisper?{' '}
                  <Link
                    to="/register"
                    className="text-cyan-400 font-semibold hover:text-cyan-300 transition-colors"
                  >
                    Create an Account
                  </Link>
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
