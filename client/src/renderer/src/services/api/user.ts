// src/renderer/src/api/users.ts
import axiosInstance from './axios'; // Your configured instance with interceptors
import { ApiResponse } from '../../types/response/api/ApiResponse';
import { UserSessionResponse } from '../../types/response/user/UserSessionResponse'; 
import { UserLoginRequest } from '../../types/request/auth/UserLoginRequest';
import { UserRegisterRequest } from '../../types/request/auth/UserRegisterRequest';

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


