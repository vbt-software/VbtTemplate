using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemplateProject.Infrastructure
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LogAttribute : Attribute
    {
    }
}
