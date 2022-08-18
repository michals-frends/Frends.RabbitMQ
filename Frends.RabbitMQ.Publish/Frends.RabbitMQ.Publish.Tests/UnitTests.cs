using Frends.RabbitMQ.Publish.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using System.Text;

namespace Frends.RabbitMQ.Publish.Tests;

[TestClass]
public class UnitTests
{
    /// <summary>
    /// You will need access to RabbitMQ queue, you can create it e.g. by running
    /// docker run -d --hostname my-rabbit -p 5672:5672 -p 8080:1567 -e RABBITMQ_DEFAULT_USER=agent -e RABBITMQ_DEFAULT_PASS=agent123  rabbitmq:3.7-management
    /// In that case URI would be amqp://agent:agent123@localhost:5672
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
    /// Connect with hostname and publish as string.
    /// </summary>
    [TestMethod]
    public void TestPublishAsString()
    {
        Connection connection = new()
        {
            Host = TestHost,
            Username = "agent",
            Password = "agent123",
            RoutingKey = "queue",
            QueueName = "queue",
            Create = false,
            Durable = false,
            AutoDelete = false,
            AuthenticationMethod = AuthenticationMethod.Host,
            ExchangeName = ""
        };

        Input input = new()
        {
            DataString = "test message",
            InputType = InputType.String,

            Headers = new Header[] {
                new Header { Name = "X-AppId", Value = "application id" },
                new Header { Name = "X-ClusterId", Value = "cluster id" },
                new Header { Name = "Content-Type", Value = "content type" },
                new Header { Name = "Content-Encoding", Value = "content encoding" },
                new Header { Name = "X-CorrelationId", Value = "correlation id" },
                new Header { Name = "X-Expiration", Value = "100" },
                new Header { Name = "X-MessageId", Value = "message id" },
                new Header { Name = "Custom-Header", Value = "custom header" }
            }
        };

        var readValues = new ReadValues();
        var result = RabbitMQ.Publish(input, connection);
        ReadMessage(readValues, connection);

        Assert.IsTrue(!string.IsNullOrEmpty(readValues.Message) && readValues.Message.Equals("test message"));
        Assert.IsTrue(result.DataFormat.Equals("String")
            && result.DataString.Equals("test message")
            && result.DataByteArray.SequenceEqual(Encoding.UTF8.GetBytes("test message")));
        Assert.IsTrue(readValues.Headers.ContainsKey("X-AppId") && readValues.Headers.ContainsValue("application id")
            && result.Headers.ContainsKey("X-AppId") && result.Headers.ContainsValue("application id"));
        Assert.IsTrue(readValues.Headers.ContainsKey("X-ClusterId") && readValues.Headers.ContainsValue("cluster id")
            && result.Headers.ContainsKey("X-ClusterId") && result.Headers.ContainsValue("cluster id"));
        Assert.IsTrue(readValues.Headers.ContainsKey("Content-Type") && readValues.Headers.ContainsValue("content type")
            && result.Headers.ContainsKey("Content-Type") && result.Headers.ContainsValue("content type"));
        Assert.IsTrue(readValues.Headers.ContainsKey("Content-Encoding") && readValues.Headers.ContainsValue("content encoding")
            && result.Headers.ContainsKey("Content-Encoding") && result.Headers.ContainsValue("content encoding"));
        Assert.IsTrue(readValues.Headers.ContainsKey("X-CorrelationId") && readValues.Headers.ContainsValue("correlation id")
            && result.Headers.ContainsKey("X-CorrelationId") && result.Headers.ContainsValue("correlation id"));
        Assert.IsTrue(readValues.Headers.ContainsKey("X-Expiration") && readValues.Headers.ContainsValue("100")
            && result.Headers.ContainsKey("X-Expiration") && result.Headers.ContainsValue("100"));
        Assert.IsTrue(readValues.Headers.ContainsKey("X-MessageId") && readValues.Headers.ContainsValue("message id")
            && result.Headers.ContainsKey("X-MessageId") && result.Headers.ContainsValue("message id"));
        Assert.IsTrue(readValues.Headers.ContainsKey("Custom-Header") && readValues.Headers.ContainsValue("custom header")
            && result.Headers.ContainsKey("Custom-Header") && result.Headers.ContainsValue("custom header"));
    }

    /// <summary>
    /// Connect with hostname and publish as byte array.
    /// </summary>
    [TestMethod]
    public void TestPublishAsByteArray()
    {
        Connection connection = new()
        {
            Host = TestHost,
            Username = "agent",
            Password = "agent123",
            RoutingKey = "queue",
            QueueName = "queue",
            Create = false,
            Durable = false,
            AutoDelete = false,
            AuthenticationMethod = AuthenticationMethod.Host,
            ExchangeName = ""
        };

        Input input = new()
        {
            DataByteArray = Encoding.UTF8.GetBytes("test message"),
            InputType = InputType.ByteArray,

            Headers = new Header[] {
                new Header { Name = "X-AppId", Value = "application id" },
                new Header { Name = "X-ClusterId", Value = "cluster id" },
                new Header { Name = "Content-Type", Value = "content type" },
                new Header { Name = "Content-Encoding", Value = "content encoding" },
                new Header { Name = "X-CorrelationId", Value = "correlation id" },
                new Header { Name = "X-Expiration", Value = "100" },
                new Header { Name = "X-MessageId", Value = "message id" },
                new Header { Name = "Custom-Header", Value = "custom header" }
            }
        };

        var readValues = new ReadValues();
        var result = RabbitMQ.Publish(input, connection);
        ReadMessage(readValues, connection);

        Assert.IsTrue(!string.IsNullOrEmpty(readValues.Message) && readValues.Message.Equals("test message"));
        Assert.IsTrue(result.DataFormat.Equals("ByteArray")
            && result.DataString.Equals("test message")
            && result.DataByteArray.SequenceEqual(Encoding.UTF8.GetBytes("test message")));
        Assert.IsTrue(readValues.Headers.ContainsKey("X-AppId") && readValues.Headers.ContainsValue("application id")
            && result.Headers.ContainsKey("X-AppId") && result.Headers.ContainsValue("application id"));
        Assert.IsTrue(readValues.Headers.ContainsKey("X-ClusterId") && readValues.Headers.ContainsValue("cluster id")
            && result.Headers.ContainsKey("X-ClusterId") && result.Headers.ContainsValue("cluster id"));
        Assert.IsTrue(readValues.Headers.ContainsKey("Content-Type") && readValues.Headers.ContainsValue("content type")
            && result.Headers.ContainsKey("Content-Type") && result.Headers.ContainsValue("content type"));
        Assert.IsTrue(readValues.Headers.ContainsKey("Content-Encoding") && readValues.Headers.ContainsValue("content encoding")
            && result.Headers.ContainsKey("Content-Encoding") && result.Headers.ContainsValue("content encoding"));
        Assert.IsTrue(readValues.Headers.ContainsKey("X-CorrelationId") && readValues.Headers.ContainsValue("correlation id")
            && result.Headers.ContainsKey("X-CorrelationId") && result.Headers.ContainsValue("correlation id"));
        Assert.IsTrue(readValues.Headers.ContainsKey("X-Expiration") && readValues.Headers.ContainsValue("100")
            && result.Headers.ContainsKey("X-Expiration") && result.Headers.ContainsValue("100"));
        Assert.IsTrue(readValues.Headers.ContainsKey("X-MessageId") && readValues.Headers.ContainsValue("message id")
            && result.Headers.ContainsKey("X-MessageId") && result.Headers.ContainsValue("message id"));
        Assert.IsTrue(readValues.Headers.ContainsKey("Custom-Header") && readValues.Headers.ContainsValue("custom header")
            && result.Headers.ContainsKey("Custom-Header") && result.Headers.ContainsValue("custom header"));
    }

    /// <summary>
    /// Connect with URI and publish as string.
    /// </summary>
    [TestMethod]
    public void TestURIConnection()
    {
        Connection connection = new()
        {
            Host = TestUri,
            RoutingKey = "queue",
            QueueName = "queue",
            Create = false,
            Durable = false,
            AutoDelete = false,
            AuthenticationMethod = AuthenticationMethod.URI,
        };

        Input input = new()
        {
            DataString = "test message",
            InputType = InputType.String,

            Headers = new Header[] {
                new Header { Name = "X-AppId", Value = "application id" },
                new Header { Name = "X-ClusterId", Value = "cluster id" },
                new Header { Name = "Content-Type", Value = "content type" },
                new Header { Name = "Content-Encoding", Value = "content encoding" },
                new Header { Name = "X-CorrelationId", Value = "correlation id" },
                new Header { Name = "X-Expiration", Value = "100" },
                new Header { Name = "X-MessageId", Value = "message id" },
                new Header { Name = "Custom-Header", Value = "custom header" }
            }
        };

        var readValues = new ReadValues();
        var result = RabbitMQ.Publish(input, connection);
        ReadMessage(readValues, connection);

        Assert.IsTrue(!string.IsNullOrEmpty(readValues.Message) && readValues.Message.Equals("test message"));
        Assert.IsTrue(result.DataFormat.Equals("String")
            && result.DataString.Equals("test message")
            && result.DataByteArray.SequenceEqual(Encoding.UTF8.GetBytes("test message")));
        Assert.IsTrue(readValues.Headers.ContainsKey("X-AppId") && readValues.Headers.ContainsValue("application id")
            && result.Headers.ContainsKey("X-AppId") && result.Headers.ContainsValue("application id"));
        Assert.IsTrue(readValues.Headers.ContainsKey("X-ClusterId") && readValues.Headers.ContainsValue("cluster id")
            && result.Headers.ContainsKey("X-ClusterId") && result.Headers.ContainsValue("cluster id"));
        Assert.IsTrue(readValues.Headers.ContainsKey("Content-Type") && readValues.Headers.ContainsValue("content type")
            && result.Headers.ContainsKey("Content-Type") && result.Headers.ContainsValue("content type"));
        Assert.IsTrue(readValues.Headers.ContainsKey("Content-Encoding") && readValues.Headers.ContainsValue("content encoding")
            && result.Headers.ContainsKey("Content-Encoding") && result.Headers.ContainsValue("content encoding"));
        Assert.IsTrue(readValues.Headers.ContainsKey("X-CorrelationId") && readValues.Headers.ContainsValue("correlation id")
            && result.Headers.ContainsKey("X-CorrelationId") && result.Headers.ContainsValue("correlation id"));
        Assert.IsTrue(readValues.Headers.ContainsKey("X-Expiration") && readValues.Headers.ContainsValue("100")
            && result.Headers.ContainsKey("X-Expiration") && result.Headers.ContainsValue("100"));
        Assert.IsTrue(readValues.Headers.ContainsKey("X-MessageId") && readValues.Headers.ContainsValue("message id")
            && result.Headers.ContainsKey("X-MessageId") && result.Headers.ContainsValue("message id"));
        Assert.IsTrue(readValues.Headers.ContainsKey("Custom-Header") && readValues.Headers.ContainsValue("custom header")
            && result.Headers.ContainsKey("Custom-Header") && result.Headers.ContainsValue("custom header"));
    }

    private static void ReadMessage(ReadValues readValues, Connection connection)
    {
        var factory = new ConnectionFactory();

        switch (connection.AuthenticationMethod)
        {
            case AuthenticationMethod.URI:
                factory.Uri = new Uri(connection.Host);
                break;
            case AuthenticationMethod.Host:
                if (!string.IsNullOrWhiteSpace(connection.Username) || !string.IsNullOrWhiteSpace(connection.Password))
                {
                    factory.UserName = connection.Username;
                    factory.Password = connection.Password;
                }
                factory.HostName = connection.Host;
                break;
        }

        IConnection _connection = factory.CreateConnection();
        IModel _model = _connection.CreateModel();

        var rcvMessage = _model.BasicGet(connection.QueueName, true);
        if (rcvMessage != null)
        {
            var message = Encoding.UTF8.GetString(rcvMessage.Body.ToArray());
            readValues.Message = message;
            readValues.Tag = rcvMessage.DeliveryTag;

            var data = new Dictionary<string, string>();
            foreach (var head in rcvMessage.BasicProperties.Headers)
            {
                var convert = Encoding.UTF8.GetString((byte[])head.Value);
                data.TryAdd(head.Key, convert);
            }
            readValues.Headers = data;
        }
    }

    internal class ReadValues
    {
        public string Message { get; set; } = "";
        public ulong Tag { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    }
}