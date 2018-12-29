using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Lidgren.Network;

namespace SkiEngine.Networking
{
    public abstract class SkiServer : SkiPeer
    {
        private NetServer LidgrenServer => (NetServer) LidgrenPeer;

        protected SkiServer(NetPeerConfiguration config, SynchronizationContext receiveMessageContext = null) 
            : base(new NetServer(config), receiveMessageContext)
        {
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
        }

        public void Start()
        {
            StartInternal();
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
    }
}
