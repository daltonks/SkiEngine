using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Lidgren.Network;

namespace SkiEngine.Networking
{
    public abstract class SkiPeer : IDisposable
    {
        public delegate void StatusDelegate(NetIncomingMessage im, string reason);
        public delegate void LogMessageDelegate(string message);
        public delegate void DataReceivedDelegate(NetIncomingMessage incomingMessage);
        
        public event Action Started;

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

        protected readonly Dictionary<Type, NetMessageMetadata> TypeToMessageMetadata = new Dictionary<Type, NetMessageMetadata>();
        protected readonly Dictionary<int, NetMessageMetadata> IndexToMessageMetadata = new Dictionary<int, NetMessageMetadata>();
        
        protected NetPeer LidgrenPeer { get; }
        protected readonly ConcurrentQueue<NetIncomingMessage> IncomingMessages = new ConcurrentQueue<NetIncomingMessage>();

        private volatile bool _running;
        private Thread _receiveMessageThread;

        protected SkiPeer(NetPeer lidgrenPeer)
        {
            LidgrenPeer = lidgrenPeer;
        }

        public void RegisterMessageType<TMessage>() where TMessage : INetMessage
        {
            var type = typeof(TMessage);
            var index = TypeToMessageMetadata.Count;
            TypeToMessageMetadata[type] = IndexToMessageMetadata[index] = new NetMessageMetadata(typeof(TMessage), index);
        }

        public void RegisterReceiveHandler<TMessage>(Action<TMessage, NetConnection> onReceivedAction) where TMessage : INetMessage
        {
            var messageMetadata = TypeToMessageMetadata[typeof(TMessage)];
            messageMetadata.Received += (obj, connection) => onReceivedAction.Invoke((TMessage)obj, connection);
        }

        protected abstract bool AllowConnection(NetIncomingMessage incomingMessage, INetMessage message);

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

        protected NetOutgoingMessage CreateOutgoingMessage(INetMessage netMessage)
        {
            if (TypeToMessageMetadata.TryGetValue(netMessage.GetType(), out var metadata))
            {
                var estimatedSizeBytes = netMessage.EstimateSizeBytes();
                var outgoingMessage = estimatedSizeBytes == null 
                    ? LidgrenPeer.CreateMessage() 
                    : LidgrenPeer.CreateMessage(estimatedSizeBytes.Value + 4);
                outgoingMessage.WriteVariableInt32(metadata.Index);
                netMessage.WriteTo(outgoingMessage);
                return outgoingMessage;
            }

            throw new ArgumentException($"{netMessage.GetType()} not registered!");
        }

        public void FlushSendQueue()
        {
            LidgrenPeer.FlushSendQueue();
        }

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
                    ReadMessage(im);
                    break;

                // Connection approval
                case NetIncomingMessageType.ConnectionApproval:
                    var message = ReadMessage(im);
                    if (AllowConnection(im, message))
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

            INetMessage ReadMessage(NetIncomingMessage incomingMessage)
            {
                var messageIndex = incomingMessage.ReadVariableInt32();
                return IndexToMessageMetadata.TryGetValue(messageIndex, out var metadata) 
                    ? metadata.Receive(incomingMessage) 
                    : null;
            }
        }

        public void Dispose()
        {
            _running = false;
            _receiveMessageThread?.Join();
            LidgrenPeer.Shutdown("Disposed");
        }
    }
}
