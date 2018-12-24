using System;
using System.Reflection;
using Lidgren.Network;
using Type = System.Type;

namespace SkiEngine.Networking
{
    public class PacketMetadata
    {
        private static readonly object[] EmptyObjectArray = new object[0];

        public int Index { get; }
        public event Action<object, NetConnection> Received;

        private readonly ConstructorInfo _emptyConstructorInfo;

        public PacketMetadata(Type packetType, int index)
        {
            Index = index;
            _emptyConstructorInfo = packetType.GetConstructor(Type.EmptyTypes);
        }

        public IPacket Receive(NetIncomingMessage message)
        {
            var packetObject = (IPacket)_emptyConstructorInfo.Invoke(EmptyObjectArray);
            packetObject.ReadFrom(message);
            Received?.Invoke(packetObject, message.SenderConnection);
            return packetObject;
        }
    }
}
