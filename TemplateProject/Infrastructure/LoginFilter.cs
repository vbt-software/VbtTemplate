using Core;
using Core.Caching;
using Core.CoreContext;
using Core.Extensions;
using Core.Models.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Services.SecurityService;
using Services.Users;
using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemplateProject.Infrastructure
{
    public class LoginFilter : IActionFilter
    {
        private readonly IRedisCacheService _redisCacheService;
        private readonly IEncryptionService _encryptionService;
        private readonly IUserService _userService;
        private readonly ICoreContext _coreContext;
        private readonly IWorkContext _workContext;

        public LoginFilter(IRedisCacheService redisCacheService, IEncryptionService encryptionService,
            IUserService userService, ICoreContext coreContext, IWorkContext workContext)
        {
            _redisCacheService = redisCacheService;
            _encryptionService = encryptionService;
            _coreContext = coreContext;
            _userService = userService;
            _workContext = workContext;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            //Kontrol edilmesi gereken bir Action mı ?
            if (HasIgnoreAttribute(context)) return;

            bool.TryParse(context.HttpContext.Request.Headers["IsMobile"].FirstOrDefault(), out var isMobile);
            int.TryParse(context.HttpContext.Request.Headers["UserId"].FirstOrDefault(), out var userId);
            var unqDeviceId = context.HttpContext.Request.Headers["UnqDeviceId"].FirstOrDefault();
            if (userId == 0)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            //Genel Kullanılacak değişkenler burada atanır.
            _workContext.CurrentUserId = userId;
            _workContext.IsMobile = isMobile;
            //--------------------------------------
            string authHeader = context.HttpContext.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Bearer"))
            {
                //Extract credentials
                var token = authHeader.Substring("Bearer ".Length).TrimStart();
                var decryptToken = _encryptionService.DecryptText(token);
                if (string.IsNullOrEmpty(decryptToken))// token yoksa UnauthorizedResult dönüyoruz
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                //İlgili UserID'ye ait Token Redis'den alınır.
                var cacheRedistoken = _redisCacheService.Get<string>(_redisCacheService.GetTokenKey(userId, isMobile, false, unqDeviceId));

                if (string.IsNullOrEmpty(cacheRedistoken)) // Redis'de Token Key yok ise  
                {
                    //Refresh Token'a bakılır
                    if (context.HttpContext.Request.Headers["RefreshToken"].FirstOrDefault() != null) // client refresh token göndermiş.
                    {
                        var clientRefreshToken = context.HttpContext.Request.Headers["RefreshToken"].FirstOrDefault();
                        var redisRefreshToken = _redisCacheService.Get<string>(_redisCacheService.GetTokenKey(userId, isMobile, true, unqDeviceId));

                        if (string.IsNullOrEmpty(redisRefreshToken))//rediste refresh token yok 
                        {
                            context.Result = new UnauthorizedResult();
                            return;
                        }
                        var decClientRefreshToken = _encryptionService.DecryptText(clientRefreshToken);
                        if (decClientRefreshToken == redisRefreshToken)//Refresh Token doğru. Yeni token ve refresh token üretip dönelim.
                        {
                            UserModel user = _userService.GetById(userId).Entity;
                            var (encToken, decToken) = _encryptionService.GenerateToken(user.Email);
                            //Oluşturulsn Token Redis'e atılır.
                            var createTime = DateTime.Now;

                            //Token Oluşturulur. Mobilde ve Web'de 1 saattir. appsettings.json'a bakınız.
                            DateTime tokenExpireTime = createTime.AddMinutes(_coreContext.TokenExpireTime);
                            _redisCacheService.Set(_redisCacheService.GetTokenKey(userId, isMobile, false, unqDeviceId), decToken, tokenExpireTime);

                            //Geri dönülecek Encrypt Token ve Yaratılma zamanı Client'ın Header'ına atanır
                            context.HttpContext.Items["token"] = encToken;
                            context.HttpContext.Items["createdTokenTime"] = createTime.GetTotalMilliSeconds();

                            //RefreshToken Oluşturulur.
                            //Refresh Token Mobilde 1 Yıl Web'de 1.5 saattir. appsettings.json'a bakınız.
                            var refreshToken = GenerateRefreshToken(user, context, unqDeviceId, isMobile);
                            if (!string.IsNullOrWhiteSpace(refreshToken))
                            {
                                //Oluşturulan RefreshToken Client'a dönülür.
                                context.HttpContext.Items["refreshToken"] = refreshToken;
                            }
                        }
                        else
                        {
                            context.Result = new UnauthorizedResult();
                            return;
                        }
                    }
                    else
                    {
                        context.Result = new UnauthorizedResult();
                        return;
                    }
                }
                else if (cacheRedistoken.Trim() != decryptToken.Trim()) //Redis'de Token Var ama tokenlar eşit değil , geçerli bir oturum isteği değil. 
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                //Redis'in süresine bakılacak
                var tokenSession = decryptToken.Split('ß')[2];
                var sessionCreateTime = DateTime.Parse(tokenSession);
                var remainingTime = DateTime.Now - sessionCreateTime;

                //tokenlar eşit , 45 ile 60'ıncı dakikalar arasındaysa token ve refresh token'ı yenileyip dönelim
                if (remainingTime.TotalMinutes >= _coreContext.TokenExpireTime && remainingTime.TotalMinutes <= _coreContext.TokenExpireTime - 15)
                {
                    UserModel user = _userService.GetById(userId).Entity;
                    var (encToken, decToken) = _encryptionService.GenerateToken(user.Email);
                    //Oluşturulsn Token Redis'e atılır.

                    var createTime = DateTime.Now;
                    DateTime tokenExpireTime = createTime.AddMinutes(_coreContext.TokenExpireTime);
                    _redisCacheService.Set(_redisCacheService.GetTokenKey(userId, isMobile, false, unqDeviceId), decToken, tokenExpireTime);

                    //Geri dönülecek Encrypt Token ve Yaratılma zamanı Client'ın Header'ına atanır
                    context.HttpContext.Items["token"] = encToken;
                    context.HttpContext.Items["createdTokenTime"] = createTime.GetTotalMilliSeconds();

                    //RefreshToken Oluşturulur.
                    //Refresh Token Mobilde 1 Yıl Web'de 1.5 saattir. appsettings.json'a bakınız.
                    var refreshToken = GenerateRefreshToken(user, context, unqDeviceId, isMobile);
                    if (!string.IsNullOrWhiteSpace(refreshToken))
                    {
                        //Oluşturulan RefreshToken Client'a dönülür.
                        context.HttpContext.Items["refreshToken"] = refreshToken;
                    }
                }
            }
            else
            {
                context.Result = new UnauthorizedResult();
            }

            if (HasLogAttribute(context))
            {
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
                        context.HttpContext.Items[userId + "_" + controller + "_" + action] = entity;
                    }
                }
                //---------------------------------
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (HasLogAttribute(context))
            {
                //Alınan Model Kaydedilecek
                string action = (string)context.RouteData.Values["action"];
                string controller = (string)context.RouteData.Values["controller"];
                int.TryParse(context.HttpContext.Request.Headers["UserId"].FirstOrDefault(), out var userId);
                if (userId != 0)
                {
                    var entity = context.HttpContext.Items[userId + "_" + controller + "_" + action];
                    return;
                }
            }
        }

        public bool HasIgnoreAttribute(ActionExecutingContext context)
        {
            return ((ControllerActionDescriptor)context.ActionDescriptor).MethodInfo.CustomAttributes.Any(filterDescriptors => filterDescriptors.AttributeType == typeof(IgnoreAttribute));
        }

        public bool HasLogAttribute(FilterContext context)
        {
            return ((ControllerActionDescriptor)context.ActionDescriptor).MethodInfo.CustomAttributes.Any(filterDescriptors => filterDescriptors.AttributeType == typeof(LogAttribute));
        }

        public string GenerateRefreshToken(UserModel user, ActionExecutingContext context, string unqDeviceId, bool isMobile)
        {
            var createTime = DateTime.Now;
            DateTime tokenExpireTime = createTime.AddMinutes(_coreContext.RefreshTokenExpireTime);
            if (isMobile)
            {
                tokenExpireTime = createTime.AddMinutes(_coreContext.MobileRefreshTokenExpireTime);
            }
            var (encToken, decToken) = _encryptionService.GenerateToken(user.Email);
            _redisCacheService.Set(_redisCacheService.GetTokenKey(user.Id, isMobile, true, unqDeviceId), decToken, tokenExpireTime);
            return encToken;
        }
    }
}
