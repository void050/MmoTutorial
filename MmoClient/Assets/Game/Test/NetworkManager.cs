using Riptide.Utils;
using System;
using UnityEngine;

namespace Riptide.Demos.PlayerHosted
{
    internal enum MessageId : ushort
    {
        SpawnPlayer = 1,
        PlayerMovement
    }

    public class NetworkManager : MonoBehaviour
    {
        private static NetworkManager _singleton;

        public static NetworkManager Singleton
        {
            get => _singleton;
            private set
            {
                if (_singleton == null)
                    _singleton = value;
                else if (_singleton != value)
                {
                    Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying object!");
                    Destroy(value);
                }
            }
        }

        [SerializeField] private ushort port;
        [Header("Prefabs")] [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject localPlayerPrefab;

        public GameObject PlayerPrefab => playerPrefab;
        public GameObject LocalPlayerPrefab => localPlayerPrefab;

        internal Client Client { get; private set; }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

            Client = new Client();
            Client.Connected += DidConnect;
            Client.ConnectionFailed += FailedToConnect;
            Client.ClientDisconnected += PlayerLeft;
            Client.Disconnected += DidDisconnect;
            JoinGame($"127.0.0.1");
        }

        private void FixedUpdate()
        {
            Client.Update();
        }

        private void OnApplicationQuit()
        {
            Client.Disconnect();
        }

        internal void StartHost()
        {
            Client.Connect($"127.0.0.1:{port}");
        }

        internal void JoinGame(string ipString)
        {
            Client.Connect($"{ipString}:{port}");
        }

        internal void LeaveGame()
        {
            Client.Disconnect();
        }

        private void DidConnect(object sender, EventArgs e)
        {
            Debug.Log("DidConnect");
        }

        private void FailedToConnect(object sender, EventArgs e)
        {
            Debug.Log("FailedToConnect");
        }

        private void PlayerJoined(object sender, ServerConnectedEventArgs e)
        {
            Debug.Log("PlayerJoined");
            /*foreach (Player player in Player.List.Values)
                if (player.Id != e.Client.Id)
                    player.SendSpawn(e.Client.Id);*/
        }

        private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
        {
            Debug.Log("PlayerLeft");
        }

        private void DidDisconnect(object sender, DisconnectedEventArgs e)
        {
            Debug.Log("DidDisconnect");
        }
    }
}