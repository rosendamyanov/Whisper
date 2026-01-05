import { FriendResponse } from './FriendResponse'

export interface FriendshipResponse {
  id: string
  friend: FriendResponse
  isAccepted: boolean
  createdAt: string
  acceptedAt: string
}
