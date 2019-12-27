using Core.ApiResponse;
using Core.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;
using Services.SecurityService;
using Repository;
using System.Linq;
using Services.Users;
using Core.Caching;
using Core.CoreContext;
using Core.Extensions;

namespace Services.Login
{
    public class LoginService : ILoginService
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IRepository<DB.Entities.Users> _userRepository;
        private readonly IUserService _userService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly ICoreContext _coreContext;
        public LoginService(IEncryptionService encryptionService, IRepository<DB.Entities.Users> userRepository, IUserService userService, IRedisCacheService redisCacheService, ICoreContext coreContext)
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
            }
            catch
            {
                string message = "Şifre işleminde bir problem yaşandı lütfen teknik destek alın.";
                if (isMobile)
                    message = "Lütfen mağazadan uygulamanın yeni versiyonunu indiriniz.";
                var response = new ServiceResponse<LoginResultModel>(null);
                response.Entity = new LoginResultModel { UserId = -2, ExceptionMessage = message };
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
                            response2.Entity = new LoginResultModel { UserId = -1, ExceptionMessage = "En fazla 2 farklı mobil cihazdan giriş yapabilirsiniz." };
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
                };

                //Token
                var (encToken, decToken) = _encryptionService.GenerateToken(user.Email);
                loginResultModel.Token = encToken;

                var createTime = DateTime.Now;
                var cacheKey = _redisCacheService.GetTokenKey(user.Id, isMobile, false, model.UnqDeviceId);
                _redisCacheService.Set(cacheKey, decToken, createTime.AddMinutes(_coreContext.TokenExpireTime));// 3 saatlik Token Açık Atılır.

                DateTime tokenExpireTime = createTime.AddMinutes(_coreContext.RefreshTokenExpireTime);
                if (isMobile)
                {
                    tokenExpireTime = createTime.AddDays(365);
                }
                //RefreshToken
                var refreshToken = _encryptionService.GenerateToken(user.Email);
                loginResultModel.RefreshToken = refreshToken.encToken;
                _redisCacheService.Set(_redisCacheService.GetTokenKey(user.Id, isMobile, true, model.UnqDeviceId), refreshToken.decToken, tokenExpireTime);

                loginResultModel.CreatedTokenTime = createTime.GetTotalMilliSeconds();
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
