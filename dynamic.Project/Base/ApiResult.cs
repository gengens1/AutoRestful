namespace dynamic.Project.Base
{
    public class ApiResult
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public Object Data { get; set; }
        public bool Result { get; set; }

        public static ApiResult Success(string message = "Success", object data = null,int code = 200)
        {
            return new ApiResult
            {
                Code = 200,
                Message = message,
                Data = data,
                Result = true
            };
        }

        public static ApiResult Fail(string message = "Fail", object data = null, int code = 200)
        {
            return new ApiResult
            {
                Code = 400,
                Message = message,
                Data = data,
                Result = false
            };
        }

    }
}
