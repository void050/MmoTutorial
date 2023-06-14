using Shared;

namespace ServerTests;

public class Tests
{
    [Test]
    public void Test1()
    {
        Assert.That(NetworkConfig.TickRate, Is.EqualTo(20));
    }
}