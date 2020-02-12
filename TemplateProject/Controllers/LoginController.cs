using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.ApiResponse;
using Core.Models;
using Core.Extensions;
using DB.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repository;
using Services;
using Services.Customers;
using AutoMapper;
using Core.Filters.Customers;
using Services.Employees;
using Core.Caching;
using Core.Models.Users;
using Core.CoreContext;
using Services.SecurityService;
using Services.Users;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Services.Login;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.Extensions.Localization;
using Core;
using TemplateProject.Infrastructure;

namespace TemplateProject.Controllers
{
    [Produces("application/json")]
    [Route("api/login")]
    public class LoginController : ControllerBase
    {
        private readonly ICoreContext _coreContext;
        private readonly ILoginService _loginService;

        private readonly IStringLocalizer<VbtController> _localizer;

        private readonly IWorkContext _workContext;

        public LoginController(ICoreContext coreContext, ILoginService loginService, IStringLocalizer<VbtController> localizer, IWorkContext workContext)
        {
            _coreContext = coreContext;
            _loginService = loginService;
            _localizer = localizer;
            _workContext = workContext;
        }

        //Salt : MVvZKOwLX9G0yWwDChW3Xg==
        //Encrpted PASSWORD : MLhoe+8evQpsAMnBfQfeag+Crj0c32psTF/oXNmXTDE=
        //Redis Password : MLhoe+8evQpsAMnBfQfeag+Crj0c32psTF/oXNmXTDE=æMVvZKOwLX9G0yWwDChW3Xg==
        /// <summary>
        /// Test User Password vbt123456 ==> dmJ0MTIzNDU2
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("login")]
        [HttpPost]
        //Amaç Swagger'da açıklama olarak girilir.
        [SwaggerOperation(Summary = "Test User Password vbt123456 ==> dmJ0MTIzNDU2", Description = "<b>Test User</b> </br><b>Password:</b> dmJ0MTIzNDU2 <br> <b>UserName:</b> bkasmer </br> <b>IsMobile:</b> false </br> <b>UnqDeviceId:</b> \"\"  <br> <b>(Mobile ise)MobileVersion : </b> 1.0.0")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            var isMobile = model.IsMobile;
            if (isMobile)
            {
                var mobileVersion = HttpContext.Request.Headers["MobileVersion"].FirstOrDefault();
                var mobileVersions = new List<string>();
                //Tanımlı Mobile versiyonlar configden okunur
                _coreContext.MobileVersion.Split('-').ToList().ForEach(s => mobileVersions.Add(s));
                //Gelen Mobile cihaz tanımlı versiyonlardan biri değilse Store'a yönlendirilir.
                if (!mobileVersions.Any(s => s == mobileVersion))
                {
                    //const string message = "Lütfen mağazadan uygulamanın yeni versiyonunu indiriniz.";
                    string message = _localizer["NewVersion"];
                    return new ObjectResult(new LoginResultModel { UserId = -2, ExceptionMessage = message });
                }
            }
            var loginResultModel = _loginService.CheckLogin(model).Entity;
            string errorCode = loginResultModel.ExceptionMessage;
            if (errorCode != null)
            {
                loginResultModel.ExceptionMessage = _localizer[errorCode];
            }
            return new ObjectResult(loginResultModel);
        }

        [ServiceFilter(typeof(LoginFilter))]
        [Route("GetOnBeHalfOfPassword")]
        [HttpPost]
        public IActionResult GetOnBeHalfofPassword()
        {
            var userID = _workContext.CurrentUserId;
            var loginResultModel = _loginService.GetOnBeHalfofPassword(userID).Entity;
            return new ObjectResult(loginResultModel);
        }

        [ServiceFilter(typeof(LoginFilter))]
        [Route("LoginAsBeHalfof/{beHalfofPassword}")]
        [HttpPost]
        public IActionResult LoginAsBeHalfof(string beHalfofPassword)
        {
            if (!string.IsNullOrEmpty(beHalfofPassword))
            {
                var userID = _workContext.CurrentUserId;
                var loginResultModel = _loginService.CheckBeHalfofPassword(beHalfofPassword).Entity;
                loginResultModel.UserId = userID;
                if (loginResultModel.BeHalfOfPassword == "")
                {
                    return new ObjectResult(loginResultModel)
                    {
                        Value = _localizer["BeHalfofTokenException"],
                        StatusCode = 431
                    };
                }
                return new ObjectResult(loginResultModel);
            }
            return new ObjectResult(null);
        }

    }
}
