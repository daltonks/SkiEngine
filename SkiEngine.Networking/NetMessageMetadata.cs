using System;
using Lidgren.Network;

namespace SkiEngine.Networking
{
    public class NetMessageMetadata
    {
        public event Action<object, NetIncomingMessage> Received;

        public int Index { get; }

        private readonly Func<NetIncomingMessage, object> _deserializeFunc;

        public NetMessageMetadata(int index, Func<NetIncomingMessage, object> deserializeFunc)
        {
            Index = index;
            _deserializeFunc = deserializeFunc;
        }

        public object Deserialize(NetIncomingMessage incomingMessage) 
            => _deserializeFunc.Invoke(incomingMessage);

        public void OnReceived(object message, NetIncomingMessage incomingMessage)
        {
            Received?.Invoke(message, incomingMessage);
        }
    }
}
