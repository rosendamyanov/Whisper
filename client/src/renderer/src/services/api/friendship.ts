import axiosInstance from './axios';
import { ApiResponse } from '../../types/api/response/api/ApiResponse';
import { FriendRequest } from '@renderer/types/api/request/friendship/FriendRequest';
import { User } from '../../types/api/response/user/User'; 

export const friendshipApi = {
    sendRequest: async (friendId: string) => {
        const response = await axiosInstance.post<ApiResponse<string>>(
            `/friends/request/${friendId}`
        );
        return response.data; 
    },

    getFriends: async () => {
        const response = await axiosInstance.get<ApiResponse<User[]>>(
            '/friends' 
        );
        return response.data; 
    },

    getPendingRequests: async () => {
        const response = await axiosInstance.get<ApiResponse<FriendRequest[]>>(
            '/friends/requests/pending'
        );
        return response.data;
    },

    acceptRequest: async (friendshipId: string) => {
        const response = await axiosInstance.post<ApiResponse<string>>(
            `/friends/accept/${friendshipId}`
        );
        return response.data;
    },

    declineRequest: async (friendshipId: string) => {
        const response = await axiosInstance.post<ApiResponse<string>>(
            `/friends/decline/${friendshipId}`
        );
        return response.data;
    }
}