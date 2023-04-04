using System;

namespace SmartFarmer.Exceptions
{
    public class TaskInitializationException : Exception
    {
        public TaskInitializationException(string message = null)
            : base(message)
        {
            
        }
    }

}