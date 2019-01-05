using System;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace SkiEngine.Networking.Protobuf
{
    public static class SkiPeerExtensions
    {
        public static void RegisterProtobufMessageType<TMessage>(this SkiPeer skiPeer) where TMessage : IMessage<TMessage>
        {
            skiPeer.RegisterProtobufMessageType(typeof(TMessage));
        }

        public static void RegisterProtobufMessageType(this SkiPeer skiPeer, Type type)
        {
            skiPeer.RegisterMessageType(
                type,
                estimateSizeBytesFunc: message => ((IMessage) message).CalculateSize(),
                serializeAction: (message, outgoingMessage) => outgoingMessage.Write((IMessage) message),
                deserializeFunc: incomingMessage => incomingMessage.ReadProtobuf(type)
            );
        }

        public static void RegisterProtobufMessageTypes(this SkiPeer skiPeer, IEnumerable<MessageDescriptor> descriptors)
        {
            foreach (var descriptor in descriptors)
            {
                skiPeer.RegisterProtobufMessageType(descriptor.ClrType);
            }
        }
    }
}
