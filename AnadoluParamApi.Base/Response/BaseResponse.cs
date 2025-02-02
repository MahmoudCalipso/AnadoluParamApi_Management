﻿namespace AnadoluParamApi.Base.Response
{
    public class BaseResponse<T> //Answers to be returned at endpoints
    {
        public bool Success { get; private set; }
        public List<string> Message { get; private set; }
        public T Response { get; private set; }

        public BaseResponse(bool isSuccess)
        {
            Response = default;
            Success = isSuccess;
            Message = isSuccess ? new List<string>() { "Success" } : new List<string>() { "Fault" };
        }

        public BaseResponse(T resource)
        {
            Success = true;
            Message = new List<string>() { "Success" };
            Response = resource;
        }

        public BaseResponse(string message)
        {
            Success = false;
            Response = default;

            if (!string.IsNullOrWhiteSpace(message))
            {
                Message = new List<string>() { message };
            }
        }

        public BaseResponse(List<string> messages)
        {
            this.Success = false;
            this.Response = default;
            this.Message = messages ?? new List<string>() { "Fault" };
        }
    }
}
