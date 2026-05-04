namespace Backend.Models
{
    public class ResponseBase
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public object? Data { get; set; }

        public ResponseBase(bool isSuccess, string message, object? data = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
        }

        public static ResponseBase Success(string message, object? data = null)
        {
            return new ResponseBase(true, message, data);
        }

        public static ResponseBase Failure(string message, object? data = null)
        {
            return new ResponseBase(false, message, data);
        }

    }

}

