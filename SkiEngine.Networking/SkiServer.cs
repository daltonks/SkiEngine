﻿using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;

namespace SkiEngine.Networking
{
    public abstract class SkiServer : SkiPeer<NetServer>
    {
        private readonly float _updateTimeMilliseconds;

        protected SkiServer(NetPeerConfiguration config, float updateTimeMilliseconds = 1000f / 60f)
            : base(new NetServer(config))
        {
            _updateTimeMilliseconds = updateTimeMilliseconds;
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

            Started += () =>
            {
                var timer = new HighResolutionTimer(_updateTimeMilliseconds);
                timer.Elapsed += Update;
                timer.Start();
            };
        }

        private void Update(object sender, HighResolutionTimerElapsedEventArgs args)
        {
            // Process incoming messages
            while (IncomingMessages.TryDequeue(out var incomingMessage))
            {
                ProcessMessage(incomingMessage);
                LidgrenPeer.Recycle(incomingMessage);
            }

            Update(_updateTimeMilliseconds / 1000);
        }

        protected abstract void Update(double deltaSeconds);

        public void Send(NetConnection recipient, INetMessage netMessage, NetDeliveryMethod deliveryMethod, int sequenceChannel)
        {
            var message = CreateOutgoingMessage(netMessage);
            LidgrenPeer.SendMessage(message, recipient, deliveryMethod, sequenceChannel);
        }

        public void Send(IList<NetConnection> recipients, INetMessage netMessage, NetDeliveryMethod deliveryMethod, int sequenceChannel)
        {
            var message = CreateOutgoingMessage(netMessage);
            LidgrenPeer.SendMessage(message, recipients, deliveryMethod, sequenceChannel);
        }

        public void SendAll(INetMessage netMessage, NetDeliveryMethod deliveryMethod, int sequenceChannel)
        {
            if(!LidgrenPeer.Connections.Any())
            {
                return;
            }

            var message = CreateOutgoingMessage(netMessage);
            LidgrenPeer.SendMessage(message, LidgrenPeer.Connections, deliveryMethod, sequenceChannel);
        }

        public void SendAllExcept(NetConnection dontSendTo, INetMessage netMessage, NetDeliveryMethod deliveryMethod, int sequenceChannel)
        {
            var allExceptConnection = LidgrenPeer.Connections.Where(c => c != dontSendTo).ToList();
            if (!allExceptConnection.Any())
            {
                return;
            }

            var message = CreateOutgoingMessage(netMessage);
            LidgrenPeer.SendMessage(message, allExceptConnection, deliveryMethod, sequenceChannel);
        }
    }
}
