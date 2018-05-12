using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Watson.SecretConnect.SecretHelper
{
    internal class AES
    {
        private RijndaelManaged aes;
        private string Key;
        private string IV;

        #region AES算法构造函数
        /// <summary>
        /// AES算法构造函数（KEY和IV随机生成）
        /// </summary>
        public AES()
        {
            aes = new RijndaelManaged();
            aes.GenerateKey();
            aes.GenerateIV();
            Key = Convert.ToBase64String(aes.Key);
            IV = Convert.ToBase64String(aes.IV);
        }

        /// <summary>
        /// AES构造函数（KEY自定义，IV随机生成）
        /// </summary>
        /// <param name="sKey">32位KEY</param>
        public AES(string sKey)
        {
            aes = new RijndaelManaged();
            aes.GenerateIV();
            Key = sKey;
            IV = Convert.ToBase64String(aes.IV);
        }

        /// <summary>
        /// AES构造函数（KEY和IV自定义）
        /// </summary>
        /// <param name="sKey">32位KEY</param>
        /// <param name="sIV">16位IV</param>
        public AES(string sKey, string sIV)
        {
            aes = new RijndaelManaged();
            Key = sKey;
            IV = sIV;
        }
        #endregion

        #region Get方法
        /// <summary>
        /// 获取KEY
        /// </summary>
        public string getKey
        {
            get
            {
                return Key;
            }
        }
        /// <summary>
        /// 获取IV
        /// </summary>
        public string getIV
        {
            get
            {
                return IV;
            }
        }
        #endregion

        #region 获取KEY和IV的私有方法
        /// <summary>
        /// 获得密钥
        /// </summary>
        /// <returns>密钥</returns>
        private byte[] GetLegalKey()
        {
            string sTemp = Key;
            aes.GenerateKey();
            byte[] bytTemp = aes.Key;
            int KeyLength = bytTemp.Length;
            if (sTemp.Length > KeyLength)
                sTemp = sTemp.Substring(0, KeyLength);
            else if (sTemp.Length < KeyLength)
                sTemp = sTemp.PadRight(KeyLength, ' ');
            return ASCIIEncoding.ASCII.GetBytes(sTemp);
        }

        /// <summary>
        /// 获得初始向量IV
        /// </summary>
        /// <returns>初试向量IV</returns>
        private byte[] GetLegalIV()
        {
            string sTemp = IV;
            aes.GenerateIV();
            byte[] bytTemp = aes.IV;
            int IVLength = bytTemp.Length;
            if (sTemp.Length > IVLength)
                sTemp = sTemp.Substring(0, IVLength);
            else if (sTemp.Length < IVLength)
                sTemp = sTemp.PadRight(IVLength, ' ');
            return ASCIIEncoding.ASCII.GetBytes(sTemp);
        }
        #endregion

        #region String加密解密操作
        /// <summary>
        /// AES加密方法（失败返回NULL）
        /// </summary>
        /// <param name="Source">明文</param>
        /// <returns>密文</returns>
        public string AesEncrypt(string Source)
        {
            try
            {
                byte[] bytIn = UTF8Encoding.UTF8.GetBytes(Source);
                MemoryStream ms = new MemoryStream();
                aes.Key = GetLegalKey();
                aes.IV = GetLegalIV();
                ICryptoTransform encrypto = aes.CreateEncryptor();
                CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);
                cs.Write(bytIn, 0, bytIn.Length);
                cs.FlushFinalBlock();
                ms.Close();
                byte[] bytOut = ms.ToArray();
                return Convert.ToBase64String(bytOut);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// AES解密方法（失败返回NULL）
        /// </summary>
        /// <param name="Source">密文</param>
        /// <returns>明文</returns>
        public string AesDecrypt(string Source)
        {
            try
            {
                byte[] bytIn = Convert.FromBase64String(Source);
                MemoryStream ms = new MemoryStream(bytIn, 0, bytIn.Length);
                aes.Key = GetLegalKey();
                aes.IV = GetLegalIV();
                ICryptoTransform encrypto = aes.CreateDecryptor();
                CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Read);
                StreamReader sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region byte[]加密解密操作
        /// <summary>
        /// AES加密方法byte[] to byte[]（失败返回NULL）
        /// </summary>
        /// <param name="Source">明文</param>
        /// <returns>密文</returns>
        public byte[] AesEncrypt(byte[] Source)
        {
            try
            {
                byte[] bytIn = Source;
                MemoryStream ms = new MemoryStream();
                aes.Key = GetLegalKey();
                aes.IV = GetLegalIV();
                ICryptoTransform encrypto = aes.CreateEncryptor();
                CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);
                cs.Write(bytIn, 0, bytIn.Length);
                cs.FlushFinalBlock();
                ms.Close();
                byte[] bytOut = ms.ToArray();
                return bytOut;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// AES解密方法byte[] to byte[]（失败返回NULL）
        /// </summary>
        /// <param name="Source">密文</param>
        /// <returns>明文</returns>
        public byte[] AesDecrypt(byte[] Source)
        {
            try
            {
                byte[] bytIn = Source;
                MemoryStream ms = new MemoryStream(bytIn, 0, bytIn.Length);
                aes.Key = GetLegalKey();
                aes.IV = GetLegalIV();
                ICryptoTransform encrypto = aes.CreateDecryptor();
                CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Read);
                StreamReader sr = new StreamReader(cs);
                return UTF8Encoding.UTF8.GetBytes(sr.ReadToEnd());
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region 文件加密解密操作
        /// <summary>
        /// AES加密方法File to File（失败返回NULL）
        /// </summary>
        /// <param name="inFileName">待加密文件的路径</param>
        /// <param name="outFileName">待加密后文件的输出路径</param>
        public void AesEncrypt(string inFileName, string outFileName)
        {
            try
            {
                FileStream fin = new FileStream(inFileName, FileMode.Open, FileAccess.Read);
                FileStream fout = new FileStream(outFileName, FileMode.OpenOrCreate, FileAccess.Write);
                fout.SetLength(0);

                aes.Key = GetLegalKey();
                aes.IV = GetLegalIV();

                byte[] bin = new byte[100];
                long rdlen = 0;
                long totlen = fin.Length;
                int len;

                ICryptoTransform encrypto = aes.CreateEncryptor();
                CryptoStream cs = new CryptoStream(fout, encrypto, CryptoStreamMode.Write);
                while (rdlen < totlen)
                {
                    len = fin.Read(bin, 0, 100);
                    cs.Write(bin, 0, len);
                    rdlen = rdlen + len;
                }
                cs.Close();
                fout.Close();
                fin.Close();
            }
            catch (Exception ex)
            {
                return;
            }
        }

        /// <summary>
        /// AES解密方法File to File（失败返回NULL）
        /// </summary>
        /// <param name="inFileName">待解密文件的路径</param>
        /// <param name="outFileName">待解密后文件的输出路径</param>
        public void AesDecrypt(string inFileName, string outFileName)
        {
            try
            {
                FileStream fin = new FileStream(inFileName, FileMode.Open, FileAccess.Read);
                FileStream fout = new FileStream(outFileName, FileMode.OpenOrCreate, FileAccess.Write);
                fout.SetLength(0);

                byte[] bin = new byte[100];
                long rdlen = 0;
                long totlen = fin.Length;
                int len;
                aes.Key = GetLegalKey();
                aes.IV = GetLegalIV();
                ICryptoTransform encrypto = aes.CreateDecryptor();
                CryptoStream cs = new CryptoStream(fout, encrypto, CryptoStreamMode.Write);
                while (rdlen < totlen)
                {
                    len = fin.Read(bin, 0, 100);
                    cs.Write(bin, 0, len);
                    rdlen = rdlen + len;
                }
                cs.Close();
                fout.Close();
                fin.Close();

            }
            catch (Exception ex)
            {
                return;
            }
        }
        #endregion
    }
}
