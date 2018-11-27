using Assets.CryptographyAssembly.Interfaces;

namespace SkiEngine.Networking.Encryption
{
    public class AesServiceAndEncryptedKey
    {
        public IAesService AesService { get; set; }
        public byte[] AesEncryptedAesKey { get; set; }
    }
}