using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Extensions
{
    public static class SessionExtensions
    {
        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetObject(key, value);
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetObject<T>(key);
            return value == null ? default(T) : value;
        }
    }
}
