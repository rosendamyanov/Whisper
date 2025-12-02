namespace Whisper.Common.Response
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }

        /// <summary>
        /// Creates a success response.
        /// </summary>
        public static ApiResponse<T> Success(T data, string message = "Operation Succesful.")
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message,
                ErrorCode = null
            };
        }
        /// <summary>
        /// Creates a failure response.
        /// </summary>
        public static ApiResponse<T> Failure(string message, string errorCode = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Data = default,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }
}
