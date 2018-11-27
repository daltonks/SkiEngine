namespace Assets.CryptographyAssembly.Interfaces
{
    public interface IAesService
    {
        byte[] AesKey { get; set; }

        byte[] Decrypt(byte[] cipherTextCombined);
        byte[] Encrypt(byte[] data);
    }
}