import axiosInstance from './axios';
import { ApiResponse } from '../../types/api/response/api/ApiResponse';
import { MessageResponse } from '../../types/api/response/chat/MessageResponse';

export const messageApi = {
    // Matches: [HttpGet("chat/{chatId}")]
    getChatMessages: async (chatId: string) => {
        const response = await axiosInstance.get<ApiResponse<MessageResponse[]>>(
            `/message/chat/${chatId}`
        );
        return response.data;
    },

    // Matches: [HttpPost] with [FromForm]
    sendMessage: async (chatId: string, content: string) => {
        const formData = new FormData();
        formData.append('chatId', chatId);
        formData.append('content', content);
        
        // If you handle files later:
        // formData.append('file', file);

        const response = await axiosInstance.post<ApiResponse<MessageResponse>>(
            '/message',
            formData,
            {
                headers: { 'Content-Type': 'multipart/form-data' }
            }
        );
        return response.data;
    }
};