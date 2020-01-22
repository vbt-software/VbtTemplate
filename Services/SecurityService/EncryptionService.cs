using Core.Configuration;
using Core.CustomException;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Services.SecurityService
{
    public class EncryptionService : IEncryptionService
    {
        private readonly IOptions<VbtConfig> _vbtConfig;
        public EncryptionService(IOptions<VbtConfig> vbtConfig)
        {
            _vbtConfig = vbtConfig;
        }


        #region Utilty

        private byte[] EncryptTextToMemory(string data, byte[] key, byte[] iv)
        {
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, new TripleDESCryptoServiceProvider().CreateEncryptor(key, iv), CryptoStreamMode.Write))
                {
                    var toEncrypt = Encoding.Unicode.GetBytes(data);
                    cs.Write(toEncrypt, 0, toEncrypt.Length);
                    cs.FlushFinalBlock();
                }

                return ms.ToArray();
            }
        }

        private string DecryptTextFromMemory(byte[] data, byte[] key, byte[] iv)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var cs = new CryptoStream(ms, new TripleDESCryptoServiceProvider().CreateDecryptor(key, iv), CryptoStreamMode.Read))
                {
                    using (var sr = new StreamReader(cs, Encoding.Unicode))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }

        #endregion

        public string EncryptText(string text, string privateKey = "")
        {
            if (string.IsNullOrEmpty(text) || text == "null")
                return string.Empty;

            if (string.IsNullOrEmpty(privateKey))
                privateKey = _vbtConfig.Value.PrivateKey;

            using (var provider = new TripleDESCryptoServiceProvider())
            {
                provider.Key = Encoding.ASCII.GetBytes(privateKey.Substring(0, 16));
                provider.IV = Encoding.ASCII.GetBytes(privateKey.Substring(8, 8));

                var encryptedBinary = EncryptTextToMemory(text, provider.Key, provider.IV);
                return Convert.ToBase64String(encryptedBinary);
            }
        }

        //Example Password: 123456 ==> MTIzNDU2
        public string DecryptText(string text, string privateKey = "")
        {
            try
            {
                if (string.IsNullOrEmpty(text) || text == "null")
                    return string.Empty;

                if (string.IsNullOrEmpty(privateKey))
                    privateKey = _vbtConfig.Value.PrivateKey;

                using (var provider = new TripleDESCryptoServiceProvider())
                {
                    provider.Key = Encoding.ASCII.GetBytes(privateKey.Substring(0, 16));
                    provider.IV = Encoding.ASCII.GetBytes(privateKey.Substring(8, 8));

                    var buffer = Convert.FromBase64String(text);
                    return DecryptTextFromMemory(buffer, provider.Key, provider.IV);
                }
            }
            catch
            {
                throw new InvalidTokenException();
            }
        }
        public (string encToken, string decToken) GenerateToken(string emailUser)
        {
            var token = new StringBuilder();

            var guid = Guid.NewGuid();
            var time = DateTime.Now;
            var email = emailUser;
            token.Append(email);
            token.Append("ß");
            token.Append(guid);
            token.Append("ß");
            token.Append(time);

            var encryptToken = EncryptText(token.ToString());

            return (encryptToken, token.ToString());
        }

        #region HashPassword

        public string HashCreate(string value, string salt)
        {
            var valueBytes = Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivation.Pbkdf2(
                value,
                Encoding.UTF8.GetBytes(salt),
                Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivationPrf.HMACSHA512,
                10000,
                256 / 8);

            return System.Convert.ToBase64String(valueBytes) + "æ" + salt;
        }

        public bool ValidateHash(string value, string salt, string hash)
            //=> HashCreate(value, salt) == hash;
            => HashCreate(value, salt).Split('æ')[0] == hash;

        public string GenerateSalt()
        {
            byte[] randomBytes = new byte[128 / 8];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }

        public string DecryptFromClientData(string password)
        {
            string keyStr = "ABCDEFGHIJKLMNOP" +
                            "QRSTUVWXYZabcdef" +
                            "ghijklmnopqrstuv" +
                            "wxyz0123456789+/" +
                            "=";

            var output = "";
            int? chr1;
            int? chr2;
            int? chr3;

            int? enc1;
            int? enc2;
            int? enc3;
            int? enc4;

            var i = 0;
            // remove all characters that are not A-Z, a-z, 0-9, +, /, or =
            Regex base64test = new Regex(@"/[^A-Za-z0-9\+\/\=]/g;");

            if (base64test.Match(password).Success)
            {
                Console.WriteLine("There were invalid base64 characters in the input text.\n" +
                                  "Valid base64 characters are A-Z, a-z, 0-9, '+', '/',and '='\n" +
                                  "Expect errors in decoding.");
            }
            password = password.Replace(@"/[^ A - Za - z0 - 9\+\/\=] / g", "");
            do
            {
                enc1 = keyStr.IndexOf(password[i++]);
                enc2 = keyStr.IndexOf(password[i++]);
                enc3 = keyStr.IndexOf(password[i++]);
                enc4 = keyStr.IndexOf(password[i++]);

                chr1 = (enc1 << 2) | (enc2 >> 4);
                chr2 = ((enc2 & 15) << 4) | (enc3 >> 2);
                chr3 = ((enc3 & 3) << 6) | enc4;

                output = output + (char)chr1;
                if (enc3 != 64)
                {
                    output = output + (char)chr2;
                }
                if (enc4 != 64)
                {
                    output = output + (char)chr3;
                }
                chr1 = chr2 = chr3 = null;
                enc1 = enc2 = enc3 = enc4 = null;
            } while (i < password.Length);
            output = System.Web.HttpUtility.UrlDecode(output, Encoding.UTF8);
            var pattern = new Regex("[|]");
            output = pattern.Replace(output, "+");
            return output;
        }

        #region ClientSideEncryptionCode
        /*Client Side Encryption For Angular TypeScript
  title = 'encyrpt';
  password: string;
  sendPassword:string;
  keyStr = "ABCDEFGHIJKLMNOP" +
    "QRSTUVWXYZabcdef" +
    "ghijklmnopqrstuv" +
    "wxyz0123456789+/" +
    "=";

 public Encrypt() {
    this.sendPassword=this.password.split('+').join('|');
    let input = escape(this.sendPassword);
    console.log("Escape:"+input);
    let output = "";
    let chr1, chr2, chr3;
    let enc1, enc2, enc3, enc4;
    let i = 0;

    do {
      chr1 = input.charCodeAt(i++);
      chr2 = input.charCodeAt(i++);
      chr3 = input.charCodeAt(i++);

      enc1 = chr1 >> 2;
      enc2 = ((chr1 & 3) << 4) | (chr2 >> 4);
      enc3 = ((chr2 & 15) << 2) | (chr3 >> 6);
      enc4 = chr3 & 63;

      if (isNaN(chr2)) {
        enc3 = enc4 = 64;
      } else if (isNaN(chr3)) {
        enc4 = 64;

      }
      output = output +
        this.keyStr.charAt(enc1) +
        this.keyStr.charAt(enc2) +
        this.keyStr.charAt(enc3) +
        this.keyStr.charAt(enc4);
      chr1 = chr2 = chr3 = "";
      enc1 = enc2 = enc3 = enc4 = "";
    } while (i < input.length);
    console.log("Password :" + output);
  }
         */
        #endregion

        #endregion
    }
}
