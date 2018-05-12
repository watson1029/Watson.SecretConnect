using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watson.SecretConnect
{
    internal class ConnectDecrypt : SecretHelper.SecretBase
    {
        /// <summary>
        /// 获取连接字符串
        /// </summary>
        /// <param name="nameOrConnectionString"></param>
        /// <returns></returns>
        public static string GetConnectionString(string nameOrConnectionString)
        {
            string connectionString = string.Empty;
            // 判断是Name还是ConnectString
            if (nameOrConnectionString.StartsWith("name="))
                connectionString = ConfigurationManager.ConnectionStrings[nameOrConnectionString.Substring(5)].ConnectionString;
            else
                connectionString = nameOrConnectionString;
            // 判断IsSecret
            if (ConfigurationManager.AppSettings["IsSecret"].ToLower().Equals("no"))
                return connectionString;
            else
            {
                // 使用AES解密连接字符串
                SecretHelper.AES aes = new SecretHelper.AES(GetKey(), GetIV());
                return aes.AesDecrypt(connectionString);
            }
        }

        /// <summary>
        /// 获取Key
        /// </summary>
        /// <returns></returns>
        private static string GetKey()
        {
            string key = string.Empty;
            string projectKey = ConfigurationManager.AppSettings["SecretKey"];
            // SecretKey和attachKey混合
            if (attachKey.Length > projectKey.Length)
            {
                int interval = Convert.ToInt32(Math.Floor(Convert.ToDecimal(attachKey.Length / projectKey.Length)));
                for (int i = 0, j = 0; i < attachKey.Length;)
                {
                    key += attachKey[i++];
                    if (i % interval == 0)
                    {
                        if (j < projectKey.Length)
                            key += projectKey[j++];
                    }
                }
            }
            else
            {
                int interval = Convert.ToInt32(Math.Floor(Convert.ToDecimal(projectKey.Length / attachKey.Length)));
                for (int i = 0, j = 0; i < projectKey.Length;)
                {
                    key += projectKey[i++];
                    if (i % interval == 0)
                    {
                        if (j < attachKey.Length)
                            key += attachKey[j++];
                    }
                }
            }
            // 混合后的Key使用摘要算法->256位
            return SecretHelper.Abstract.EncryptMD5(key);
        }

        /// <summary>
        /// 获取偏移向量IV
        /// </summary>
        /// <returns></returns>
        private static string GetIV()
        {
            string iv16 = string.Empty;
            string iv = string.Empty;
            // 判断是否需要自定义偏移向量
            if (ConfigurationManager.AppSettings["SecretIV"] == null)
                iv = SecretHelper.Abstract.EncryptMD5(attachIV);
            else
                iv = SecretHelper.Abstract.EncryptMD5(GetDesDecrypt() + attachIV);
            // 将256位摘要数据截取成128位偏移向量
            for (int i = 0; i < iv.Length; i++)
            {
                if (i % 2 == 0)
                    iv16 += iv[i];
            }
            return iv16;
        }

        /// <summary>
        /// 获取经过DES解密的偏移向量
        /// </summary>
        /// <returns></returns>
        private static string GetDesDecrypt()
        {
            string code = ConfigurationManager.AppSettings["SecretIV"];
            string key = GetKey();
            string key8 = string.Empty;
            // 将256位Key截取成DES所需的64位Key
            for (int i = 0; i < key.Length; i++)
            {
                if (i % 4 == 0)
                    key8 += key[i];
            }
            string iv = SecretHelper.Abstract.EncryptMD5(attachIV);
            string iv16 = string.Empty;
            // 将256位摘要数据截取成128位偏移向量
            for (int j = 0; j < iv.Length; j++)
            {
                if (j % 2 == 0)
                    iv16 += iv[j];
            }
            // 使用DES解密偏移向量
            SecretHelper.DES des = new SecretHelper.DES(key8, iv16);
            return des.DesDecrypt(code);
        }
    }
}
