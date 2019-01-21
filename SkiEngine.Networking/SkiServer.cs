using System.Collections.Generic;
using System.Threading;
using Lidgren.Network;
using SkiEngine.Networking.Encryption;
using SkiEngine.Networking.Messages;

namespace SkiEngine.Networking
{
    public abstract class SkiServer : SkiPeer
    {
        public NetServer LidgrenServer => (NetServer) LidgrenPeer;

        protected readonly Dictionary<NetConnection, SkiServerConnection> _skiConnections = new Dictionary<NetConnection,SkiServerConnection>();
        private readonly ServerCryptoService _serverCryptoService = new ServerCryptoService();

        protected SkiServer(NetPeerConfiguration config, SynchronizationContext receiveMessageContext = null) 
            : base(new NetServer(config), receiveMessageContext)
        {
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

            StatusConnected += OnConnected;
            StatusDisconnected += OnDisconnected;

            RegisterReceiveHandler<RequestXmlRsaPublicKeyMessage>(
                (message, netMessage) =>
                {
                    Send(
                        netMessage.SenderConnection, 
                        new XmlRsaPublicKeyMessage { XmlRsaPublicKey = _serverCryptoService.XmlRsaPublicKey }, 
                        NetDeliveryMethod.ReliableOrdered,
                        0
                    );
                }
            );

            RegisterReceiveHandler<RsaEncryptedAesKeyMessage>(
                (message, netMessage) =>
                {
                    _serverCryptoService.ReceivedRsaEncryptedAesKey(
                        message.RsaEncryptedAesKey,
                        serviceAndEncryptedKey =>
                        {
                            Send(
                                netMessage.SenderConnection, 
                                new AesEncryptedAesKeyMessage { AesEncryptedAesKey = serviceAndEncryptedKey.AesEncryptedAesKey}, 
                                NetDeliveryMethod.ReliableOrdered,
                                0
                            );

                            var skiConnection = _skiConnections[netMessage.SenderConnection];
                            skiConnection.AesService = serviceAndEncryptedKey.AesService;
                            skiConnection.HandshakeCompleted = true;
                        },
                        onFail: () =>
                        {
                            netMessage.SenderConnection.Disconnect("Message not valid.");
                        }
                    );
                }
            );
        }
        
        public void Start()
        {
            StartInternal();
        }

        private void OnConnected(NetIncomingMessage im, string reason)
        {
            _skiConnections[im.SenderConnection] = new SkiServerConnection();
        }

        private void OnDisconnected(NetIncomingMessage im, string reason)
        {
            _skiConnections.Remove(im.SenderConnection);
        }

        protected override bool AllowHandling(NetIncomingMessage incomingMessage, object message)
        {
            return _skiConnections[incomingMessage.SenderConnection].AllowHandling(incomingMessage, message);
        }

        protected override bool CanDecrypt(NetIncomingMessage incomingMessage)
        {
            return _skiConnections[incomingMessage.SenderConnection].HandshakeCompleted;
        }

        protected override byte[] Decrypt(NetIncomingMessage incomingMessage)
        {
            return _skiConnections[incomingMessage.SenderConnection]
                .Decrypt(incomingMessage.Data, incomingMessage.LengthBytes);
        }

        public void Send(NetConnection recipient, object message, NetDeliveryMethod deliveryMethod, int sequenceChannel)
        {
            var connection = _skiConnections[recipient];

            var outgoingMessage = CreateOutgoingMessage(message);
            if (connection.HandshakeCompleted)
            {
                var encryptedBytes = connection.Encrypt(outgoingMessage.Data, outgoingMessage.LengthBytes);
                outgoingMessage = LidgrenServer.CreateMessage(encryptedBytes.Length);
                outgoingMessage.Write(encryptedBytes);
            }
            
            LidgrenServer.SendMessage(outgoingMessage, recipient, deliveryMethod, sequenceChannel);
        }

        public class SkiServerConnection
        {
            public bool HandshakeCompleted { get; set; }
            public AesService AesService { get; set; }

            public bool AllowHandling(NetIncomingMessage incomingMessage, object netMessage)
            {
                var disconnect = false;

                if (!HandshakeCompleted)
                {
                    disconnect = netMessage.GetType() != typeof(RequestXmlRsaPublicKeyMessage)
                       && netMessage.GetType() != typeof(RsaEncryptedAesKeyMessage);
                }

                if (disconnect)
                {
                    incomingMessage.SenderConnection.Disconnect("Received message not allowed.");
                }

                return !disconnect;
            }

            public byte[] Encrypt(byte[] data, int length)
            {
                return AesService.Encrypt(data, length);
            }

            public byte[] Decrypt(byte[] data, int length)
            {
                return AesService.Decrypt(data, length);
            }
        }
    }
}
