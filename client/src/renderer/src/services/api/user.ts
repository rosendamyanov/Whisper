// src/renderer/src/api/users.ts
import axiosInstance from './axios'; // Your configured instance with interceptors
import { ApiResponse } from '../../types/api/response/api/ApiResponse';
import { UserSessionResponse } from '../../types/api/response/user/UserSessionResponse'; 
import { UserLoginRequest } from '../../types/api/request/auth/UserLoginRequest';
import { UserRegisterRequest } from '../../types/api/request/auth/UserRegisterRequest';

export const usersApi = {
  login: async (credentials: UserLoginRequest) => {
    // The endpoint matches your UserController
    const response = await axiosInstance.post<ApiResponse<UserSessionResponse>>(
      '/user/login', 
      credentials
    );
    return response.data;
  },

  register: async (data: UserRegisterRequest) => {
    const response = await axiosInstance.post<ApiResponse<UserSessionResponse>>(
      '/user/register', 
      data
    );
    return response.data;
  }
};


