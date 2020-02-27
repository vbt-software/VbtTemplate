using Core;
using Core.Caching;
using Core.CoreContext;
using Core.CustomException;
using Core.Extensions;
using Core.Models.Roles;
using Core.Models.Users;
using Core.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Services.Roles;
using Services.Users;
using System;
using System.Linq;
using TemplateProject.Controllers;

namespace TemplateProject.Infrastructure
{
    public class LoginFilter : IActionFilter
    {
        private readonly IRedisCacheService _redisCacheService;
        private readonly IEncryption _encryption;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly ICoreContext _coreContext;
        private readonly IWorkContext _workContext;

        private readonly IStringLocalizer<VbtController> _localizer;

        private static readonly object lockObject = new object();

        public LoginFilter(IRedisCacheService redisCacheService, IEncryption encryption,
            IUserService userService, ICoreContext coreContext, IWorkContext workContext, IRoleService roleService, IStringLocalizer<VbtController> localizer)
        {
            _redisCacheService = redisCacheService;
            _encryption = encryption;
            _coreContext = coreContext;
            _userService = userService;
            _workContext = workContext;
            _roleService = roleService;
            _localizer = localizer;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                //Kontrol edilmesi gereken bir Action mı ? Mesela Login için bu işlem yapılmaz!
                if (HasIgnoreAttribute(context)) return;

                //BeHalfOfToken and BeHalfOfUserId //Birinin Adına mı Girmiş!
                var beHalfOfUserId = context.HttpContext.Request.Headers["BeHalfOfUserId"].FirstOrDefault();
                var beHalfOfToken = context.HttpContext.Request.Headers["BeHalfOfToken"].FirstOrDefault();
                var beHalfOfPassword = context.HttpContext.Request.Headers["BeHalfOfPassword"].FirstOrDefault();

                //beHalfOfUserId parametersi ZORUNLU olmadığı için, null ise "beHalfOfPassword" değerinden alınır.
                if (beHalfOfPassword != null && (beHalfOfUserId == null || int.Parse(beHalfOfUserId) == 0) && !string.IsNullOrEmpty(beHalfOfPassword))
                {
                    var decryptBeHalfOfPassword = _encryption.DecryptFromClientData(beHalfOfPassword);
                    if (decryptBeHalfOfPassword.Split('@').Length > 1)
                    {
                        beHalfOfUserId = decryptBeHalfOfPassword.Split('@')[1];
                    }
                }

                bool.TryParse(context.HttpContext.Request.Headers["IsMobile"].FirstOrDefault(), out var isMobile);
                int.TryParse(context.HttpContext.Request.Headers["UserId"].FirstOrDefault(), out var userId);
                //Mobile için
                var unqDeviceId = context.HttpContext.Request.Headers["UnqDeviceId"].FirstOrDefault();
                //Tüm platformlar için gerekli kontrollerin yapılabilmesi için UserID şarttır.              

                //if (userId == 0) // Başkası Adına da Girmiş Olabilir (BeHalfOf)
                if (userId == 0 && int.Parse(beHalfOfUserId) == 0)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }
                //Genel Kullanılacak değişkenler burada atanır.
                _workContext.CurrentUserId = userId;
                _workContext.IsMobile = isMobile;
                //--------------------------------------

                //BeHalfOfUserId Global
                if (beHalfOfUserId != null && beHalfOfUserId != "" && int.Parse(beHalfOfUserId) > 0)
                {
                    _workContext.CurrentBeHalfOfUserId = int.Parse(beHalfOfUserId);
                }

                //Set UserIsAdmin
                UserModel user = _userService.GetById(userId).Entity;
                if (user != null)
                {
                    _workContext.IsAdmin = user.IsAdmin;
                }
                //---------------------

                //BeHalfOfToken Check! Birisinin Adına Girmiş İse Onun Kuralları Geçerli Olur.
                if (beHalfOfToken != null && beHalfOfToken != "")
                {
                    var decryptBeHalfOfToken = _encryption.DecryptText(beHalfOfToken);
                    var decryptBeHalfOfPassword = _encryption.DecryptFromClientData(beHalfOfPassword);
                    var cacheRedisbeHalfOfToken = _redisCacheService.Get<string>(_redisCacheService.GetTokenKeyForBeHalfOf(int.Parse(beHalfOfUserId), decryptBeHalfOfPassword));
                    //Başkasının yerine girdi ve bilgiler doğru değil ise devam edilmez. Geriya hata dönülür.
                    if (string.IsNullOrEmpty(cacheRedisbeHalfOfToken) || (!string.IsNullOrEmpty(cacheRedisbeHalfOfToken) && cacheRedisbeHalfOfToken.Trim() != decryptBeHalfOfToken.Trim()))
                    {
                        context.Result = new ObjectResult(context.ModelState)
                        {
                            Value = _localizer["BeHalfofTokenException"],
                            StatusCode = 431
                        };
                        return;
                    }
                }
                else
                {
                    //BeHalfOf Değil ise Token Kontrolü yapılır.
                    string authHeader = context.HttpContext.Request.Headers["Authorization"];
                    //Not: Bu durum sadece Web ortamı için geçerlidir. Mobilden her zaman Token gelmektedir.
                    if (authHeader != null && authHeader.StartsWith("Bearer"))
                    {
                        //Extract credentials
                        var token = authHeader.Substring("Bearer ".Length).TrimStart();
                        var decryptToken = _encryption.DecryptText(token);
                        //Not: Bu durum sadece Web ortamı için geçerlidir. Mobilden her zaman Token gelmektedir. Hiçbir zaman timeout'a uğramaz. Tek fark 45 dakikadan büyük ise RefreshToken'da gönderilir. 
                        if (string.IsNullOrEmpty(decryptToken))// token yoksa UnauthorizedResult dönüyoruz. Bu sadece Web ortamı için geçerlidir. Mobilede her zaman Token dönülür. Gelmemiş ise ona da UnauthorizedResult dönülür.
                        {
                            context.Result = new UnauthorizedResult();
                            return;
                        }

                        //İlgili UserID'ye ait Token Redis'den alınır.
                        var cacheRedistoken = _redisCacheService.Get<string>(_redisCacheService.GetTokenKey(userId, isMobile, false, unqDeviceId));

                        if (string.IsNullOrEmpty(cacheRedistoken) && isMobile) // Redis'de Token Key yok ise , bu durum SADECE MOBILE'DE BAKILMALIDIR.
                        {
                            //Refresh Token kontrolü yapılır.
                            CreateTokensByCheckRefreshToken(context, true); //true'nun amacı  context.Result = new UnauthorizedResult() dönüşünün yapılmasının istenmesidir.
                            #region CreateTokensByCheckRefreshToken Methodu Altına Taşındı.
                            //if (context.HttpContext.Request.Headers["RefreshToken"].FirstOrDefault() != null) // client refresh token göndermiş.
                            //{
                            //    var clientRefreshToken = context.HttpContext.Request.Headers["RefreshToken"].FirstOrDefault();
                            //    var redisRefreshToken = _redisCacheService.Get<string>(_redisCacheService.GetTokenKey(userId, isMobile, true, unqDeviceId));

                            //    if (string.IsNullOrEmpty(redisRefreshToken))//rediste refresh token yok 
                            //    {
                            //        context.Result = new UnauthorizedResult();
                            //        return;
                            //    }
                            //    var decClientRefreshToken = _encryptionService.DecryptText(clientRefreshToken);
                            //    if (decClientRefreshToken == redisRefreshToken)//Refresh Token doğru. Yeni token ve refresh token üretip dönelim.
                            //    {
                            //        UserModel user = _userService.GetById(userId).Entity;
                            //        var (encToken, decToken) = _encryptionService.GenerateToken(user.Email);
                            //        //Oluşturulsn Token Redis'e atılır.
                            //        var createTime = DateTime.Now;

                            //        //Token Oluşturulur. Mobilde ve Web'de 1 saattir. appsettings.json'a bakınız.
                            //        DateTime tokenExpireTime = createTime.AddMinutes(_coreContext.TokenExpireTime);
                            //        _redisCacheService.Set(_redisCacheService.GetTokenKey(userId, isMobile, false, unqDeviceId), decToken, tokenExpireTime);

                            //        //Geri dönülecek Encrypt Token ve Yaratılma zamanı Client'ın Header'ına atanır
                            //        context.HttpContext.Items["token"] = encToken;
                            //        context.HttpContext.Items["createdTokenTime"] = createTime.GetTotalMilliSeconds();

                            //        //RefreshToken Oluşturulur.
                            //        //Refresh Token Mobilde 1 Yıl, Web'de 1.5 saattir. appsettings.json'a bakınız.
                            //        var refreshToken = GenerateRefreshToken(user, context, unqDeviceId, isMobile);
                            //        if (!string.IsNullOrWhiteSpace(refreshToken))
                            //        {
                            //            //Oluşturulan RefreshToken Client'a dönülür.
                            //            context.HttpContext.Items["refreshToken"] = refreshToken;
                            //        }
                            //    }
                            //    else
                            //    {
                            //        context.Result = new UnauthorizedResult();
                            //        return;
                            //    }
                            //}
                            //else
                            //{
                            //    context.Result = new UnauthorizedResult();
                            //    return;
                            //}
                            #endregion
                        }
                        else if ((string.IsNullOrEmpty(cacheRedistoken)) || (!string.IsNullOrEmpty(cacheRedistoken) && cacheRedistoken.Trim() != decryptToken.Trim())) //Redis'de Token Yok Ya da Redis'de Token Var ama tokenlar eşit değil , geçerli bir oturum isteği değil. 
                        {
                            context.Result = new UnauthorizedResult();
                            return;
                        }

                        //Redis'in süresine bakılacak
                        var tokenSession = decryptToken.Split('ß')[2];
                        var sessionCreateTime = DateTime.Parse(tokenSession);
                        var remainingTime = DateTime.Now - sessionCreateTime;

                        //Tokenlar eşit , 45 ile 60'ıncı dakikalar arasındaysa token ve refresh token'ı yenileyip dönelim. Önemli Not: Redis Cache'de Token var ise!
                        //if (remainingTime.TotalMinutes >= _coreContext.TokenExpireTime && remainingTime.TotalMinutes <= _coreContext.TokenExpireTime - 15)
                        //if (remainingTime.TotalMinutes >= _coreContext.TokenExpireTime - 15 && remainingTime.TotalMinutes <= _coreContext.TokenExpireTime)
                        //1. KONTROL
                        if ((string.IsNullOrEmpty(cacheRedistoken) == false) && (remainingTime.TotalMinutes >= _coreContext.TokenExpireTime - 15 && remainingTime.TotalMinutes <= _coreContext.TokenExpireTime))
                        {
                            //------------CheckTime On Redis              

                            //İlgili UserID'ye ait Token Redis'den alınır.
                            //                            cacheRedistoken = _redisCacheService.Get<string>(_redisCacheService.GetTokenKey(userId, isMobile, false, unqDeviceId));
                            //                            var redisToken = cacheRedistoken.Split('ß')[2];
                            //                            var redisTokenCreateTime = DateTime.Parse(redisToken);
                            //                            var redisTokenTime = DateTime.Now - redisTokenCreateTime;

                            //Bu 3. Kontrol oluyordu. Kaldırıldı.
                            //------------End CheckTime on Redis
                            //Check TimeOut 2. Time for Backend Redis!
                            //                            if (redisTokenTime.TotalMinutes >= _coreContext.TokenExpireTime - 15)
                            //                            {
                            lock (lockObject)
                            {
                                //Double Check Lock With Redis Key!
                                //İlgili UserID'ye ait Token Redis'den alınır.
                                cacheRedistoken = _redisCacheService.Get<string>(_redisCacheService.GetTokenKey(userId, isMobile, false, unqDeviceId));
                                var redisToken = cacheRedistoken.Split('ß')[2];
                                var redisTokenCreateTime = DateTime.Parse(redisToken);
                                var redisTokenTime = DateTime.Now - redisTokenCreateTime;
                                //2.KONTROL Check Timeout 2. Time for Backend Redis!
                                if (redisTokenTime.TotalMinutes >= _coreContext.TokenExpireTime - 15)
                                {
                                    CreateTokensByCheckRefreshToken(context);
                                }
                            }
                            //                            }
                            #region CreateTokensByCheckRefreshToken Methodu Altına Taşındı.
                            //if (context.HttpContext.Request.Headers["RefreshToken"].FirstOrDefault() != null) // client refresh token göndermiş.
                            //{
                            //    var clientRefreshToken = context.HttpContext.Request.Headers["RefreshToken"].FirstOrDefault();
                            //    var redisRefreshToken = _redisCacheService.Get<string>(_redisCacheService.GetTokenKey(userId, isMobile, true, unqDeviceId));

                            //    if (string.IsNullOrEmpty(redisRefreshToken))//rediste refresh token yok 
                            //    {
                            //        context.Result = new UnauthorizedResult();
                            //        return;
                            //    }
                            //    var decClientRefreshToken = _encryptionService.DecryptText(clientRefreshToken);
                            //    if (decClientRefreshToken == redisRefreshToken)//Refresh Token doğru. Yeni token ve refresh token üretip dönelim.
                            //    {
                            //        UserModel user = _userService.GetById(userId).Entity;
                            //        var (encToken, decToken) = _encryptionService.GenerateToken(user.Email);
                            //        //Oluşturulan Token Redis'e atılır.

                            //        var createTime = DateTime.Now;
                            //        DateTime tokenExpireTime = createTime.AddMinutes(_coreContext.TokenExpireTime);
                            //        _redisCacheService.Set(_redisCacheService.GetTokenKey(userId, isMobile, false, unqDeviceId), decToken, tokenExpireTime);

                            //        //Geri dönülecek Encrypt Token ve Yaratılma zamanı Client'ın Header'ına atanır
                            //        context.HttpContext.Items["token"] = encToken;
                            //        context.HttpContext.Items["createdTokenTime"] = createTime.GetTotalMilliSeconds();

                            //        //RefreshToken Oluşturulur.
                            //        //Refresh Token Mobilde 1 Yıl Web'de 1.5 saattir. appsettings.json'a bakınız.
                            //        var refreshToken = GenerateRefreshToken(user, context, unqDeviceId, isMobile);
                            //        if (!string.IsNullOrWhiteSpace(refreshToken))
                            //        {
                            //            //Oluşturulan RefreshToken Client'a dönülür.
                            //            context.HttpContext.Items["refreshToken"] = refreshToken;
                            //        }
                            //    }
                            //}
                            #endregion
                        }
                    }
                    else
                    {
                        context.Result = new UnauthorizedResult();
                        return;
                    }
                }

                //Role Yetkisine bakılır.
                if (HasRoleAttribute(context) && !_workContext.IsAdmin)
                {
                    try
                    {
                        var arguments = ((ControllerActionDescriptor)context.ActionDescriptor).MethodInfo.CustomAttributes.FirstOrDefault(fd => fd.AttributeType == typeof(RoleAttribute)).ConstructorArguments;

                        int roleGroupID = (int)arguments[0].Value;
                        Int64 roleID = (Int64)arguments[1].Value;
                        //BeHalfOfUserID değeri var ise o verilir. Yok ise client'ın UserID değeri alınır.
                        int userIDprm = _workContext.CurrentBeHalfOfUserId != 0 ? _workContext.CurrentBeHalfOfUserId : userId;
                        RoleModel role = _roleService.GetRoleById(userIDprm, roleGroupID, roleID).Entity;
                        //RoleModel role = _roleService.GetRoleById(userId, roleGroupID, roleID).Entity;
                        if (role.Id == 0)
                        {
                            //Forbidden 403 Result. Yetkiniz Yoktur..
                            context.Result = new ObjectResult(context.ModelState)
                            {
                                //Value = null,
                                //Value = "You are not authorized for this page",
                                Value = _localizer["Forbidden"],
                                StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden
                            };
                            return;
                        }
                    }
                    catch
                    {
                        int i = 0;
                    }
                }
                else if (HasAdminAttribute(context) && !_workContext.IsAdmin)//User'a Admin yetkisinin verilmesi ya da alınması yetkisine bakılır.
                {
                    //Forbidden 403 Result. Yetkiniz Yoktur..
                    context.Result = new ObjectResult(context.ModelState)
                    {
                        //Value = null,
                        //Value = "You are not Admin for this Action",
                        Value = _localizer["ForbiddenAdmin"],
                        StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden
                    };
                    return;
                }
                else
                {
                    int y = 0;
                }
                //Log işlemleri
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
                            //Burada BeHalhOfUserID'de kullanılabilir.
                            context.HttpContext.Items[userId + "_" + controller + "_" + action] = entity;
                        }
                    }
                    //---------------------------------
                }
            }
            catch (InvalidTokenException ex)
            {
                //Forbidden 430 Result. Yetkiniz Yoktur..
                context.Result = new ObjectResult(context.ModelState)
                {
                    //Value = "Invalid Token Execption." + ex.Message,
                    Value = _localizer["TokenException"] + ex.Message,
                    StatusCode = 430
                };
                return;
            }
        }
        //Burada 3 yerde geçtiği için Extract Function() olarak dışarı alınmıştır. Amaç RefreshToken kontrolü ile platforma göre yeni Tokenların oluşturulmasıdır.
        public void CreateTokensByCheckRefreshToken(ActionExecutingContext context, bool returnResult = false)
        {
            if (context.HttpContext.Request.Headers["RefreshToken"].FirstOrDefault() != null) // client refresh token göndermiş.
            {
                bool.TryParse(context.HttpContext.Request.Headers["IsMobile"].FirstOrDefault(), out var isMobile);
                int.TryParse(context.HttpContext.Request.Headers["UserId"].FirstOrDefault(), out var userId);
                var unqDeviceId = context.HttpContext.Request.Headers["UnqDeviceId"].FirstOrDefault();
                if (userId == 0)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                var clientRefreshToken = context.HttpContext.Request.Headers["RefreshToken"].FirstOrDefault();
                var redisRefreshToken = _redisCacheService.Get<string>(_redisCacheService.GetTokenKey(userId, isMobile, true, unqDeviceId));

                if (string.IsNullOrEmpty(redisRefreshToken))//rediste refresh token yok 
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }
                var decClientRefreshToken = _encryption.DecryptText(clientRefreshToken);
                if (decClientRefreshToken == redisRefreshToken)//Refresh Token doğru. Yeni token ve refresh token üretip dönelim.
                {
                    UserModel user = _userService.GetById(userId).Entity;
                    var (encToken, decToken) = _encryption.GenerateToken(user.Email);
                    //Oluşturulan Token Redis'e atılır.

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
                else if (returnResult)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }
            }
            else if (returnResult)
            {
                context.Result = new UnauthorizedResult();
                return;
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
                    string testLog = ((Core.Models.EmployeeTerritory)entity).FirstName + "-" + ((Core.Models.EmployeeTerritory)entity).Title;
                    string model = Newtonsoft.Json.JsonConvert.SerializeObject(entity);
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

        public bool HasRoleAttribute(FilterContext context)
        {
            return ((ControllerActionDescriptor)context.ActionDescriptor).MethodInfo.CustomAttributes.Any(filterDescriptors => filterDescriptors.AttributeType == typeof(RoleAttribute));
        }
        public bool HasAdminAttribute(FilterContext context)
        {
            return ((ControllerActionDescriptor)context.ActionDescriptor).MethodInfo.CustomAttributes.Any(filterDescriptors => filterDescriptors.AttributeType == typeof(AdminAttribute));
        }

        public string GenerateRefreshToken(UserModel user, ActionExecutingContext context, string unqDeviceId, bool isMobile)
        {
            var createTime = DateTime.Now;
            DateTime tokenExpireTime = createTime.AddMinutes(_coreContext.RefreshTokenExpireTime);
            if (isMobile)
            {
                tokenExpireTime = createTime.AddMinutes(_coreContext.MobileRefreshTokenExpireTime);
            }
            var (encToken, decToken) = _encryption.GenerateToken(user.Email);
            _redisCacheService.Set(_redisCacheService.GetTokenKey(user.Id, isMobile, true, unqDeviceId), decToken, tokenExpireTime);
            return encToken;
        }
    }
}
