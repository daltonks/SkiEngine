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

        public static T ReadProtobufDelimited<T>(this NetIncomingMessage netMessage) where T : IMessage<T>
        {
            return (T) netMessage.ReadProtobufDelimited(typeof(T));
        }

        public static object ReadProtobufDelimited(this NetIncomingMessage netMessage, Type type)
        {
            var sizeInBytes = netMessage.ReadVariableInt32();
            var result = GetParser(type).ParseFrom(netMessage.Data, netMessage.PositionInBytes, sizeInBytes);
            netMessage.Position += sizeInBytes * 8;
            return result;
        }
        
        public static T ReadProtobuf<T>(this NetIncomingMessage netMessage) where T : IMessage<T>
        {
            return (T) netMessage.ReadProtobuf(typeof(T));
        }

        public static object ReadProtobuf(this NetIncomingMessage netMessage, Type type)
        {
            return GetParser(type).ParseFrom(netMessage.Data, netMessage.PositionInBytes, netMessage.Data.Length - netMessage.PositionInBytes);
        }

        private static readonly ConcurrentDictionary<Type, object> _typeToParserMap = new ConcurrentDictionary<Type, object>();
        private static MessageParser GetParser(Type type)
        {
            if(!_typeToParserMap.TryGetValue(type, out var parser))
            {
                _typeToParserMap[type] 
                    = parser 
                    = type.GetProperty("Parser", BindingFlags.Static | BindingFlags.Public).GetValue(null);
            }

            return (MessageParser) parser;
        }
    }
}
