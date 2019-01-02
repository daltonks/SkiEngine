using System;
using Google.Protobuf;

namespace SkiEngine.Networking.Protobuf
{
    public static class SkiPeerExtensions
    {
        public static void RegisterMessageType<TMessage>(this SkiPeer skiPeer) where TMessage : IMessage<TMessage>
        {
            skiPeer.RegisterMessageType<TMessage>(
                estimateSizeBytesFunc: message => ((TMessage) message).CalculateSize(),
                serializeAction: (message, outgoingMessage) => outgoingMessage.Write((TMessage) message),
                deserializeFunc: incomingMessage => incomingMessage.Read<TMessage>()
            );
        }
    }
}
