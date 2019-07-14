﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace OnUtils.Application.Exceptions
{
    /// <summary>
    /// Представляет ошибки, возникающие во время выполнения приложения, с кодом ошибки.
    /// </summary>
    public class ErrorCodeException : HandledException
    {
        public ErrorCodeException(string message) : this(0, message)
        {
        }

        public ErrorCodeException(HttpStatusCode code, string message) : this(code, message, null)
        {
        }

        public ErrorCodeException(HttpStatusCode code, string message, Exception innerException) : base(message, innerException)
        {
            this.Code = code;
        }

        /// <summary>
        /// Код ошибки.
        /// </summary>
        public HttpStatusCode Code
        {
            get;
            private set;
        }
    }
}