using System;
namespace ServerCore
{
    public enum StatusCode
    {
        Success = 200,

        NotConnected = 301,

        InvalidRequest = 302,
        UnknownError = 501,

    }
}

