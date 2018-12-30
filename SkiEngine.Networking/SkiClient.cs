using System;
using System.Threading;
using Lidgren.Network;
using SkiEngine.Networking.Encryption;
using SkiEngine.Networking.Messages;

namespace SkiEngine.Networking
{
    public abstract class SkiClient : SkiPeer
    {
        private NetClient LidgrenClient => (NetClient) LidgrenPeer;

        private bool _handshakeCompleted;
        private ClientCryptoService _clientCryptoService;
        private Type _nextExpectedMessageType = typeof(XmlRsaPublicKeyMessage);

        protected SkiClient(NetPeerConfiguration config, SynchronizationContext receiveMessageContext = null) 
            : base(new NetClient(config), receiveMessageContext)
        {
            StatusConnected += OnConnected;
        }
        
        public void Connect(string host, int port)
        {
            StartInternal();
            
            LidgrenClient.Connect(host, port);
        }

        private void OnConnected(NetIncomingMessage im, string reason)
        {
            Send(new RequestXmlRsaPublicKeyMessage(), NetDeliveryMethod.ReliableOrdered, 0);
        }

        protected override bool CanDecrypt(NetIncomingMessage incomingMessage)
        {
            return _handshakeCompleted;
        }

        protected override byte[] Decrypt(NetIncomingMessage incomingMessage)
        {
            return _clientCryptoService.Decrypt(incomingMessage.Data);
        }

        protected override bool AllowHandling(NetIncomingMessage incomingMessage, INetMessage netMessage)
        {
            var disconnect = _nextExpectedMessageType != null && netMessage.GetType() != _nextExpectedMessageType;

            if (!disconnect)
            {
                switch (netMessage)
                {
                    case XmlRsaPublicKeyMessage xmlRsaPublicKeyMessage:
                        _clientCryptoService = new ClientCryptoService(
                            xmlRsaPublicKeyMessage.XmlRsaPublicKey,
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
                        break;
                    case AesEncryptedAesKeyMessage aesEncryptedAesKeyMessage:
                        _clientCryptoService.ReceivedAesEncryptedAesKey(
                            aesEncryptedAesKeyMessage.AesEncryptedAesKey,
                            onSuccess: () =>
                            {
                                _handshakeCompleted = true;
                            },
                            onFail: () =>
                            {
                                disconnect = true;
                            }
                        );

                        _nextExpectedMessageType = null;
                        break;
                }
            }
            
            if (disconnect)
            {
                LidgrenClient.Disconnect("Received message not allowed.");
            }

            return !disconnect;
        }

        public void Send(INetMessage netMessage, NetDeliveryMethod deliveryMethod, int sequenceChannel)
        {
            var message = CreateOutgoingMessage(netMessage);

            if (_handshakeCompleted)
            {
                var encryptedBytes = _clientCryptoService.Encrypt(message.Data);
                var encryptedMessage = LidgrenClient.CreateMessage(encryptedBytes.Length);
                LidgrenClient.Recycle(message);
                message = encryptedMessage;
            }
            
            LidgrenClient.SendMessage(message, LidgrenClient.ServerConnection, deliveryMethod, sequenceChannel);
        }

        protected override bool AllowConnection(NetIncomingMessage incomingMessage)
        {
            return true;
        }
    }
}
