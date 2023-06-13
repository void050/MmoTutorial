using Riptide.Demos.ConsoleServer.Udp;
using Riptide.Utils;
using Shared;

namespace Riptide.Demos.ConsoleServer
{
    internal class UdpServerStarter
    {
        private static RiptideServer server;
        private static bool isRunning;


        public static void Run()
        {
            Console.Title = "Server";

            RiptideLogger.Initialize(Console.WriteLine, true);
            isRunning = true;

            new Thread(Loop).Start();

            Console.WriteLine("Press enter to stop the server at any time.");
            Console.ReadLine();

            isRunning = false;

            Console.ReadLine();
        }

        private static void Loop()
        {
            server = new RiptideServer
            {
                TimeoutTime = ushort.MaxValue
            };
            server.OnMessage += OnMessageReceived;
            server.Start(NetworkConfig.ServerPort, NetworkConfig.MaxClients);

            while (isRunning)
            {
                server.Update();
                Thread.Sleep(10);
            }

            server.Stop();
        }

        private static void OnMessageReceived(object? sender, MessageReceivedEventArgs args)
        {
            //todo use dictionary
            GameMessageId messageId = (GameMessageId)args.MessageId;
            Console.WriteLine($"Received {messageId}");
            switch (messageId)
            {
                case GameMessageId.None:
                    Console.WriteLine("Received None");
                    break;
                case GameMessageId.JoinRequest:
                    Console.WriteLine("Received JoinRequest");
                    Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.JoinResponse);
                    message.AddString("Welcome to the server!");
                    server.Send(message, args.FromConnection);
                    break;
                case GameMessageId.JoinResponse:
                    Console.WriteLine("Received JoinResponse");
                    break;
                default:
                    Console.WriteLine($"Received Unknown {args.MessageId.ToString()}");
                    break;
            }
        }
    }
}