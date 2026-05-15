using System.Collections.Generic;

namespace Utils.Responses
{
    public interface IResult
    {
        bool IsSuccess { get; }

        string Message { get; }
        
        IEnumerable<string> Messages { get; }

        IEnumerable<string> Errors { get; }

        int StatusCode { get; }
    }

    public interface IResult<out T> : IResult
    {
        T Data { get; }
    }
}