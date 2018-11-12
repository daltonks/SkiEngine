using Lidgren.Network;

namespace SkiEngine.Networking
{
    public abstract class SkiClient : SkiPeer<NetClient>
    {
        protected SkiClient(NetPeerConfiguration config, string password = "")
            : base(new NetClient(config), password)
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

        public void Connect(string host, int port)
        {
            LidgrenPeer.Start();
            var hail = LidgrenPeer.CreateMessage(Password);
            LidgrenPeer.Connect(host, port, hail);

            StartReadMessagesConcurrently();
        }

        public void Send(IPacket packet, NetDeliveryMethod deliveryMethod, int sequenceChannel)
        {
            if(TryCreateOutgoingMessage(packet, out var message))
            {
                LidgrenPeer.SendMessage(message, LidgrenPeer.ServerConnection, deliveryMethod, sequenceChannel);
            }
        }

        protected override bool AllowJoin(NetIncomingMessage im)
        {
            return true;
        }
    }
}
