using System;
using Lidgren.Network;

namespace SkiEngine.Networking
{
    public class NetMessageMetadata
    {
        public event Action<object, NetIncomingMessage> Received;

        public int Index { get; }

        private readonly Func<object, int?> _estimateSizeBytesFunc;
        private readonly Action<object, NetOutgoingMessage> _serializeAction;
        private readonly Func<NetIncomingMessage, object> _deserializeFunc;
        
        public NetMessageMetadata(
            int index,
            Func<object, int?> estimateSizeBytesFunc,
            Action<object, NetOutgoingMessage> serializeAction,
            Func<NetIncomingMessage, object> deserializeFunc
        )
        {
            Index = index;
            _estimateSizeBytesFunc = estimateSizeBytesFunc;
            _serializeAction = serializeAction;
            _deserializeFunc = deserializeFunc;
        }

        public int? EstimateSizeBytes(object message) 
            => _estimateSizeBytesFunc.Invoke(message);

        public void Serialize(object message, NetOutgoingMessage outgoingMessage)
            => _serializeAction.Invoke(message, outgoingMessage);

        public object Deserialize(NetIncomingMessage incomingMessage) 
            => _deserializeFunc.Invoke(incomingMessage);

        public void OnReceived(object message, NetIncomingMessage incomingMessage)
        {
            Received?.Invoke(message, incomingMessage);
        }
    }
}
