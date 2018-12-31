using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Lidgren.Network;
using SkiEngine.Networking.Messages;

namespace SkiEngine.Networking
{
    public abstract class SkiPeer : IDisposable
    {
        public delegate void StatusDelegate(NetIncomingMessage im, string reason);
        public delegate void LogMessageDelegate(string message);
        public delegate void DataReceivedDelegate(NetIncomingMessage incomingMessage);
        
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

        public event Action<Exception> ExceptionProcessingMessage;
        
        protected NetPeer LidgrenPeer { get; }

        private readonly Dictionary<Type, NetMessageMetadata> _typeToMessageMetadata = new Dictionary<Type, NetMessageMetadata>();
        private readonly Dictionary<int, NetMessageMetadata> _indexToMessageMetadata = new Dictionary<int, NetMessageMetadata>();

        private readonly SynchronizationContext _receiveMessageContext;
        private bool _disposed;

        protected SkiPeer(NetPeer lidgrenPeer, SynchronizationContext receiveMessageContext = null)
        {
            LidgrenPeer = lidgrenPeer;
            _receiveMessageContext = receiveMessageContext;

            RegisterMessageType<AesEncryptedAesKeyMessage>();
            RegisterMessageType<RequestXmlRsaPublicKeyMessage>();
            RegisterMessageType<RsaEncryptedAesKeyMessage>();
            RegisterMessageType<XmlRsaPublicKeyMessage>();

            ExceptionProcessingMessage += exception =>
            {
                Debug.WriteLine(exception);
            };
        }

        protected abstract bool AllowConnection(NetIncomingMessage incomingMessage);
        protected abstract bool AllowHandling(NetIncomingMessage incomingMessage, INetMessage netMessage);
        protected abstract bool CanDecrypt(NetIncomingMessage incomingMessage);
        protected abstract byte[] Decrypt(NetIncomingMessage incomingMessage);

        public void RegisterMessageType<TMessage>() where TMessage : INetMessage
        {
            var type = typeof(TMessage);
            var index = _typeToMessageMetadata.Count;
            _typeToMessageMetadata[type] = _indexToMessageMetadata[index] = new NetMessageMetadata(typeof(TMessage), index);
        }

        public void RegisterReceiveHandler<TMessage>(Action<TMessage, NetIncomingMessage> onReceivedAction) where TMessage : INetMessage
        {
            var messageMetadata = _typeToMessageMetadata[typeof(TMessage)];
            messageMetadata.Received += (obj, incomingMessage) => onReceivedAction.Invoke((TMessage)obj, incomingMessage);
        }

        protected void StartInternal()
        {
            if (_receiveMessageContext == null)
            {
                // _receiveMessageContext is null, so receive messages on a different thread
                LidgrenPeer.Start();

                var thread = new Thread(() =>
                {
                    while (!_disposed)
                    {
                        while (LidgrenPeer.ReadMessage(out var message))
                        {
                            ProcessMessage(message);
                        }

                        Thread.Sleep(1);
                    }
                });
                thread.Start();
            }
            else
            {
                // _receiveMessageContext is not null, so process messages with it
                LidgrenPeer.RegisterReceivedCallback(
                    _ => ProcessMessage(LidgrenPeer.ReadMessage()),
                    _receiveMessageContext
                );

                LidgrenPeer.Start();
            }
        }

        protected NetOutgoingMessage CreateOutgoingMessage(INetMessage netMessage)
        {
            if (_typeToMessageMetadata.TryGetValue(netMessage.GetType(), out var metadata))
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

        private void ProcessMessage(NetIncomingMessage im)
        {
            try
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
                        var status = (NetConnectionStatus) im.ReadByte();

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
                        if (CanDecrypt(im))
                        {
                            var decryptedBytes = Decrypt(im);
                            LidgrenPeer.Recycle(im.Data);
                            im.Data = decryptedBytes;
                            im.LengthBytes = decryptedBytes.Length;
                        }

                        var messageIndex = im.ReadVariableInt32();
                        if (_indexToMessageMetadata.TryGetValue(messageIndex, out var metadata))
                        {
                            var netMessage = metadata.ToNetMessage(im);

                            if (AllowHandling(im, netMessage))
                            {
                                metadata.OnReceived(netMessage, im);
                            }
                        }

                        break;

                    // Connection approval
                    case NetIncomingMessageType.ConnectionApproval:
                        if (AllowConnection(im))
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
                        Debug.WriteLine("Unhandled net message type: " + im.MessageType + " " + im.LengthBytes +
                                        " bytes " + im.DeliveryMethod + "|" + im.SequenceChannel);
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionProcessingMessage?.Invoke(ex);
            }
            finally
            {
                LidgrenPeer.Recycle(im);
            }
        }

        public void Dispose()
        {
            _disposed = true;
            LidgrenPeer.Shutdown("Disposed");
        }
    }
}
