using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace Watson.SecretConnect.SecretHelper
{
    internal abstract class Abstract
    {
        #region MD5
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="message">待加密字符串</param>
        /// <returns>加密字符串</returns>
        public static string EncryptMD5(string Input)
        {
            string output = null;
            if (string.IsNullOrEmpty(Input))
            {
                output = Input;
            }
            else
            {
                MD5 algorithm = MD5.Create();
                byte[] data = algorithm.ComputeHash(Encoding.UTF8.GetBytes(Input));
                for (int i = 0; i < data.Length; i++)
                {
                    output += data[i].ToString("x2").ToUpperInvariant();
                }
            }
            return output;
        }
        #endregion

        #region SHA1
        /// <summary>
        /// SHA1加密
        /// </summary>
        /// <param name="message">待加密字符串</param>
        /// <returns>加密字符串</returns>
        public static string EncryptSHA1(string Input)
        {
            string output = null;
            if (string.IsNullOrEmpty(Input))
            {
                output = Input;
            }
            else
            {
                SHA1 algorithm = SHA1.Create();
                byte[] data = algorithm.ComputeHash(Encoding.UTF8.GetBytes(Input));
                for (int i = 0; i < data.Length; i++)
                {
                    output += data[i].ToString("x2").ToUpperInvariant();
                }
            }
            return output;
        }
        #endregion
    }
}
