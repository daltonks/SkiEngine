using System;
using Lidgren.Network;

namespace SkiEngine.Networking
{
    public class NetMessageMetadata
    {
        public static NetMessageMetadata Create<TMessage>(
            int index,
            Func<TMessage, int?> estimateSizeBytesFunc,
            Action<TMessage, NetOutgoingMessage> serializeAction,
            Func<NetIncomingMessage, TMessage> deserializeFunc
        ) 
            => new NetMessageMetadata(
                index,
                message => estimateSizeBytesFunc.Invoke((TMessage) message),
                (message, outgoingMessage) => serializeAction.Invoke((TMessage) message, outgoingMessage),
                (incomingMessage) => deserializeFunc.Invoke(incomingMessage)
            );

        public event Action<object, NetIncomingMessage> Received;

        public int Index { get; }

        private readonly Func<object, int?> _estimateSizeBytesFunc;
        private readonly Action<object, NetOutgoingMessage> _serializeAction;
        private readonly Func<NetIncomingMessage, object> _deserializeFunc;
        
        private NetMessageMetadata(
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
        {
            return _estimateSizeBytesFunc.Invoke(message);
        }

        public void Serialize(object message, NetOutgoingMessage outgoingMessage)
        {
            _serializeAction.Invoke(message, outgoingMessage);
        }

        public object Deserialize(NetIncomingMessage incomingMessage)
        {
            return _deserializeFunc.Invoke(incomingMessage);
        }

        public void OnReceived(object message, NetIncomingMessage incomingMessage)
        {
            Received?.Invoke(message, incomingMessage);
        }
    }
}
