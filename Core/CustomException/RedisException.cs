using System;
using System.Collections.Generic;
using System.Text;

namespace Core.CustomException
{
    public class RedisNotAvailableException : Exception
    {
        public string _errorCode = "432";
        public override string Message
        {
            get
            {
                return "Redis is not available Exeception";
            }
        }

        public string ErrorCode
        {
            get
            {
                return _errorCode;
            }
        }
    }
}
