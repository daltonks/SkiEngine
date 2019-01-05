﻿using System;
using Google.Protobuf;

namespace SkiEngine.Networking.Protobuf
{
    public static class SkiPeerExtensions
    {
        public static void RegisterProtobufMessageType<TMessage>(this SkiPeer skiPeer) where TMessage : IMessage<TMessage>
        {
            skiPeer.RegisterMessageType(
                estimateSizeBytesFunc: message => message.CalculateSize(),
                serializeAction: (message, outgoingMessage) => outgoingMessage.Write(message),
                deserializeFunc: incomingMessage => incomingMessage.Read<TMessage>()
            );
        }
    }
}
