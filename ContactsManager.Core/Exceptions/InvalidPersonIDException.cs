﻿namespace Exceptions
{
    public class InvalidPersonIDException : Exception
    {
        public InvalidPersonIDException() : base()
        {
            
        }
        public InvalidPersonIDException(string? message) : base(message)
        {
            
        }
        public InvalidPersonIDException(string? message, Exception exception) : base(message,exception)
        {

        }
    }
}
