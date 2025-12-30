using System;

namespace Identity.Domain.Exceptions
{
    /// <summary>
    /// Custom exception for identity domain validation errors
    /// </summary>
    public class IdentityDomainException : Exception
    {
        public IdentityDomainException()
        {
        }

        public IdentityDomainException(string message) : base(message)
        {
        }

        public IdentityDomainException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public IdentityDomainException(string message, string paramName) : base(message)
        {
            ParamName = paramName;
        }

        public string? ParamName { get; }
    }
}
