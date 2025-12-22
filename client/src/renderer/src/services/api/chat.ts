import axiosInstance from "./axios";
import { ApiResponse } from "@renderer/types/api/response/api/ApiResponse";
import { ChatResponse } from "@renderer/types/api/response/chat/ChatResponse";

export const chatApi = {
    getMyChats: async () => {
        const response = await axiosInstance.get<ApiResponse<ChatResponse[]>>(
            'chat/my-chats'
        );
        return response.data;
    }
}