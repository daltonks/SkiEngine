using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Lidgren.Network;
using SkiEngine.Networking.Encryption;
using SkiEngine.Networking.Messages;

namespace SkiEngine.Networking
{
    public abstract class SkiServer : SkiPeer
    {
        private NetServer LidgrenServer => (NetServer) LidgrenPeer;

        private readonly Dictionary<NetConnection, SkiServerConnection> _connectionMap = new Dictionary<NetConnection,SkiServerConnection>();
        private readonly ServerCryptoService _serverCryptoService = new ServerCryptoService();

        protected SkiServer(NetPeerConfiguration config, SynchronizationContext receiveMessageContext = null) 
            : base(new NetServer(config), receiveMessageContext)
        {
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

            StatusConnected += OnConnected;
            StatusDisconnected += OnDisconnected;
        }
        
        public void Start()
        {
            StartInternal();
        }

        private void OnConnected(NetIncomingMessage im, string reason)
        {
            _connectionMap[im.SenderConnection] = new SkiServerConnection(this, im.SenderConnection);
        }

        private void OnDisconnected(NetIncomingMessage im, string reason)
        {
            _connectionMap.Remove(im.SenderConnection);
        }

        protected override bool AllowHandling(NetIncomingMessage incomingMessage, INetMessage netMessage)
        {
            return _connectionMap[incomingMessage.SenderConnection].AllowHandling(incomingMessage, netMessage);
        }

        protected override bool CanDecrypt(NetIncomingMessage incomingMessage)
        {
            return _connectionMap[incomingMessage.SenderConnection].HandshakeCompleted;
        }

        protected override byte[] Decrypt(NetIncomingMessage incomingMessage)
        {
            return _connectionMap[incomingMessage.SenderConnection].Decrypt(incomingMessage.Data);
        }

        public void Send(NetConnection recipient, INetMessage netMessage, NetDeliveryMethod deliveryMethod, int sequenceChannel)
        {
            var connection = _connectionMap[recipient];

            var message = CreateOutgoingMessage(netMessage);
            if (connection.HandshakeCompleted)
            {
                var encryptedBytes = connection.Encrypt(message.Data);
                var encryptedMessage = LidgrenServer.CreateMessage(encryptedBytes.Length);
                LidgrenServer.Recycle(message);
                message = encryptedMessage;
            }
            
            LidgrenServer.SendMessage(message, recipient, deliveryMethod, sequenceChannel);
        }

        private class SkiServerConnection
        {
            public bool HandshakeCompleted { get; private set; }

            private readonly SkiServer _skiServer;
            private readonly NetConnection _netConnection;
            private AesService _aesService;
            
            public SkiServerConnection(SkiServer skiServer, NetConnection netConnection)
            {
                _skiServer = skiServer;
                _netConnection = netConnection;
            }

            public bool AllowHandling(NetIncomingMessage incomingMessage, INetMessage netMessage)
            {
                var disconnect = false;

                if (!HandshakeCompleted)
                {
                    switch (netMessage)
                    {
                        case RequestXmlRsaPublicKeyMessage _:
                            _skiServer.Send(
                                _netConnection, 
                                new XmlRsaPublicKeyMessage { XmlRsaPublicKey = _skiServer._serverCryptoService.XmlRsaPublicKey }, 
                                NetDeliveryMethod.ReliableOrdered,
                                0
                            );
                            break;
                        case RsaEncryptedAesKeyMessage rsaEncryptedAesKeyMessage:
                            _skiServer._serverCryptoService.ReceivedRsaEncryptedAesKey(
                                rsaEncryptedAesKeyMessage.RsaEncryptedAesKey,
                                serviceAndEncryptedKey =>
                                {
                                    _skiServer.Send(
                                        incomingMessage.SenderConnection, 
                                        new AesEncryptedAesKeyMessage { AesEncryptedAesKey = serviceAndEncryptedKey.AesEncryptedAesKey}, 
                                        NetDeliveryMethod.ReliableOrdered,
                                        0
                                    );

                                    _aesService = serviceAndEncryptedKey.AesService;

                                    HandshakeCompleted = true;
                                },
                                onFail: () =>
                                {
                                    disconnect = true;
                                }
                            );
                            break;
                        default:
                            disconnect = true;
                            break;
                    }
                }

                if (disconnect)
                {
                    incomingMessage.SenderConnection.Disconnect("Received message not allowed.");
                }

                return !disconnect;
            }

            public byte[] Encrypt(byte[] data)
            {
                return _aesService.Encrypt(data);
            }

            public byte[] Decrypt(byte[] data)
            {
                return _aesService.Decrypt(data);
            }
        }
    }
}
