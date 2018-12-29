using System.Threading;
using Lidgren.Network;

namespace SkiEngine.Networking
{
    public abstract class SkiClient : SkiPeer
    {
        private NetClient LidgrenClient => (NetClient) LidgrenPeer;

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

        public void Send(INetMessage netMessage, NetDeliveryMethod deliveryMethod, int sequenceChannel)
        {
            var message = CreateOutgoingMessage(netMessage);
            LidgrenClient.SendMessage(message, LidgrenClient.ServerConnection, deliveryMethod, sequenceChannel);
        }

        protected override bool AllowConnection(INetMessage message)
        {
            return true;
        }
    }
}
