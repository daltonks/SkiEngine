using System;
using System.Collections.Concurrent;
using System.Reflection;
using Google.Protobuf;
using Lidgren.Network;

namespace SkiEngine.Networking.Protobuf
{
    public static class NetMessageExtensions
    {
        public static void WriteDelimited(this NetOutgoingMessage netMessage, IMessage protobufModel)
        {
            var bytes = protobufModel.ToByteArray();
            netMessage.WriteVariableInt32(bytes.Length);
            netMessage.Write(bytes);
        }

        public static void Write(this NetOutgoingMessage netMessage, IMessage protobufModel)
        {
            netMessage.Write(protobufModel.ToByteArray());
        }

        public static T ReadDelimited<T>(this NetIncomingMessage netMessage) where T : IMessage<T>
        {
            var sizeInBytes = netMessage.ReadVariableInt32();
            var result = GetParser<T>().ParseFrom(netMessage.Data, netMessage.PositionInBytes, sizeInBytes);
            netMessage.Position += sizeInBytes * 8;
            return result;
        }
        
        public static T Read<T>(this NetIncomingMessage netMessage) where T : IMessage<T>
        {
            return GetParser<T>().ParseFrom(netMessage.Data, netMessage.PositionInBytes, netMessage.Data.Length - netMessage.PositionInBytes);
        }

        private static readonly ConcurrentDictionary<Type, object> _typeToParserMap = new ConcurrentDictionary<Type, object>();
        private static MessageParser<T> GetParser<T>() where T : IMessage<T>
        {
            if(!_typeToParserMap.TryGetValue(typeof(T), out var parser))
            {
                _typeToParserMap[typeof(T)] 
                    = parser 
                    = typeof(T).GetProperty("Parser", BindingFlags.Static | BindingFlags.Public).GetValue(null);
            }

            return (MessageParser<T>) parser;
        }
    }
}
