namespace PRN232.Lab1.Services.Models
{
    public class ServiceResult<T>
    {
        public bool Success { get; private set; }
        public T? Data { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public ServiceErrorType ErrorType { get; private set; } = ServiceErrorType.None;

        public static ServiceResult<T> Ok(T data, string message = "Request processed successfully")
        {
            return new ServiceResult<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ServiceResult<T> Fail(string message, ServiceErrorType errorType)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Message = message,
                ErrorType = errorType
            };
        }
    }
}
