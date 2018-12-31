using System;
using System.Reflection;
using Lidgren.Network;

namespace SkiEngine.Networking
{
    public class NetMessageMetadata
    {
        private static readonly object[] EmptyObjectArray = new object[0];

        public int Index { get; }
        public event Action<object, NetIncomingMessage> Received;

        private readonly ConstructorInfo _emptyConstructorInfo;

        public NetMessageMetadata(Type messageType, int index)
        {
            Index = index;
            _emptyConstructorInfo = messageType.GetConstructor(Type.EmptyTypes);
        }

        public INetMessage ToNetMessage(NetIncomingMessage incomingMessage)
        {
            var netMessage = (INetMessage)_emptyConstructorInfo.Invoke(EmptyObjectArray);
            netMessage.ReadFrom(incomingMessage);
            return netMessage;
        }

        public void OnReceived(INetMessage netMessage, NetIncomingMessage incomingMessage)
        {
            Received?.Invoke(netMessage, incomingMessage);
        }
    }
}
