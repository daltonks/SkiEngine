using Lidgren.Network;

namespace SkiEngine.Networking.Messages
{
    public class AesEncryptedAesKeyMessage : INetMessage
    {
        public byte[] AesEncryptedAesKey { get; set; }

        public int? EstimateSizeBytes() => null;

        public void WriteTo(NetOutgoingMessage message)
        {
            message.WriteVariableInt32(AesEncryptedAesKey.Length);
            message.Write(AesEncryptedAesKey);
        }

        public void ReadFrom(NetIncomingMessage message)
        {
            var length = message.ReadVariableInt32();
            AesEncryptedAesKey = message.ReadBytes(length);
        }
    }
}
