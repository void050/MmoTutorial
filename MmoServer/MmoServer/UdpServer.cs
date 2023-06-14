using System.Numerics;
using Game;
using Game.Components;
using Riptide.Demos.ConsoleServer.Udp;
using Riptide.Utils;
using Shared;

namespace Riptide.Demos.ConsoleServer;

internal class UdpServerStarter
{
    private readonly RiptideServer _server = new()
    {
        TimeoutTime = ushort.MaxValue
    };

    private bool _isRunning;
    private readonly Dictionary<ushort, GameObject> _playerById = new();
    private readonly Dictionary<Connection, GameObject> _playerByConnection = new();
    private readonly Dictionary<string, ushort> _playerIdByLogin = new();
    private ushort _nextUserId = 1;
    private readonly GameObjectSystem _gameObjectSystem = new();

    public UdpServerStarter()
    {
        _server.OnMessage += OnMessageReceived;
    }

    public void Run()
    {
        Console.Title = "Server";

        RiptideLogger.Initialize(Console.WriteLine, true);
        _isRunning = true;

        new Thread(NetworkLoop).Start();
        new Thread(GameLoop).Start();

        Console.WriteLine("Press enter to stop the server at any time.");
        Console.ReadLine();

        _isRunning = false;

        Console.ReadLine();
    }

    private void NetworkLoop()
    {
        _server.Start(NetworkConfig.ServerPort, NetworkConfig.MaxClients);

        while (_isRunning)
        {
            try
            {
                _server.Update();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Thread.Sleep(NetworkConfig.TickIntervalMilliseconds);
        }

        _server.Stop();
    }

    private void GameLoop()
    {
        List<NetworkPlayerBehaviour> networkPlayers = new();
        while (_isRunning)
        {
            _gameObjectSystem.Update(NetworkConfig.TickInterval);
            networkPlayers.Clear();
            _gameObjectSystem.GetAll(networkPlayers);
            PlayerSnapshot[] players = new PlayerSnapshot[networkPlayers.Count];

            int next = 0;
            foreach (var networkPlayer in networkPlayers)
            {
                players[next++] = networkPlayer.GetSnapshot();
            }

            SnapshotResponse gameSnapshot = new SnapshotResponse
            {
                Snapshot = new GameSnapshot
                {
                    Players = players
                }
            };
            Message message = Message.Create(MessageSendMode.Unreliable, GameMessageId.SnapshotResponse.ToUShort());
            try
            {
                message.Add(gameSnapshot);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                continue;
            }

            _server.SendToAll(message);

            Thread.Sleep(NetworkConfig.TickIntervalMilliseconds);
        }
    }

    /// <summary>
    /// Never executed in two threads at once.
    /// </summary>
    private void OnMessageReceived(object? sender, MessageReceivedEventArgs args)
    {
        GameMessageId messageId = (GameMessageId)args.MessageId;
        try
        {
            Handle(args.FromConnection, args.Message, messageId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void Handle(Connection userConnection, Message incoming, GameMessageId messageId)
    {
        //todo use dictionary
        GameObject? player;
        switch (messageId)
        {
            case GameMessageId.JoinRequest:
                incoming.Get(out JoinRequest joinRequest);
                //todo add password check
                if (!_playerIdByLogin.TryGetValue(joinRequest.Login, out var playerId))
                {
                    playerId = _nextUserId++;
                    _playerIdByLogin.Add(joinRequest.Login, playerId);
                }

                if (!_playerById.TryGetValue(playerId, out player))
                {
                    player = CreatePlayer(playerId);
                    _playerById.Add(playerId, player);
                    _gameObjectSystem.AddGameObject(player);
                }

                _playerByConnection[userConnection] = player;

                player.GetRequiredBehaviour<NetworkPlayerBehaviour>().Connection = userConnection;

                Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.JoinResponse.ToUShort());
                message.Add(new JoinResponse { PlayerId = playerId });
                _server.Send(message, userConnection);
                break;

            case GameMessageId.InputRequest:

                if (!_playerByConnection.TryGetValue(userConnection, out player))
                {
                    Console.WriteLine("Not registered player");
                    return;
                }

                incoming.Get(out InputRequest inputRequest);

                player.GetRequiredBehaviour<PlayerMovementBehaviour>().PlayerInput = inputRequest.Input;
                break;
            default:
                Console.WriteLine($"Received unexpected: {messageId}");
                break;
        }
    }

    private GameObject CreatePlayer(ushort playerId)
    {
        var shared = Random.Shared;
        Vector2 position = new(shared.NextSingle() * 100, shared.NextSingle() * 100);
        return new GameObject($"player_{playerId}", new TransformBehaviour
        {
            Position = position
        }, new NetworkPlayerBehaviour
        {
            PlayerId = playerId
        }, new PlayerMovementBehaviour
        {
            Speed = 5
        });
    }
}