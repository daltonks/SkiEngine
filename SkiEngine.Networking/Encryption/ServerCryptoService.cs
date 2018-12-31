using System;
using System.Diagnostics;
using System.Security.Cryptography;

namespace SkiEngine.Networking.Encryption
{
    public class ServerCryptoService : IDisposable
    {
        private const int RsaKeySizeInBytes = 256;

        public string XmlRsaPublicKey { get; }
        private readonly RSACryptoServiceProvider _rsaServiceProvider;
        
        public ServerCryptoService()
        {
            _rsaServiceProvider = new RSACryptoServiceProvider(RsaKeySizeInBytes * 8);
            XmlRsaPublicKey = _rsaServiceProvider.ToXmlStringWorkaround(false);
        }

        public void ReceivedRsaEncryptedAesKey(byte[] rsaEncryptedAesKeyFromClient, Action<AesServiceAndEncryptedKey> sendAesEncryptedAesKeyToClient, Action onFail)
        {
            try
            {
                var aesKey = _rsaServiceProvider.Decrypt(rsaEncryptedAesKeyFromClient, false);

                var aesService = new AesService(aesKey);
                var keyAndEncrypted = new AesServiceAndEncryptedKey
                {
                    AesService = aesService,
                    AesEncryptedAesKey = aesService.Encrypt(aesKey, aesKey.Length)
                };
                sendAesEncryptedAesKeyToClient.Invoke(keyAndEncrypted);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                onFail?.Invoke();
            }
        }

        public void Dispose()
        {
            _rsaServiceProvider?.Dispose();
        }
    }
}