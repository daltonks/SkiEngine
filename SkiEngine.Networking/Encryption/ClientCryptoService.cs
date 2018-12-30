using System;
using System.Diagnostics;
using System.Security.Cryptography;
using NETCore.Encrypt.Extensions.Internal;

namespace SkiEngine.Networking.Encryption
{
    public class ClientCryptoService : AesService
    {
        public ClientCryptoService(string xmlRsaPublicKey, Action<byte[]> sendRsaEncryptedAesKeyToServerAction)
        {
            using (var rsaService = new RSACryptoServiceProvider())
            {
                rsaService.FromXmlStringWorkaround(xmlRsaPublicKey);
                var rsaEncryptedAesKey = rsaService.Encrypt(AesKey, false);
                sendRsaEncryptedAesKeyToServerAction.Invoke(rsaEncryptedAesKey);
            }
        }

        public void ReceivedAesEncryptedAesKey(byte[] aesEncryptedAesKey, Action onSuccess, Action onFail)
        {
            try
            {
                var receivedAesKey = Decrypt(aesEncryptedAesKey);
                if (CompareAesKey(receivedAesKey))
                {
                    onSuccess?.Invoke();
                }
                else
                {
                    onFail?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                onFail?.Invoke();
            }
        }
    }
}