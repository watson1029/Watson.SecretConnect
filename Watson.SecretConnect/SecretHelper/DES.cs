using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Watson.SecretConnect.SecretHelper
{
    internal class DES
    {
        private string Key;
        private string IV;
        private DESCryptoServiceProvider des;

        #region DES算法构造函数
        /// <summary>
        /// DES算法构造函数（KEY和IV随机生成）
        /// </summary>
        public DES()
        {
            des = new DESCryptoServiceProvider();
            des.GenerateKey();
            des.GenerateIV();
            Key = Convert.ToBase64String(des.Key);
            IV = Convert.ToBase64String(des.IV);
        }

        /// <summary>
        /// DES算法构造函数（KEY自定义，IV随机生成）
        /// </summary>
        /// <param name="sKey"></param>
        public DES(string sKey)
        {
            des = new DESCryptoServiceProvider();
            des.GenerateIV();
            Key = sKey;
            IV = Convert.ToBase64String(des.IV);
        }

        /// <summary>
        /// DES算法构造函数（KEY和IV自定义）
        /// </summary>
        /// <param name="sKey"></param>
        /// <param name="sIV"></param>
        public DES(string sKey, string sIV)
        {
            des = new DESCryptoServiceProvider();
            Key = sKey;
            IV = sIV;
        }
        #endregion

        #region Get方法
        /// <summary>
        /// 获取Key
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

        #region 获取Key和IV的私有方法
        /// <summary>
        /// 获得密钥
        /// </summary>
        /// <returns>密钥</returns>
        private byte[] GetLegalKey()
        {
            string sTemp = Key;
            des.GenerateKey();
            byte[] bytTemp = des.Key;
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
            des.GenerateIV();
            byte[] bytTemp = des.IV;
            int IVLength = bytTemp.Length;
            if (sTemp.Length > IVLength)
                sTemp = sTemp.Substring(0, IVLength);
            else if (sTemp.Length < IVLength)
                sTemp = sTemp.PadRight(IVLength, ' ');
            return ASCIIEncoding.ASCII.GetBytes(sTemp);
        }
        #endregion

        #region 加密与解密
        ///<summary>
        /// DES加密方法（失败返回NULL）
        ///</summary>
        ///<param name="strText">明文</param>
        ///<returns>密文</returns>
        public string DesEncrypt(string strText)
        {
            try
            {
                byte[] bytIn = UTF8Encoding.UTF8.GetBytes(strText);
                MemoryStream ms = new MemoryStream();
                des.Key = GetLegalKey();
                des.IV = GetLegalIV();
                ICryptoTransform encrypto = des.CreateEncryptor();
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

        ///<summary>
        /// DES解密方法（失败返回NULL）
        ///</summary>
        ///<param name="strText">密文</param>
        ///<returns>明文</returns>
        public string DesDecrypt(string strText)
        {
            try
            {
                byte[] bytIn = Convert.FromBase64String(strText);
                MemoryStream ms = new MemoryStream(bytIn, 0, bytIn.Length);
                des.Key = GetLegalKey();
                des.IV = GetLegalIV();
                ICryptoTransform encrypto = des.CreateDecryptor();
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
    }
}
