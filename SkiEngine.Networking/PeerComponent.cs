using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Lidgren.Network;
using SkiEngine.NCS.Component.Base;

namespace SkiEngine.Networking
{
    public abstract class PeerComponent : Component
    {
        public delegate void StatusRespondedAwaitingApprovalDelegate(NetIncomingMessage im, string reason);
        public delegate void StatusNoneDelegate(NetIncomingMessage im, string reason);
        public delegate void StatusInitiatedConnectDelegate(NetIncomingMessage im, string reason);
        public delegate void StatusReceivedInitiationDelegate(NetIncomingMessage im, string reason);
        public delegate void StatusRespondedConnectDelegate(NetIncomingMessage im, string reason);
        public delegate void StatusConnectedDelegate(NetIncomingMessage im, string reason);
        public delegate void StatusDisconnectingDelegate(NetIncomingMessage im, string reason);
        public delegate void StatusDisconnectedDelegate(NetIncomingMessage im, string reason);

        public delegate void DebugMessageDelegate(string message);
        public delegate void VerboseDebugMessageDelegate(string message);
        public delegate void ErrorMessageDelegate(string message);
        public delegate void WarningMessageDelegate(string message);
        
        protected const string IncorrectPasswordReason = "INCORRECT_PASSWORD";

        public event StatusRespondedAwaitingApprovalDelegate StatusRespondedAwaitingApproval;
        public event StatusNoneDelegate StatusNone;
        public event StatusInitiatedConnectDelegate StatusInitiatedConnect;
        public event StatusReceivedInitiationDelegate StatusReceivedInitiation;
        public event StatusRespondedConnectDelegate StatusRespondedConnect;
        public event StatusConnectedDelegate StatusConnected;
        public event StatusDisconnectingDelegate StatusDisconnecting;
        public event StatusDisconnectedDelegate StatusDisconnected;

        public event DebugMessageDelegate DebugMessage;
        public event VerboseDebugMessageDelegate VerboseDebugMessage;
        public event ErrorMessageDelegate ErrorMessage;
        public event WarningMessageDelegate WarningMessage;

        protected string Password { get; }

        protected readonly Dictionary<Type, PacketMetadata> TypeToPacketMetadata = new Dictionary<Type, PacketMetadata>();
        protected readonly Dictionary<int, PacketMetadata> IndexToPacketMetadata = new Dictionary<int, PacketMetadata>();

        protected PeerComponent(string password = "")
        {
            Password = password ?? "";
        }

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

        protected abstract bool AllowJoin(NetIncomingMessage im);

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
                    var packetIndex = im.ReadInt32();
                    if (IndexToPacketMetadata.TryGetValue(packetIndex, out var metadata))
                    {
                        metadata.Receive(im);
                    }
                    break;

                // Connection approval
                case NetIncomingMessageType.ConnectionApproval:
                    var password = im.ReadString() ?? "";
                    if (password == Password && AllowJoin(im))
                    {
                        im.SenderConnection.Approve();
                    }
                    else
                    {
                        im.SenderConnection.Deny(IncorrectPasswordReason);
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
    }

    public abstract class PeerComponent<T> : PeerComponent where T : NetPeer
    {
        public event Action Started;

        protected T LidgrenPeer { get; }
        protected readonly ConcurrentQueue<NetIncomingMessage> IncomingMessages = new ConcurrentQueue<NetIncomingMessage>();

        private volatile bool _running;
        private Thread _receiveMessageThread;
        
        protected PeerComponent(T lidgrenPeer, string password = "") : base(password)
        {
            LidgrenPeer = lidgrenPeer;

            Destroyed += OnDestroyed;
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

        protected bool TryCreateOutgoingMessage(IPacket packet, out NetOutgoingMessage message)
        {
            if (!TypeToPacketMetadata.TryGetValue(packet.GetType(), out var metadata))
            {
                Debug.Fail($"{packet.GetType()} not registered!");
                message = null;
                return false;
            }

            message = LidgrenPeer.CreateMessage();
            message.Write(metadata.Index);
            packet.WriteTo(message);
            return true;
        }

        public void FlushSendQueue()
        {
            LidgrenPeer.FlushSendQueue();
        }

        private void OnDestroyed(IComponent component)
        {
            _running = false;
            _receiveMessageThread?.Join();
            LidgrenPeer.Shutdown("Disposed");
        }
    }
}
