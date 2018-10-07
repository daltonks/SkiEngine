using Lidgren.Network;

namespace SkiEngine.Networking
{
    public interface IPacket
    {
        void WriteTo(NetOutgoingMessage message);
        void ReadFrom(NetIncomingMessage message);
    }
}
