using Core.CustomException;
using Core.Models.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemplateProject.Infrastructure
{
    public class LoginLogFilter: IActionFilter
    {
        public LoginLogFilter() { }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            //Alınan Model Kaydedilecek
            string action = (string)context.RouteData.Values["action"];
            string controller = (string)context.RouteData.Values["controller"];

            var entity = context.HttpContext.Items[controller + "_" + action];
            var result = context.Result;

            string userID = ((LoginResultModel)((ObjectResult)result).Value).UserId.ToString();
            string testLog = userID + "-" + ((LoginModel)entity).UserName + "-" + ((LoginModel)entity).Password;
            string model = Newtonsoft.Json.JsonConvert.SerializeObject((((ObjectResult)result).Value));
            return;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                //Log işlemleri
                string action = (string)context.RouteData.Values["action"];
                string controller = (string)context.RouteData.Values["controller"];
                //Loglanacak Model Alınır
                foreach (ControllerParameterDescriptor param in context.ActionDescriptor.Parameters)
                {
                    if (param.ParameterInfo.CustomAttributes.Any(
                        attr => attr.AttributeType == typeof(FromBodyAttribute))
                    )
                    {
                        var entity = context.ActionArguments[param.Name];
                        //Burada BeHalhOfUserID'de kullanılabilir.
                        context.HttpContext.Items[controller + "_" + action] = entity;
                    }
                }
                //---------------------------------                
            }
            catch (InvalidTokenException ex)
            {
                //Forbidden 430 Result. Yetkiniz Yoktur..
                context.Result = new ObjectResult(context.ModelState)
                {
                    //Value = "Invalid Token Execption." + ex.Message,
                    Value = "Geçerli Bir Token Girilmemiştir",
                    StatusCode = 430
                };
                return;
            }
        }
    }
}
