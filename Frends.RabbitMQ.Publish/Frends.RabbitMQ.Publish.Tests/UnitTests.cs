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
    /// docker run -d --hostname my-rabbit -p 5672:5672 -p 8080:1567 -p 15672:15672 -e RABBITMQ_DEFAULT_USER=agent -e RABBITMQ_DEFAULT_PASS=agent123 rabbitmq:3.9-management
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

        var readValues = new Helper.ReadValues();
        var result = RabbitMQ.Publish(input, connection);
        Helper.ReadMessage(readValues, connection);

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

        var readValues = new Helper.ReadValues();
        var result = RabbitMQ.Publish(input, connection);
        Helper.ReadMessage(readValues, connection);

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

    [TestMethod]
    public void TestPublishAsString_WithoutHeaders()
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
            Headers = null
        };

        var readValues = new Helper.ReadValues();
        var result = RabbitMQ.Publish(input, connection);
        Helper.ReadMessage(readValues, connection);
        Assert.IsNotNull(readValues.Message);
        Assert.AreEqual("test message", readValues.Message);
        Assert.AreEqual("String", result.DataFormat);
        Assert.AreEqual("test message", result.DataString);
        Assert.IsTrue(result.DataByteArray.SequenceEqual(Encoding.UTF8.GetBytes("test message")));
        Assert.AreEqual(0, result.Headers.Count);
    }

    [TestMethod]
    public void TestPublishAsByteArray_WithoutHeaders()
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
            Headers = null
        };

        var readValues = new Helper.ReadValues();
        var result = RabbitMQ.Publish(input, connection);
        Helper.ReadMessage(readValues, connection);
        Assert.IsNotNull(readValues.Message);
        Assert.AreEqual("test message", readValues.Message);
        Assert.AreEqual("ByteArray", result.DataFormat);
        Assert.AreEqual("test message", result.DataString);
        Assert.IsTrue(result.DataByteArray.SequenceEqual(Encoding.UTF8.GetBytes("test message")));
    }

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

        var readValues = new Helper.ReadValues();
        var result = RabbitMQ.Publish(input, connection);
        Helper.ReadMessage(readValues, connection);

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
}