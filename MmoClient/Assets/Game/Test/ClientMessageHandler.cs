using System;
using Shared;
using UnityEngine;

namespace Riptide.Demos.PlayerHosted
{
    public class ClientMessageHandler
    {
        private readonly Client _client;
        public ushort? PlayerId { get; private set; }

        public ClientMessageHandler(Client client)
        {
            _client = client;
            _client.MessageReceived += HandleMessage;
        }

        private void HandleMessage(object sender, MessageReceivedEventArgs args)
        {
            GameMessageId messageId = (GameMessageId)args.MessageId;

            Message response = args.Message;
            switch (messageId)
            {
                case GameMessageId.JoinResponse:
                    response.Get(out JoinResponse joinResponse);
                    Debug.Log($"Received JoinResponse PlayerId: {joinResponse.PlayerId}");
                    PlayerId = joinResponse.PlayerId;
                    break;
                case GameMessageId.SnapshotResponse:
                    response.Get(out SnapshotResponse snapshotResponse);
                    Debug.Log(snapshotResponse.Snapshot.ToString());
                    break;
                default:
                    Debug.LogError($"Received Unknown {messageId}");
                    break;
            }
        }
    }
}