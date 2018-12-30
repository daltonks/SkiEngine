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

        private ClientCryptoService _clientCryptoService;
        private Type _nextExpectedMessageType = typeof(XmlRsaPublicKeyMessage);

        protected SkiClient(NetPeerConfiguration config, SynchronizationContext receiveMessageContext = null) 
            : base(new NetClient(config), receiveMessageContext)
        {
            
        }

        public void Connect(string host, int port, INetMessage hailNetMessage = null)
        {
            StartInternal();

            NetOutgoingMessage hailMessage;

            if (hailNetMessage == null)
            {
                hailMessage = LidgrenClient.CreateMessage();
                hailMessage.WriteVariableInt32(-1);
            }
            else
            {
                hailMessage = CreateOutgoingMessage(hailNetMessage);
            }
            
            LidgrenClient.Connect(host, port, hailMessage);
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
                            onSuccess: null,
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
            LidgrenClient.SendMessage(message, LidgrenClient.ServerConnection, deliveryMethod, sequenceChannel);
        }

        protected override bool AllowConnection(NetIncomingMessage incomingMessage)
        {
            return true;
        }
    }
}
