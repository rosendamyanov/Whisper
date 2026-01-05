export interface ApiResponse<T> {
  isSuccess: boolean
  data: T
  message: string
  errorCode: string | null
}
