import axiosInstance from './axios'
import { ApiResponse } from '../../types/api/response/api/ApiResponse'
import { UserSessionResponse } from '../../types/api/response/user/UserSessionResponse'
import { UserLoginRequest } from '../../types/api/request/auth/UserLoginRequest'
import { UserRegisterRequest } from '../../types/api/request/auth/UserRegisterRequest'
import { User } from '../../types/api/response/user/User'

export const usersApi = {
  login: async (credentials: UserLoginRequest) => {
    const response = await axiosInstance.post<ApiResponse<UserSessionResponse>>(
      '/user/login',
      credentials
    )
    return response.data
  },

  register: async (data: UserRegisterRequest) => {
    const response = await axiosInstance.post<ApiResponse<UserSessionResponse>>(
      '/user/register',
      data
    )
    return response.data
  },

  search: async (query: string) => {
    const response = await axiosInstance.get<ApiResponse<User[]>>('user/search', {
      params: { query }
    })
    return response.data
  }
}
