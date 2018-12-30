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
            _connectionMap[im.SenderConnection] = new SkiServerConnection(this);
        }

        private void OnDisconnected(NetIncomingMessage im, string reason)
        {
            _connectionMap.Remove(im.SenderConnection);
        }

        protected override bool AllowHandling(NetIncomingMessage incomingMessage, INetMessage netMessage)
        {
            return _connectionMap[incomingMessage.SenderConnection].AllowHandling(incomingMessage, netMessage);
        }

        public void Send(NetConnection recipient, INetMessage netMessage, NetDeliveryMethod deliveryMethod, int sequenceChannel)
        {
            var message = CreateOutgoingMessage(netMessage);
            LidgrenServer.SendMessage(message, recipient, deliveryMethod, sequenceChannel);
        }

        public void Send(IList<NetConnection> recipients, INetMessage netMessage, NetDeliveryMethod deliveryMethod, int sequenceChannel)
        {
            var message = CreateOutgoingMessage(netMessage);
            LidgrenServer.SendMessage(message, recipients, deliveryMethod, sequenceChannel);
        }

        public void SendAll(INetMessage netMessage, NetDeliveryMethod deliveryMethod, int sequenceChannel)
        {
            if(!LidgrenServer.Connections.Any())
            {
                return;
            }

            var message = CreateOutgoingMessage(netMessage);
            LidgrenServer.SendMessage(message, LidgrenServer.Connections, deliveryMethod, sequenceChannel);
        }

        public void SendAllExcept(NetConnection dontSendTo, INetMessage netMessage, NetDeliveryMethod deliveryMethod, int sequenceChannel)
        {
            var allExceptConnection = LidgrenServer.Connections.Where(c => c != dontSendTo).ToList();
            if (!allExceptConnection.Any())
            {
                return;
            }

            var message = CreateOutgoingMessage(netMessage);
            LidgrenServer.SendMessage(message, allExceptConnection, deliveryMethod, sequenceChannel);
        }

        private class SkiServerConnection
        {
            private readonly SkiServer _skiServer;

            private bool _handshakeCompleted;
            
            public SkiServerConnection(SkiServer skiServer)
            {
                _skiServer = skiServer;
            }

            public bool AllowHandling(NetIncomingMessage incomingMessage, INetMessage netMessage)
            {
                var disconnect = false;

                if (!_handshakeCompleted)
                {
                    if (netMessage is RsaEncryptedAesKeyMessage rsaEncryptedAesKeyMessage)
                    {
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

                                _handshakeCompleted = true;
                            },
                            onFail: () =>
                            {
                                disconnect = true;
                            }
                        );
                    }
                    else
                    {
                        disconnect = true;
                    }
                }

                if (disconnect)
                {
                    incomingMessage.SenderConnection.Disconnect("Received message not allowed.");
                }

                return !disconnect;
            }
        }
    }
}
