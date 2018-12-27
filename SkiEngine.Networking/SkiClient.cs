using Lidgren.Network;

namespace SkiEngine.Networking
{
    public abstract class SkiClient : SkiPeer<NetClient>
    {
        protected SkiClient(NetPeerConfiguration config) : base(new NetClient(config))
        {
            
        }

        public void Update()
        {
            while (IncomingMessages.TryDequeue(out var incomingMessage))
            {
                ProcessMessage(incomingMessage);
                LidgrenPeer.Recycle(incomingMessage);
            }
        }

        public void Connect(string host, int port, INetMessage hailNetMessage = null)
        {
            LidgrenPeer.Start();

            NetOutgoingMessage hailMessage;

            if (hailNetMessage == null)
            {
                hailMessage = LidgrenPeer.CreateMessage();
                hailMessage.WriteVariableInt32(-1);
            }
            else
            {
                hailMessage = CreateOutgoingMessage(hailNetMessage);
            }
            
            LidgrenPeer.Connect(host, port, hailMessage);

            StartReadMessagesConcurrently();
        }

        public void Send(INetMessage netMessage, NetDeliveryMethod deliveryMethod, int sequenceChannel)
        {
            var message = CreateOutgoingMessage(netMessage);
            LidgrenPeer.SendMessage(message, LidgrenPeer.ServerConnection, deliveryMethod, sequenceChannel);
        }

        protected override bool AllowConnection(INetMessage hailNetMessage = null)
        {
            return true;
        }
    }
}
