using Frends.RabbitMQ.Read.Definitions;
using Frends.RabbitMQ.Read.Tests.Lib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using System.Text;

namespace Frends.RabbitMQ.Read.Tests;

[TestClass]
public class UnitTests
{
    /// <summary>
    /// You will need access to RabbitMQ queue, you can create it e.g. by running
    /// docker run -d --hostname my-rabbit -p 5672:5672 -p 8080:1567 -p 15672:15672 -e RABBITMQ_DEFAULT_USER=agent -e RABBITMQ_DEFAULT_PASS=agent123  rabbitmq:3.9-management
    /// In that case URI would be amqp://agent:agent123@localhost:5672 
    /// Access UI from http://localhost:15672 username: agent, password: agent123
    /// </summary>

    private const string TestUri = "amqp://agent:agent123@localhost:5672";
    private const string TestHost = "localhost";

    [TestInitialize]
    public void CreateExchangeAndQueue()
    {
        var factory = new ConnectionFactory { Uri = new Uri(TestUri) };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.ExchangeDeclare("exchange", type: "fanout", durable: false, autoDelete: false);
        channel.QueueDeclare("queue", durable: false, exclusive: false, autoDelete: false);
        channel.QueueBind("queue", "exchange", routingKey: "");
    }

    [TestCleanup]
    public void DeleteExchangeAndQueue()
    {
        var factory = new ConnectionFactory { Uri = new Uri(TestUri) };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDelete("queue", false, false);
        channel.ExchangeDelete("exchange", ifUnused: false);
    }

    /// <summary>
    /// Connect with hostname and read single message.
    /// </summary>
    [TestMethod]
    public void TestReadSingleMessageWithHostAutoNack()
    {
        Connection connection = new()
        {
            Host = TestHost,
            Username = "agent",
            Password = "agent123",
            RoutingKey = "queue",
            QueueName = "queue",
            AuthenticationMethod = AuthenticationMethod.Host,
            ExchangeName = null,

            AutoAck = ReadAckType.AutoAck,
            ReadMessageCount = 1,
        };

        Publish(connection, 1);
        var result = RabbitMQ.Read(connection);

        Assert.IsTrue(result.MessagesBase64.Count == 1 && result.MessageUTF8.Count == 1 && result.Success.Equals(true));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Data.Equals("VGVzdCBtZXNzYWdlIDA=") && result.MessageUTF8.Any(x => x.Data.Equals("Test message 0"))));

        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("custom header"))));

        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("custom header"))));
    }

    /// <summary>
    /// Connect with hostname and read multiple messages.
    /// </summary>
    [TestMethod]
    public void TestReadMultipleMessagesWithHostAutoNack()
    {
        Connection connection = new()
        {
            Host = TestHost,
            Username = "agent",
            Password = "agent123",
            RoutingKey = "queue",
            QueueName = "queue",
            AuthenticationMethod = AuthenticationMethod.Host,
            ExchangeName = null,

            AutoAck = ReadAckType.AutoAck,
            ReadMessageCount = 2,
        };

        Publish(connection, 2);
        var result = RabbitMQ.Read(connection);

        var test1 = result.MessagesBase64.Count;
        var test2 = result.MessageUTF8.Count;


        Assert.IsTrue(result.MessagesBase64.Count > 1 && result.MessageUTF8.Count > 1 && result.Success.Equals(true));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Data.Equals("VGVzdCBtZXNzYWdlIDA=") && result.MessageUTF8.Any(x => x.Data.Equals("Test message 0")))
            && result.MessagesBase64.Any(x => x.Data.Equals("VGVzdCBtZXNzYWdlIDE=") && result.MessageUTF8.Any(x => x.Data.Equals("Test message 1"))));

        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("custom header"))));

        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("custom header"))));
    }

    /// <summary>
    /// Connect with URI and read single message.
    /// </summary>
    [TestMethod]
    public void TestReadSingleMessageWithURIAutoNack()
    {
        Connection connection = new()
        {
            Host = TestUri,
            RoutingKey = "queue",
            QueueName = "queue",
            AuthenticationMethod = AuthenticationMethod.URI,
            ExchangeName = null,

            AutoAck = ReadAckType.AutoAck,
            ReadMessageCount = 1,
        };

        Publish(connection, 1);
        var result = RabbitMQ.Read(connection);

        Assert.IsTrue(result.MessagesBase64.Count == 1 && result.MessageUTF8.Count == 1 && result.Success.Equals(true));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Data.Equals("VGVzdCBtZXNzYWdlIDA=") && result.MessageUTF8.Any(x => x.Data.Equals("Test message 0"))));

        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("custom header"))));

        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("custom header"))));
    }

    [TestMethod]
    public void TestReadSingleMessageWithHostAutoNackAndRequeue()
    {
        Connection connection = new()
        {
            Host = TestHost,
            Username = "agent",
            Password = "agent123",
            RoutingKey = "queue",
            QueueName = "queue",
            AuthenticationMethod = AuthenticationMethod.Host,
            ExchangeName = null,

            AutoAck = ReadAckType.AutoNackAndRequeue,
            ReadMessageCount = 1,
        };

        Publish(connection, 1);
        var result = RabbitMQ.Read(connection);

        Assert.IsTrue(result.MessagesBase64.Count == 1 && result.MessageUTF8.Count == 1 && result.Success.Equals(true));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Data.Equals("VGVzdCBtZXNzYWdlIDA=") && result.MessageUTF8.Any(x => x.Data.Equals("Test message 0"))));

        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("custom header"))));

        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("custom header"))));
    }

    /// <summary>
    /// Connect with hostname and read multiple messages.
    /// </summary>
    [TestMethod]
    public void TestReadMultipleMessagesWithHostAutoNackAndRequeue()
    {
        Connection connection = new()
        {
            Host = TestHost,
            Username = "agent",
            Password = "agent123",
            RoutingKey = "queue",
            QueueName = "queue",
            AuthenticationMethod = AuthenticationMethod.Host,
            ExchangeName = null,

            AutoAck = ReadAckType.AutoNackAndRequeue,
            ReadMessageCount = 2,
        };

        Publish(connection, 2);
        var result = RabbitMQ.Read(connection);

        var test1 = result.MessagesBase64.Count;
        var test2 = result.MessageUTF8.Count;


        Assert.IsTrue(result.MessagesBase64.Count > 1 && result.MessageUTF8.Count > 1 && result.Success.Equals(true));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Data.Equals("VGVzdCBtZXNzYWdlIDA=") && result.MessageUTF8.Any(x => x.Data.Equals("Test message 0")))
            && result.MessagesBase64.Any(x => x.Data.Equals("VGVzdCBtZXNzYWdlIDE=") && result.MessageUTF8.Any(x => x.Data.Equals("Test message 1"))));

        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("custom header"))));

        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("custom header"))));
    }

    /// <summary>
    /// Connect with URI and read single message.
    /// </summary>
    [TestMethod]
    public void TestReadSingleMessageWithURIAutoNackAndRequeue()
    {
        Connection connection = new()
        {
            Host = TestUri,
            RoutingKey = "queue",
            QueueName = "queue",
            AuthenticationMethod = AuthenticationMethod.URI,
            ExchangeName = null,

            AutoAck = ReadAckType.AutoNackAndRequeue,
            ReadMessageCount = 1,
        };

        Publish(connection, 1);
        var result = RabbitMQ.Read(connection);

        Assert.IsTrue(result.MessagesBase64.Count == 1 && result.MessageUTF8.Count == 1 && result.Success.Equals(true));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Data.Equals("VGVzdCBtZXNzYWdlIDA=") && result.MessageUTF8.Any(x => x.Data.Equals("Test message 0"))));

        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("custom header"))));

        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("custom header"))));
    }

    [TestMethod]
    public void TestReadSingleMessageWithHostAutoReject()
    {
        Connection connection = new()
        {
            Host = TestHost,
            Username = "agent",
            Password = "agent123",
            RoutingKey = "queue",
            QueueName = "queue",
            AuthenticationMethod = AuthenticationMethod.Host,
            ExchangeName = null,

            AutoAck = ReadAckType.AutoReject,
            ReadMessageCount = 1,
        };

        Publish(connection, 1);
        var result = RabbitMQ.Read(connection);

        Assert.IsTrue(result.MessagesBase64.Count == 1 && result.MessageUTF8.Count == 1 && result.Success.Equals(true));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Data.Equals("VGVzdCBtZXNzYWdlIDA=") && result.MessageUTF8.Any(x => x.Data.Equals("Test message 0"))));

        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("custom header"))));

        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("custom header"))));
    }

    /// <summary>
    /// Connect with hostname and read multiple messages.
    /// </summary>
    [TestMethod]
    public void TestReadMultipleMessagesWithHostAutoReject()
    {
        Connection connection = new()
        {
            Host = TestHost,
            Username = "agent",
            Password = "agent123",
            RoutingKey = "queue",
            QueueName = "queue",
            AuthenticationMethod = AuthenticationMethod.Host,
            ExchangeName = null,

            AutoAck = ReadAckType.AutoReject,
            ReadMessageCount = 2,
        };

        Publish(connection, 2);
        var result = RabbitMQ.Read(connection);

        var test1 = result.MessagesBase64.Count;
        var test2 = result.MessageUTF8.Count;


        Assert.IsTrue(result.MessagesBase64.Count > 1 && result.MessageUTF8.Count > 1 && result.Success.Equals(true));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Data.Equals("VGVzdCBtZXNzYWdlIDA=") && result.MessageUTF8.Any(x => x.Data.Equals("Test message 0")))
            && result.MessagesBase64.Any(x => x.Data.Equals("VGVzdCBtZXNzYWdlIDE=") && result.MessageUTF8.Any(x => x.Data.Equals("Test message 1"))));

        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("custom header"))));

        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("custom header"))));
    }

    /// <summary>
    /// Connect with URI and read single message.
    /// </summary>
    [TestMethod]
    public void TestReadSingleMessageWithURIAutoReject()
    {
        Connection connection = new()
        {
            Host = TestUri,
            RoutingKey = "queue",
            QueueName = "queue",
            AuthenticationMethod = AuthenticationMethod.URI,
            ExchangeName = null,

            AutoAck = ReadAckType.AutoReject,
            ReadMessageCount = 1,
        };

        Publish(connection, 1);
        var result = RabbitMQ.Read(connection);

        Assert.IsTrue(result.MessagesBase64.Count == 1 && result.MessageUTF8.Count == 1 && result.Success.Equals(true));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Data.Equals("VGVzdCBtZXNzYWdlIDA=") && result.MessageUTF8.Any(x => x.Data.Equals("Test message 0"))));

        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("custom header"))));

        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("custom header"))));
    }

    [TestMethod]
    public void TestReadSingleMessageWithHostAutoRejectAndRequeue()
    {
        Connection connection = new()
        {
            Host = TestHost,
            Username = "agent",
            Password = "agent123",
            RoutingKey = "queue",
            QueueName = "queue",
            AuthenticationMethod = AuthenticationMethod.Host,
            ExchangeName = null,

            AutoAck = ReadAckType.AutoRejectAndRequeue,
            ReadMessageCount = 1,
        };

        Publish(connection, 1);
        var result = RabbitMQ.Read(connection);

        Assert.IsTrue(result.MessagesBase64.Count == 1 && result.MessageUTF8.Count == 1 && result.Success.Equals(true));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Data.Equals("VGVzdCBtZXNzYWdlIDA=") && result.MessageUTF8.Any(x => x.Data.Equals("Test message 0"))));

        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("custom header"))));

        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("custom header"))));
    }

    /// <summary>
    /// Connect with hostname and read multiple messages.
    /// </summary>
    [TestMethod]
    public void TestReadMultipleMessagesWithHostAutoRejectAndRequeue()
    {
        Connection connection = new()
        {
            Host = TestHost,
            Username = "agent",
            Password = "agent123",
            RoutingKey = "queue",
            QueueName = "queue",
            AuthenticationMethod = AuthenticationMethod.Host,
            ExchangeName = null,

            AutoAck = ReadAckType.AutoRejectAndRequeue,
            ReadMessageCount = 2,
        };

        Publish(connection, 2);
        var result = RabbitMQ.Read(connection);

        var test1 = result.MessagesBase64.Count;
        var test2 = result.MessageUTF8.Count;


        Assert.IsTrue(result.MessagesBase64.Count > 1 && result.MessageUTF8.Count > 1 && result.Success.Equals(true));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Data.Equals("VGVzdCBtZXNzYWdlIDA=") && result.MessageUTF8.Any(x => x.Data.Equals("Test message 0")))
            && result.MessagesBase64.Any(x => x.Data.Equals("VGVzdCBtZXNzYWdlIDE=") && result.MessageUTF8.Any(x => x.Data.Equals("Test message 1"))));

        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("custom header"))));

        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("custom header"))));
    }

    /// <summary>
    /// Connect with URI and read single message.
    /// </summary>
    [TestMethod]
    public void TestReadSingleMessageWithURIAutoRejectAndRequeue()
    {
        Connection connection = new()
        {
            Host = TestUri,
            RoutingKey = "queue",
            QueueName = "queue",
            AuthenticationMethod = AuthenticationMethod.URI,
            ExchangeName = null,

            AutoAck = ReadAckType.AutoRejectAndRequeue,
            ReadMessageCount = 1,
        };

        Publish(connection, 1);
        var result = RabbitMQ.Read(connection);

        Assert.IsTrue(result.MessagesBase64.Count == 1 && result.MessageUTF8.Count == 1 && result.Success.Equals(true));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Data.Equals("VGVzdCBtZXNzYWdlIDA=") && result.MessageUTF8.Any(x => x.Data.Equals("Test message 0"))));

        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessagesBase64.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessagesBase64.Any(x => x.Headers.ContainsValue("custom header"))));

        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("application id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-AppId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("cluster id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Type") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content type"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Content-Encoding") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("content encoding"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-CorrelationId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("correlation id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-Expiration") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("100"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("X-MessageId") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("message id"))));
        Assert.IsTrue(result.MessageUTF8.Any(x => x.Headers.ContainsKey("Custom-Header") && result.MessageUTF8.Any(x => x.Headers.ContainsValue("custom header"))));
    }

    public static void Publish(Connection connection, int messageCount, Dictionary<string, object>? args = null)
    {
        ConnectionHelper connectionHelper = new();
        var message = "Test message";

        Helper.OpenConnectionIfClosed(connectionHelper, connection);

        connectionHelper.AMQPModel.QueueDeclare(queue: connection.QueueName,
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: args);

        var basicProperties = connectionHelper.AMQPModel.CreateBasicProperties();
        basicProperties.Persistent = false;

        var headers = new Dictionary<string, object>() {
                { "X-AppId", "application id" },
                { "X-ClusterId", "cluster id" },
                { "Content-Type", "content type" },
                { "Content-Encoding", "content encoding" },
                { "X-CorrelationId", "correlation id" },
                { "X-Expiration", "100" },
                { "X-MessageId", "message id" },
                { "Custom-Header", "custom header" }
        };

        basicProperties.Headers = headers;

        for (var i = 0; i < messageCount; i++)
            connectionHelper.AMQPModel.BasicPublish(exchange: "exchange",
                routingKey: connection.RoutingKey,
                basicProperties: basicProperties,
                body: Encoding.UTF8.GetBytes(message + " " + i));
    }
}