using System.Collections.Generic;

namespace Utils.Responses
{
public class Result : IResult
    {
        public bool IsSuccess { get; init; }

        public string Message { get; init; } = string.Empty;
        public IEnumerable<string> Messages { get; init; } = new List<string>();
        public IEnumerable<string> Errors { get; init; } = [];

        public int StatusCode { get; init; }

        protected Result(bool isSuccess, int statusCode, string? message = null, IEnumerable<string>? errors = null)
        {
            IsSuccess = isSuccess;
            Message = message ?? string.Empty;
            Errors = errors ?? [];
            StatusCode = statusCode;
        }

        public static Result Success(int statusCode = 200, string? message = null) => new(true, statusCode, message);

     
        public static Result Failure(IEnumerable<string> errors, int statusCode = 400) => new(false, statusCode, null, errors);

        public static Result Failure(string message, int statusCode = 400) => new(false, statusCode, message);
    }


    public class Result<T> : Result, IResult<T>
    {
        public T Data { get; init; }

        private Result(T data, bool isSuccess, int statusCode, string? message = null, IEnumerable<string>? errors = null) : base(isSuccess, statusCode, message, errors)
        {
            Data = data;
        }

        public static Result<T> Success(T data, int statusCode = 200, string? message = null) => new(data, true, statusCode, message);

        public static Result<T> Failure(IEnumerable<string> errors, int statusCode = 400, string? message = null) => new(default, false, statusCode, message, errors);
    }
}