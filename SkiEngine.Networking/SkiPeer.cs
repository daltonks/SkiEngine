using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Lidgren.Network;

namespace SkiEngine.Networking
{
    public abstract class SkiPeer
    {
        public delegate void StatusDelegate(NetIncomingMessage im, string reason);
        public delegate void LogMessageDelegate(string message);
        
        public event StatusDelegate StatusRespondedAwaitingApproval;
        public event StatusDelegate StatusNone;
        public event StatusDelegate StatusInitiatedConnect;
        public event StatusDelegate StatusReceivedInitiation;
        public event StatusDelegate StatusRespondedConnect;
        public event StatusDelegate StatusConnected;
        public event StatusDelegate StatusDisconnecting;
        public event StatusDelegate StatusDisconnected;

        public event LogMessageDelegate DebugMessage;
        public event LogMessageDelegate VerboseDebugMessage;
        public event LogMessageDelegate ErrorMessage;
        public event LogMessageDelegate WarningMessage;

        protected readonly Dictionary<Type, PacketMetadata> TypeToPacketMetadata = new Dictionary<Type, PacketMetadata>();
        protected readonly Dictionary<int, PacketMetadata> IndexToPacketMetadata = new Dictionary<int, PacketMetadata>();

        public void RegisterPacketType<TPacket>() where TPacket : IPacket
        {
            var type = typeof(TPacket);
            var index = TypeToPacketMetadata.Count;
            TypeToPacketMetadata[type] = IndexToPacketMetadata[index] = new PacketMetadata(typeof(TPacket), index);
        }

        public void RegisterReceiveHandler<TPacket>(Action<TPacket, NetConnection> onReceivedAction) where TPacket : IPacket
        {
            var packetMetadata = TypeToPacketMetadata[typeof(TPacket)];
            packetMetadata.Received += (obj, connection) => onReceivedAction.Invoke((TPacket)obj, connection);
        }

        protected abstract bool AllowJoin(IPacket hailPacket = null);

        protected void ProcessMessage(NetIncomingMessage im)
        {
            switch (im.MessageType)
            {
                // Logs
                case NetIncomingMessageType.DebugMessage:
                    DebugMessage?.Invoke(im.ReadString());
                    break;
                case NetIncomingMessageType.ErrorMessage:
                    ErrorMessage?.Invoke(im.ReadString());
                    break;
                case NetIncomingMessageType.WarningMessage:
                    WarningMessage?.Invoke(im.ReadString());
                    break;
                case NetIncomingMessageType.VerboseDebugMessage:
                    VerboseDebugMessage?.Invoke(im.ReadString());
                    break;

                // StatusChanged
                case NetIncomingMessageType.StatusChanged:
                    var status = (NetConnectionStatus)im.ReadByte();

                    var reason = im.ReadString();
                    switch (status)
                    {
                        case NetConnectionStatus.None:
                            StatusNone?.Invoke(im, reason);
                            break;
                        case NetConnectionStatus.InitiatedConnect:
                            StatusInitiatedConnect?.Invoke(im, reason);
                            break;
                        case NetConnectionStatus.ReceivedInitiation:
                            StatusReceivedInitiation?.Invoke(im, reason);
                            break;
                        case NetConnectionStatus.RespondedAwaitingApproval:
                            StatusRespondedAwaitingApproval?.Invoke(im, reason);
                            break;
                        case NetConnectionStatus.RespondedConnect:
                            StatusRespondedConnect?.Invoke(im, reason);
                            break;
                        case NetConnectionStatus.Connected:
                            StatusConnected?.Invoke(im, reason);
                            break;
                        case NetConnectionStatus.Disconnecting:
                            StatusDisconnecting?.Invoke(im, reason);
                            break;
                        case NetConnectionStatus.Disconnected:
                            StatusDisconnected?.Invoke(im, reason);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                // Received data
                case NetIncomingMessageType.Data:
                    ReadPacket(im);
                    break;

                // Connection approval
                case NetIncomingMessageType.ConnectionApproval:
                    var packet = ReadPacket(im);

                    if (AllowJoin(packet))
                    {
                        im.SenderConnection.Approve();
                    }
                    else
                    {
                        im.SenderConnection.Deny();
                    }
                    break;

                // Unhandled message types
                case NetIncomingMessageType.Error:
                case NetIncomingMessageType.UnconnectedData:
                case NetIncomingMessageType.Receipt:
                case NetIncomingMessageType.DiscoveryRequest:
                case NetIncomingMessageType.DiscoveryResponse:
                case NetIncomingMessageType.NatIntroductionSuccess:
                case NetIncomingMessageType.ConnectionLatencyUpdated:
                default:
                    Debug.WriteLine("Unhandled net message type: " + im.MessageType + " " + im.LengthBytes + " bytes " + im.DeliveryMethod + "|" + im.SequenceChannel);
                    break;
            }
        }

        private IPacket ReadPacket(NetIncomingMessage incomingMessage)
        {
            var packetIndex = incomingMessage.ReadVariableInt32();
            if (IndexToPacketMetadata.TryGetValue(packetIndex, out var metadata))
            {
                return metadata.Receive(incomingMessage);
            }

            return null;
        }
    }

    public abstract class SkiPeer<TNetPeer> : SkiPeer, IDisposable where TNetPeer : NetPeer
    {
        public event Action Started;

        protected TNetPeer LidgrenPeer { get; }
        protected readonly ConcurrentQueue<NetIncomingMessage> IncomingMessages = new ConcurrentQueue<NetIncomingMessage>();

        private volatile bool _running;
        private Thread _receiveMessageThread;
        
        protected SkiPeer(TNetPeer lidgrenPeer)
        {
            LidgrenPeer = lidgrenPeer;
        }

        public void StartReadMessagesConcurrently()
        {
            if (_receiveMessageThread != null)
            {
                throw new InvalidOperationException("A thread is already running!");
            }

            _receiveMessageThread = new Thread(StartReadMessagesAndBlock);
            _receiveMessageThread.Start();

            Started?.Invoke();
        }

        private void StartReadMessagesAndBlock()
        {
            LidgrenPeer.Start();
            _running = true;

            while (_running)
            {
                NetIncomingMessage im;
                while ((im = LidgrenPeer.ReadMessage()) != null)
                {
                    IncomingMessages.Enqueue(im);
                }
                Thread.Sleep(1);
            }
        }

        protected NetOutgoingMessage CreateOutgoingMessage(IPacket packet)
        {
            if (TypeToPacketMetadata.TryGetValue(packet.GetType(), out var metadata))
            {
                var message = LidgrenPeer.CreateMessage();
                message.WriteVariableInt32(metadata.Index);
                packet.WriteTo(message);
                return message;
            }

            throw new ArgumentException($"{packet.GetType()} not registered!");
        }

        public void FlushSendQueue()
        {
            LidgrenPeer.FlushSendQueue();
        }

        public void Dispose()
        {
            _running = false;
            _receiveMessageThread?.Join();
            LidgrenPeer.Shutdown("Disposed");
        }
    }
}
