using System.Diagnostics;
using System.Numerics;
using Game;
using Game.Components;
using Riptide.Demos.ConsoleServer.Udp;
using Riptide.Utils;
using Shared;
using PrecisionTiming;

namespace Riptide.Demos.ConsoleServer;

internal class UdpServerStarter
{
    private readonly RiptideServer _server = new()
    {
        TimeoutTime = ushort.MaxValue
    };

    private readonly Dictionary<ushort, GameObject> _playerById = new();
    private readonly Dictionary<Connection, GameObject> _playerByConnection = new();
    private readonly Dictionary<string, ushort> _playerIdByLogin = new();
    private readonly GameObjectSystem _gameObjectSystem = new();
    private readonly List<Message> _sendToAll = new();
    private readonly List<Message> _iterateSendToAll = new();
    private readonly PrecisionTimer _gameLoopTimer = new();
    private readonly PrecisionTimer _networkLoopTimer = new();
    private int _gameLoopUpdateIndex;
    private ushort _nextUserId = 1;

    public UdpServerStarter()
    {
        _server.OnMessage += OnMessageReceived;
        _networkLoopTimer.SetInterval(NetworkLoop, NetworkConfig.TickIntervalMilliseconds, false);
        _gameLoopTimer.SetInterval(GameLoop, NetworkConfig.TickIntervalMilliseconds, false);
    }

    public void Run()
    {
        Console.Title = "Server";

        RiptideLogger.Initialize(Console.WriteLine, true);

        const int botsCount = 75;
        for (int i = 0; i < botsCount; i++)
        {
            var bot = CreateBot(_nextUserId++);
            _gameObjectSystem.AddGameObject(bot);
        }

        _server.Start(NetworkConfig.ServerPort, NetworkConfig.MaxClients);
        _networkLoopTimer.Start();
        _gameLoopTimer.Start();

        Console.WriteLine("Press enter to stop the server at any time.");
        Console.ReadLine();

        _server.Stop();

        Console.ReadLine();
    }

    private void NetworkLoop()
    {
        try
        {
            _iterateSendToAll.Clear();
            lock (_sendToAll)
            {
                _iterateSendToAll.AddRange(_sendToAll);
                _sendToAll.Clear();
            }

            foreach (var message in _iterateSendToAll)
            {
                _server.SendToAll(message);
            }

            _server.Update();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }


    private void GameLoop()
    {
        List<NetworkPlayerBehaviour> networkPlayers = new();
        _gameObjectSystem.Update(NetworkConfig.TickInterval);

        if (_gameLoopUpdateIndex++ % NetworkConfig.SnapshotEveryTick != 0)
        {
            return;
        }

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
            return;
        }

        lock (_sendToAll)
        {
            _sendToAll.Add(message);
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

    private GameObject CreateBot(ushort playerId)
    {
        var gameObject = CreatePlayer(playerId);
        gameObject.AddComponent(new SimulatedPlayerBehaviour
        {
            ChangeDirectionEverySeconds = Random.Shared.NextSingle() / 2 + 0.5f
        });
        return gameObject;
    }

    private GameObject CreatePlayer(ushort playerId)
    {
        var shared = Random.Shared;
        Vector2 position = new(shared.NextSingle() * 10, shared.NextSingle() * 10);
        return new GameObject($"player_{playerId}", new TransformBehaviour
        {
            Position = position
        }, new NetworkPlayerBehaviour
        {
            PlayerId = playerId,
            ViewId = (byte)Random.Shared.Next(1, 3)
        }, new PlayerMovementBehaviour
        {
            Speed = 5
        });
    }
}