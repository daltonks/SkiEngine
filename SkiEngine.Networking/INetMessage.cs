using Lidgren.Network;

namespace SkiEngine.Networking
{
    public interface INetMessage
    {
        int? EstimateSizeBytes();
        void WriteTo(NetOutgoingMessage message);
        void ReadFrom(NetIncomingMessage message);
    }
}
