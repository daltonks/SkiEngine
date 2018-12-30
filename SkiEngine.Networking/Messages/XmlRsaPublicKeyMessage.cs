using Lidgren.Network;

namespace SkiEngine.Networking.Messages
{
    public class XmlRsaPublicKeyMessage : INetMessage
    {
        public string XmlRsaPublicKey { get; set; }

        public int? EstimateSizeBytes() => null;

        public void WriteTo(NetOutgoingMessage message)
        {
            message.Write(XmlRsaPublicKey);
        }

        public void ReadFrom(NetIncomingMessage message)
        {
            XmlRsaPublicKey = message.ReadString();
        }
    }
}
