using System;
using System.Collections.Concurrent;
using System.Reflection;
using Google.Protobuf;
using Lidgren.Network;

namespace SkiEngine.Networking.Protobuf
{
    public class ProtobufNetMessage<TProtobufMessage> : INetMessage where TProtobufMessage : IMessage<TProtobufMessage>
    {
        public TProtobufMessage ProtobufMessage { get; set; }

        public int? EstimateSizeBytes() => ProtobufMessage.CalculateSize();

        public void WriteTo(NetOutgoingMessage message)
        {
            message.Write(ProtobufMessage);
        }

        public void ReadFrom(NetIncomingMessage message)
        {
            ProtobufMessage = message.Read<TProtobufMessage>();
        }
    }
}
