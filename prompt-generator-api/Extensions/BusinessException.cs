using System;
using System.Collections.Generic;

namespace tennis_wave_api.Extensions;

public class BusinessException : Exception
{
    public string ErrorCode { get; }

    public BusinessException(string message, string errorCode = "BUSINESS_ERROR") 
        : base(message)
    {
        ErrorCode = errorCode;
    }
}

public class ValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(Dictionary<string, string[]> errors) 
        : base("Valid Fail")
    {
        Errors = errors;
    }
}