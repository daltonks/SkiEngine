using System;
using System.Threading;
using System.Threading.Tasks;
using Lidgren.Network;
using SkiEngine.Networking.Encryption;
using SkiEngine.Networking.Messages;

namespace SkiEngine.Networking
{
    public abstract class SkiClient : SkiPeer
    {
        public event Action HandshakeCompleted;

        public NetClient LidgrenClient => (NetClient) LidgrenPeer;

        private bool _handshakeCompleted;
        private ClientCryptoService _clientCryptoService;
        private Type _nextExpectedMessageType = typeof(XmlRsaPublicKeyMessage);
        private readonly TaskCompletionSource<bool> _handshakeCompletedCompletionSource = new TaskCompletionSource<bool>();

        protected SkiClient(NetPeerConfiguration config, SynchronizationContext receiveMessageContext = null) 
            : base(new NetClient(config), receiveMessageContext)
        {
            StatusConnected += OnConnected;
            StatusDisconnected += OnDisconnected;

            RegisterReceiveHandler<XmlRsaPublicKeyMessage>(
                (message, netMessage) =>
                {
                    _clientCryptoService = new ClientCryptoService(
                        message.XmlRsaPublicKey,
                        rsaEncryptedAesKey =>
                        {
                            Send(
                                new RsaEncryptedAesKeyMessage { RsaEncryptedAesKey = rsaEncryptedAesKey }, 
                                NetDeliveryMethod.ReliableOrdered, 
                                0
                            );
                        }
                    );

                    _nextExpectedMessageType = typeof(AesEncryptedAesKeyMessage);
                }
            );

            RegisterReceiveHandler<AesEncryptedAesKeyMessage>(
                (message, netMessage) =>
                {
                    _clientCryptoService.ReceivedAesEncryptedAesKey(
                        message.AesEncryptedAesKey,
                        onSuccess: () =>
                        {
                            _handshakeCompleted = true;
                            _handshakeCompletedCompletionSource.TrySetResult(true);
                            HandshakeCompleted?.Invoke();
                        },
                        onFail: () =>
                        {
                            LidgrenClient.Disconnect($"Improper {nameof(AesEncryptedAesKeyMessage)}.");
                        }
                    );

                    _nextExpectedMessageType = null;
                }
            );
        }
        
        public virtual Task ConnectAsync(string host, int port)
        {
            StartInternal();
            
            LidgrenClient.Connect(host, port);

            return _handshakeCompletedCompletionSource.Task;
        }
        
        private void OnConnected(NetIncomingMessage im, string reason)
        {
            Send(new RequestXmlRsaPublicKeyMessage(), NetDeliveryMethod.ReliableOrdered, 0);
        }

        private void OnDisconnected(NetIncomingMessage im, string reason)
        {
            _handshakeCompletedCompletionSource.TrySetResult(false);
        }

        protected override bool CanDecrypt(NetIncomingMessage incomingMessage)
        {
            return _handshakeCompleted;
        }

        protected override byte[] Decrypt(NetIncomingMessage incomingMessage)
        {
            return _clientCryptoService.Decrypt(incomingMessage.Data, incomingMessage.LengthBytes);
        }

        protected override bool AllowHandling(NetIncomingMessage incomingMessage, object message)
        {
            var disconnect = _nextExpectedMessageType != null && message.GetType() != _nextExpectedMessageType;
            
            if (disconnect)
            {
                LidgrenClient.Disconnect("Received message not allowed.");
            }

            return !disconnect;
        }

        protected void Send(object message, NetDeliveryMethod deliveryMethod, int sequenceChannel)
        {
            var outgoingMessage = CreateOutgoingMessage(message);

            if (_handshakeCompleted)
            {
                var encryptedBytes = _clientCryptoService.Encrypt(outgoingMessage.Data, outgoingMessage.LengthBytes);
                var encryptedMessage = LidgrenClient.CreateMessage(encryptedBytes);
                LidgrenClient.Recycle(outgoingMessage);
                outgoingMessage = encryptedMessage;
            }
            
            LidgrenClient.SendMessage(outgoingMessage, LidgrenClient.ServerConnection, deliveryMethod, sequenceChannel);
        }

        protected override bool AllowConnection(NetIncomingMessage incomingMessage)
        {
            return true;
        }
    }
}
