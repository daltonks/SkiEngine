using System;
using Lidgren.Network;

namespace SkiEngine.Networking.Messages
{
    public class RequestXmlRsaPublicKeyMessage : INetMessage
    {
        public int? EstimateSizeBytes() => null;

        public void WriteTo(NetOutgoingMessage message)
        {
            
        }

        public void ReadFrom(NetIncomingMessage message)
        {
            
        }
    }
}
