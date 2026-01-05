import { AuthResponse } from '../auth/AuthResponse'
import { User } from './User'

export interface UserSessionResponse {
  authResponse: AuthResponse
  user: User
}
