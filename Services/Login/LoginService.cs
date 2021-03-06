﻿using Core.ApiResponse;
using Core.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;
using Repository;
using System.Linq;
using Services.Users;
using Core.Caching;
using Core.CoreContext;
using Core.Extensions;
using Core.Security;

namespace Services.Login
{
    public class LoginService : ILoginService
    {
        private readonly IEncryption _encryptionService;
        private readonly IRepository<DB.Entities.Users> _userRepository;
        private readonly IUserService _userService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly ICoreContext _coreContext;
        public LoginService(IEncryption encryptionService, IRepository<DB.Entities.Users> userRepository, IUserService userService, IRedisCacheService redisCacheService, ICoreContext coreContext)
        {
            _encryptionService = encryptionService;
            _userRepository = userRepository;
            _userService = userService;
            _redisCacheService = redisCacheService;
            _coreContext = coreContext;
        }

        public ServiceResponse<LoginResultModel> CheckLogin(LoginModel model)
        {
            var isMobile = model.IsMobile;
            string decPassword;
            try
            {
                //Client'dan encrypted olarak gelen Password Decrypt edilir.
                //Example Password: vbt123456 ==> dmJ0MTIzNDU2
                decPassword = _encryptionService.DecryptFromClientData(model.Password);
                if (model.Password != "" && decPassword == "")
                {
                    //string message = "Şifre işleminde bir problem yaşandı lütfen teknik destek alın.";
                    string message = "PasswordError";
                    //                    if (isMobile)
                    //                        message = "Lütfen mağazadan uygulamanın yeni versiyonunu indiriniz.";
                    var response = new ServiceResponse<LoginResultModel>(null);
                    response.Entity = new LoginResultModel { UserId = -2, ExceptionMessage = message };
                    response.IsSuccessful = false;
                    return response;
                }
            }
            catch
            {
                //string message = "Şifre işleminde bir problem yaşandı lütfen teknik destek alın.";
                string message = "PasswordError";
                //                if (isMobile)
                //                    message = "Lütfen mağazadan uygulamanın yeni versiyonunu indiriniz.";
                var response = new ServiceResponse<LoginResultModel>(null);
                response.Entity = new LoginResultModel { UserId = -2, ExceptionMessage = message };
                response.IsSuccessful = false;
                return response;
            }
            var user = IsValidUserAndPasswordCombination(model.UserName, decPassword);
            if (user != null)
            {
                //Eğer Mobil ise
                if (model.IsMobile && !string.IsNullOrEmpty(model.UnqDeviceId))
                {
                    var loginedCount = (decimal)_redisCacheService.Count($"{user.Id}*");
                    //Aynı account ile En Fazla 2 Mobile Cihazın Girilmesine İzin Verilir.
                    if (loginedCount >= 2)
                    {
                        //Mobilden Login Olunmuş ise RefreshToken Her zaman Alınır (true)
                        var controlCacheKey = _redisCacheService.GetTokenKey(user.Id, isMobile, true, model.UnqDeviceId);
                        var controlKey = _redisCacheService.Get<string>(controlCacheKey);

                        if (string.IsNullOrEmpty(controlKey))
                        {
                            var response2 = new ServiceResponse<LoginResultModel>(null);                            
                            //response2.Entity = new LoginResultModel { UserId = -1, ExceptionMessage = "En fazla 2 farklı mobil cihazdan giriş yapabilirsiniz." };
                            response2.Entity = new LoginResultModel { UserId = -1, ExceptionMessage = "OverMobileUsageExeception" };
                            response2.IsSuccessful = false;
                            return response2;
                        }

                    }
                }
                //User Session'a Atılabilir. Sonradan Kullanmak için.
                ////3.1'de Destek Yok. HttpContext.Session.SetObject("User", user);

                var loginResultModel = new LoginResultModel
                {
                    UserName = user.UserName,
                    Name = user.Name,
                    UserId = user.Id,
                    IsAdmin = user.IsAdmin,
                };

                //Token
                var (encToken, decToken) = _encryptionService.GenerateToken(user.Email);
                loginResultModel.Token = encToken;

                var createTime = DateTime.Now;
                var cacheKey = _redisCacheService.GetTokenKey(user.Id, isMobile, false, model.UnqDeviceId);
                _redisCacheService.Set(cacheKey, decToken, createTime.AddMinutes(_coreContext.TokenExpireTime));// 1 saatlik Token Açık Atılır.

                DateTime tokenExpireTime = createTime.AddMinutes(_coreContext.RefreshTokenExpireTime);
                if (isMobile)
                {
                    tokenExpireTime = createTime.AddMinutes(_coreContext.MobileRefreshTokenExpireTime);
                    //tokenExpireTime = createTime.AddDays(365);
                }
                //RefreshToken
                var refreshToken = _encryptionService.GenerateToken(user.Email);
                loginResultModel.RefreshToken = refreshToken.encToken;
                _redisCacheService.Set(_redisCacheService.GetTokenKey(user.Id, isMobile, true, model.UnqDeviceId), refreshToken.decToken, tokenExpireTime);

                loginResultModel.CreatedTokenTime = createTime.GetTotalMilliSeconds();
                var response = new ServiceResponse<LoginResultModel>(null);
                response.Entity = loginResultModel;
                response.IsSuccessful = true;
                return response;
            }
            else
            {
                var loginResultModel = new LoginResultModel();
                var response = new ServiceResponse<LoginResultModel>(null);
                response.Entity = loginResultModel;
                response.IsSuccessful = false;
                return response;
            }
        }
        private UserModel IsValidUserAndPasswordCombination(string username, string password/*, out bool hasChangePassword*/)
        {
            //hasChangePassword = false;
            UserModel userData = null;
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                var user = _userRepository.Table.FirstOrDefault(k => k.UserName.Equals(username));

                if (user != null)
                {
                    //Test User Password vbt123456 ==> dmJ0MTIzNDU2(Client Encryption) (Salt: LnTvXQl1IJ5g+rcw6dx5Zw==)(Encrypt: "Kol1K0t4wW193Np8NcI/SSCwF3tnJvAG1Z7LUg623FQ=") 
                    var encryptKey = user.PasswordHash;

                    string getEncryptKey = encryptKey.Split('æ')[0];
                    string getSalt = encryptKey.Split('æ')[1];
                    /* RESET PASSWORD
                    if (getEncryptKey == "Zk2IuJoPqcfxdlhP4wl0ef7J530eS0KA25OaNYfWJ2w" && user.PasswordHash == _encryptionService.HashCreate(password, getSalt))
                    {
                        hasChangePassword = true;
                    }
                    */
                    bool isSuccess = _encryptionService.ValidateHash(password, getSalt, getEncryptKey);

                    if (isSuccess)
                    {
                        userData = _userService.GetById(user.Id).Entity;
                    }
                }
                return userData;
            }
            return null;
        }

        public ServiceResponse<LoginResultModel> CheckBeHalfofPassword(string _beHalfOfPassword)
        {
            string beHalfOfPassword = _encryptionService.DecryptFromClientData(_beHalfOfPassword); //Client'da şifrelenen Password, DecryptFromClientData ile Decrypt edilir.

            string beHalfOfKey = _redisCacheService.GetKeyWithBeHalfOfPassword(beHalfOfPassword, out string beHalfofUserId);
            var beHalfOfToken = _redisCacheService.Get<string>(beHalfOfKey);
            var loginResultModel = new LoginResultModel
            {
                BeHalfOfPassword = beHalfOfPassword,
                BeHalfOfToken = _encryptionService.EncryptText(beHalfOfToken),
                BeHalfOfUserId = beHalfofUserId,
            };
            var response = new ServiceResponse<LoginResultModel>(null);
            response.Entity = loginResultModel;
            return response;
        }
        public ServiceResponse<LoginResultModel> GetOnBeHalfofPassword(int userId)
        {
            var user = _userRepository.Table.FirstOrDefault(k => k.Id.Equals(userId));

            if (user != null)
            {
                var loginResultModel = new LoginResultModel
                {
                    UserName = user.UserName,
                    Name = user.Name,
                    UserId = user.Id,
                };

                //Token
                var (encToken, decToken) = _encryptionService.GenerateToken(user.Email);
                loginResultModel.BeHalfOfToken = encToken;

                var createTime = DateTime.Now;
                var cacheKey = _redisCacheService.GetTokenKeyForBeHalfOf(user.Id);
                _redisCacheService.Set(cacheKey, decToken, createTime.AddMinutes(_coreContext.TokenBeHalfOfExpireTime));// 1 saatlik TokenBeHalfOf Açık Atılır.

                loginResultModel.CreatedTokenTime = createTime.GetTotalMilliSeconds();
                loginResultModel.BeHalfOfPassword = cacheKey.Split(':').Length > 1 ? cacheKey.Split(':')[2] : "";
                loginResultModel.BeHalfOfUserId = user.Id.ToString();

                var response = new ServiceResponse<LoginResultModel>(null);
                response.Entity = loginResultModel;
                return response;
            }
            else
            {
                var loginResultModel = new LoginResultModel();
                var response = new ServiceResponse<LoginResultModel>(null);
                response.Entity = loginResultModel;
                return response;
            }
        }
        public IServiceResponse<bool> Delete(long id, int userId)
        {
            throw new NotImplementedException();
        }

        public IServiceResponse<LoginModel> GetAll(int page, int pageSize, int userId)
        {
            throw new NotImplementedException();
        }

        public IServiceResponse<LoginModel> GetById(long id, int userId)
        {
            throw new NotImplementedException();
        }

        public IServiceResponse<LoginModel> Insert(LoginModel entityViewModel, int userId)
        {
            throw new NotImplementedException();
        }

        public IServiceResponse<LoginModel> Update(LoginModel entityViewModel, int userId)
        {
            throw new NotImplementedException();
        }
    }
}
