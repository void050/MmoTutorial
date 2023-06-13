using System;
using Shared;
using UnityEngine;

namespace Riptide.Demos.PlayerHosted
{
    public class ClientMessageHandler
    {
        private readonly Client _client;

        public ClientMessageHandler(Client client)
        {
            _client = client;
            _client.MessageReceived += HandleMessage;
        }

        private void HandleMessage(object sender, MessageReceivedEventArgs args)
        {
            GameMessageId messageId = (GameMessageId)args.MessageId;
            switch (messageId)
            {
                case GameMessageId.None:
                    Debug.LogError("Received None");
                    break;
                case GameMessageId.JoinRequest:
                    Debug.LogError("Received JoinRequest");
                    break;
                case GameMessageId.JoinResponse:
                    Debug.Log("Received JoinResponse");
                    string response = args.Message.GetString();
                    Debug.Log(response);
                    break;
                default:
                    Debug.LogError($"Received Unknown {args.MessageId.ToString()}");
                    break;
            }
        }
    }
}