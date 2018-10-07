using Lidgren.Network;
using SkiEngine.NCS.Component.Base;
using SkiEngine.NCS.System;

namespace SkiEngine.Networking
{
    public abstract class ClientComponent : PeerComponent<NetClient>, IUpdateableComponent
    {
        public UpdateableComponentPart UpdateablePart { get; }

        protected ClientComponent(NetPeerConfiguration config, string password = "")
            : base(new NetClient(config), password)
        {
            UpdateablePart = new UpdateableComponentPart(Update) { UpdateOrder = int.MinValue };
        }

        private void Update(UpdateTime updateTime)
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
