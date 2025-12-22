import { User } from "../user/User"
import { MessageResponse } from "./MessageResponse"

export interface ChatResponse {
    id: string,
    isGroup: boolean,
    name: string | null,
    participants: User[],
    createdAt: string,
    lastMessage: MessageResponse | null;
}