using System;
namespace TemplateProject.Infrastructure
{
    [AttributeUsage(AttributeTargets.Method)]
    public class IgnoreAttribute : Attribute
    {
    }
}
