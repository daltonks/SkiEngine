using Lidgren.Network;

namespace SkiEngine.Networking.Messages
{
    public class RsaEncryptedAesKeyMessage : INetMessage
    {
        public byte[] RsaEncryptedAesKey { get; set; }

        public int? EstimateSizeBytes() => null;

        public void WriteTo(NetOutgoingMessage message)
        {
            message.WriteVariableInt32(RsaEncryptedAesKey.Length);
            message.Write(RsaEncryptedAesKey);
        }

        public void ReadFrom(NetIncomingMessage message)
        {
            var length = message.ReadVariableInt32();
            RsaEncryptedAesKey = message.ReadBytes(length);
        }
    }
}
