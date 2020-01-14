using System;

namespace DB.PartialEntites
{
    [AttributeUsage(AttributeTargets.All)]
    public class SetCurrentDate : System.Attribute
    {
        public SetCurrentDate()
        {
        }
    }
}
