using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watson.SecretGenerate
{
    internal class ConnectEncrypt : SecretHelper.SecretBase
    {
        private string secretKey;
        private string secretIV;
        private string RandomIV;
        public ConnectEncrypt(string secretKey, bool isDesIV)
        {
            // 获取secretKey
            this.secretKey = GetKey(secretKey);
            string ivmd5 = string.Empty;
            // 是否使用随机IV
            if (isDesIV)
            {
                this.RandomIV = Guid.NewGuid().ToString("N").ToUpper();
                ivmd5 = SecretHelper.Abstract.EncryptMD5(this.RandomIV + attachIV);
            }
            else
                ivmd5 = SecretHelper.Abstract.EncryptMD5(attachIV);
            // 将256位摘要数据截取成128位偏移向量
            for (int i = 0; i < ivmd5.Length; i++)
            {
                if (i % 2 == 0)
                    this.secretIV += ivmd5[i];
            }
        }

        /// <summary>
        /// 将连接字符串加密
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public string SecretCode(string connectionString)
        {
            // 使用AES加密连接字符串
            SecretHelper.AES aes = new SecretHelper.AES(secretKey, secretIV);
            return aes.AesEncrypt(connectionString);
        }

        public string SecretIV()
        {
            string key8 = string.Empty;
            // 将256位Key截取成DES所需的64位Key
            for (int i = 0; i < secretKey.Length; i++)
            {
                if (i % 4 == 0)
                    key8 += secretKey[i];
            }
            string iv = SecretHelper.Abstract.EncryptMD5(attachIV);
            string iv16 = string.Empty;
            // 将256位摘要数据截取成128位偏移向量
            for (int j = 0; j < iv.Length; j++)
            {
                if (j % 2 == 0)
                    iv16 += iv[j];
            }
            // 使用DES加密随机向量
            SecretHelper.DES des = new SecretHelper.DES(key8, iv16);
            return des.DesEncrypt(RandomIV);
        }

        /// <summary>
        /// 获取Key
        /// </summary>
        /// <returns></returns>
        private string GetKey(string secretKey)
        {
            string key = string.Empty;
            string projectKey = secretKey;
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
    }
}
