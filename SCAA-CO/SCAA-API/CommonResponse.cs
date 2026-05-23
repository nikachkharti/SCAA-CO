namespace SCAA_API
{
    public sealed class CommonResponse
    {
        public string Message { get; set; }
        public object Result { get; set; }
        public bool IsSuccess { get; set; }
        public int HttpStatusCode { get; set; }

        public CommonResponse(object result, string message, bool isSuccess, int httpStatusCode)
        {
            Message = message;
            Result = result;
            IsSuccess = isSuccess;
            HttpStatusCode = httpStatusCode;
        }

        public CommonResponse()
        {
        }
    }


    public static class CommonResponseMessage
    {
        public static string SuccessMessage { get; } = "Request completed successfully";
    };
}
