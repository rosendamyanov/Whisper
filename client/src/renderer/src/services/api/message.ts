import axiosInstance from './axios'
import { ApiResponse } from '../../types/api/response/api/ApiResponse'
import { MessageResponse } from '../../types/api/response/chat/MessageResponse'

export const messageApi = {
  getChatMessages: async (chatId: string) => {
    const response = await axiosInstance.get<ApiResponse<MessageResponse[]>>(
      `/message/chat/${chatId}`
    )
    return response.data
  },

  sendMessage: async (chatId: string, content: string) => {
    const formData = new FormData()
    formData.append('chatId', chatId)
    formData.append('content', content)

    const response = await axiosInstance.post<ApiResponse<MessageResponse>>('/message', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    return response.data
  }
}
