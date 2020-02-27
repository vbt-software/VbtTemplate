using System;
using System.Collections.Generic;
using System.Text;

namespace DB.PartialEntites
{
    [AttributeUsage(AttributeTargets.All)]
    public class CryptoData : System.Attribute
    {
        public CryptoData()
        {
        }
    }

}
