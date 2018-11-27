using System;
using System.IO;
using System.Security.Cryptography;

namespace SkiEngine.Networking.Encryption
{
    public class AesService
    {
        private const int KeySizeInBytes = 16;

        public byte[] AesKey { get; set; }

        protected AesService()
        {
            AesKey = new byte[KeySizeInBytes];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(AesKey);
            }
        }

        public AesService(byte[] key)
        {
            AesKey = key;
        }

        protected bool CompareAesKey(byte[] otherAesKey)
        {
            if (otherAesKey == null || otherAesKey.Length != AesKey.Length)
            {
                return false;
            }

            for (var i = 0; i < AesKey.Length; i++)
            {
                if (AesKey[i] != otherAesKey[i])
                {
                    return false;
                }
            }

            return true;
        }

        public byte[] Encrypt(byte[] data)
        {
            byte[] encrypted;
            byte[] initializationVector;

            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = AesKey;

                aesAlg.GenerateIV();
                initializationVector = aesAlg.IV;

                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Mode = CipherMode.CBC;

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();
                    }
                    encrypted = memoryStream.ToArray();
                }
            }

            var combinedIvCt = new byte[initializationVector.Length + encrypted.Length];
            Array.Copy(initializationVector, 0, combinedIvCt, 0, initializationVector.Length);
            Array.Copy(encrypted, 0, combinedIvCt, initializationVector.Length, encrypted.Length);

            return combinedIvCt;
        }

        public byte[] Decrypt(byte[] cipherTextCombined)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = AesKey;

                var initializationVector = new byte[aesAlg.BlockSize / 8];
                var cipherData = new byte[cipherTextCombined.Length - initializationVector.Length];

                Array.Copy(cipherTextCombined, initializationVector, initializationVector.Length);
                Array.Copy(cipherTextCombined, initializationVector.Length, cipherData, 0, cipherData.Length);

                aesAlg.IV = initializationVector;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Mode = CipherMode.CBC;

                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var cipherStream = new MemoryStream(cipherData))
                using (var cryptoStream = new CryptoStream(cipherStream, decryptor, CryptoStreamMode.Read))
                using (var outputMemoryStream = new MemoryStream())
                {
                    cryptoStream.CopyTo(outputMemoryStream);
                    return outputMemoryStream.ToArray();
                }
            }
        }
    }
}
