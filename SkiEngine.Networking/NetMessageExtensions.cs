using Google.Protobuf;
using Lidgren.Network;
using SkiaSharp;

namespace SkiEngine.Networking
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

        public static void Write(this NetOutgoingMessage netMessage, SKPoint point)
        {
            netMessage.Write(point.X);
            netMessage.Write(point.Y);
        }

        public static T ReadDelimited<T>(this NetIncomingMessage netMessage, MessageParser<T> parser) where T : IMessage<T>
        {
            var sizeInBytes = netMessage.ReadVariableInt32();
            var result = parser.ParseFrom(netMessage.Data, netMessage.PositionInBytes, sizeInBytes);
            netMessage.Position += sizeInBytes * 8;
            return result;
        }
        
        public static T Read<T>(this NetIncomingMessage netMessage, MessageParser<T> parser) where T : IMessage<T>
        {
            return parser.ParseFrom(netMessage.Data, netMessage.PositionInBytes, netMessage.Data.Length - netMessage.PositionInBytes);
        }
        
        public static SKPoint ReadPoint(this NetIncomingMessage netMessage)
        {
            return new SKPoint(netMessage.ReadFloat(), netMessage.ReadFloat());
        }
    }
}
