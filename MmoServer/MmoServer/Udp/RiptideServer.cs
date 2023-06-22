using Riptide;

namespace Game.Udp;

public class RiptideServer : Server
{
    public event EventHandler<MessageReceivedEventArgs>? OnMessage;

    protected override void OnMessageReceived(Message message, Connection fromConnection)
    {
        ushort messageId = message.GetUShort();
        OnMessage?.Invoke(this, new MessageReceivedEventArgs(fromConnection, messageId, message));
    }
}