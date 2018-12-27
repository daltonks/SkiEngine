using Lidgren.Network;

namespace SkiEngine.Networking
{
    public abstract class SkiClient : SkiPeer
    {
        private NetClient LidgrenClient => (NetClient) LidgrenPeer;

        protected SkiClient(NetPeerConfiguration config) : base(new NetClient(config))
        {
            
        }

        public void Update()
        {
            while (IncomingMessages.TryDequeue(out var incomingMessage))
            {
                ProcessMessage(incomingMessage);
                LidgrenClient.Recycle(incomingMessage);
            }
        }

        public void Connect(string host, int port, INetMessage hailNetMessage = null)
        {
            LidgrenClient.Start();

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

            StartReadMessagesConcurrently();
        }

        public void Send(INetMessage netMessage, NetDeliveryMethod deliveryMethod, int sequenceChannel)
        {
            var message = CreateOutgoingMessage(netMessage);
            LidgrenClient.SendMessage(message, LidgrenClient.ServerConnection, deliveryMethod, sequenceChannel);
        }

        protected override bool AllowConnection(NetIncomingMessage incomingMessage, INetMessage message)
        {
            return true;
        }
    }
}
