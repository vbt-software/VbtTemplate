using Core.Caching;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemplateProject.Infrastructure
{
    public class LoginFilter : IActionFilter
    {
        public LoginFilter(IRedisCacheService redisCacheService)
        {

        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
           
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {

        }
    }   
}
