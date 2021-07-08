using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Utilme
{
    public class Result<T>
    {
        public T Value { get; }
        public bool IsOk { get; } = true;
        public ResultStatus Status { get; } = ResultStatus.Ok;
        public string ErrorMessage { get; }

        private Result(T value) => Value = value;

        private Result(ResultStatus status)
        {
            Status = status;
            if(status != ResultStatus.Ok)
            {
                IsOk = false;
            }
        }

        private Result(string message)
        {
            Status = ResultStatus.Error;
            IsOk = false;
            ErrorMessage = message;
        }

        public static Result<T> Ok(T value) => new Result<T>(value);
        public static Result<T> Error(string message) => new Result<T>(message);
        public static Result<T> NotFound => new Result<T>(ResultStatus.NotFound);
        public static Result<T> Timeout => new Result<T>(ResultStatus.Timeout);
        public static Result<T> Cancelled => new Result<T>(ResultStatus.Cancelled);
        public static Result<T> NotSupported() => new Result<T>(ResultStatus.NotSupported);
        public static Result<T> InvalidData() => new Result<T>(ResultStatus.InvalidData);                               

    }
}
