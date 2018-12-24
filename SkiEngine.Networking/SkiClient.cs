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

        public void Connect(string host, int port, IPacket hailPacket = null)
        {
            LidgrenPeer.Start();

            NetOutgoingMessage hailMessage;

            if (hailPacket == null)
            {
                hailMessage = LidgrenPeer.CreateMessage();
                hailMessage.WriteVariableInt32(-1);
            }
            else
            {
                hailMessage = CreateOutgoingMessage(hailPacket);
            }
            
            LidgrenPeer.Connect(host, port, hailMessage);

            StartReadMessagesConcurrently();
        }

        public void Send(IPacket packet, NetDeliveryMethod deliveryMethod, int sequenceChannel)
        {
            var message = CreateOutgoingMessage(packet);
            LidgrenPeer.SendMessage(message, LidgrenPeer.ServerConnection, deliveryMethod, sequenceChannel);
        }

        protected override bool AllowJoin(IPacket hailPacket = null)
        {
            return true;
        }
    }
}
