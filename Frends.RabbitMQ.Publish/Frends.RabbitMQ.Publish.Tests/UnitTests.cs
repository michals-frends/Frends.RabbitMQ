using System.Collections.Concurrent;
using Frends.RabbitMQ.Publish.Definitions;
using Frends.RabbitMQ.Publish.Tests.Lib;
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

    private const string _testUri = "amqp://agent:agent123@localhost:5672";
    private const string _testHost = "localhost";
    private const string _queue = "quorum";
    private const string _exchange = "exchange";
    private static Header[] _headers = new Header[0];

    [TestInitialize]
    public void CreateExchangeAndQueue()
    {
        var factory = new ConnectionFactory { Uri = new Uri(_testUri) };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.ExchangeDeclare(_exchange, type: "fanout", durable: false, autoDelete: false);
        channel.QueueDeclare(_queue, durable: false, exclusive: false, autoDelete: false);
        channel.QueueBind(_queue, _exchange, routingKey: "");

        _headers = new Header[] {
            new Header { Name = "X-AppId", Value = "application id" },
            new Header { Name = "X-ClusterId", Value = "cluster id" },
            new Header { Name = "Content-Type", Value = "content type" },
            new Header { Name = "Content-Encoding", Value = "content encoding" },
            new Header { Name = "X-CorrelationId", Value = "correlation id" },
            new Header { Name = "X-Expiration", Value = "100" },
            new Header { Name = "X-MessageId", Value = "message id" },
            new Header { Name = "Custom-Header", Value = "custom header" }
        };
    }

    [TestCleanup]
    public void DeleteExchangeAndQueue()
    {
        var factory = new ConnectionFactory { Uri = new Uri(_testUri) };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDelete(_queue, false, false);
        channel.ExchangeDelete(_exchange, ifUnused: false);
    }

    [TestMethod]
    public void TestPublishAsString()
    {
        Connection connection = new()
        {
            Host = _testHost,
            Username = "agent",
            Password = "agent123",
            RoutingKey = _queue,
            QueueName = _queue,
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
            Headers = _headers
        };

        var readValues = new Helper.ReadValues();
        var result = RabbitMQ.Publish(input, connection);
        Helper.ReadMessage(readValues, connection);

        Assert.IsTrue(!string.IsNullOrEmpty(readValues.Message));
        Assert.AreEqual("test message", readValues.Message);
        Assert.AreEqual("String", result.DataFormat);
        Assert.AreEqual("test message", result.DataString);
        Assert.IsTrue(result.DataByteArray.SequenceEqual(Encoding.UTF8.GetBytes("test message")));
        Assert.IsTrue(readValues.Headers.ContainsKey("X-AppId"));
        Assert.IsTrue(readValues.Headers.ContainsValue("application id"));
        Assert.IsTrue(result.Headers.ContainsKey("X-AppId"));
        Assert.IsTrue(result.Headers.ContainsValue("application id"));
        Assert.IsTrue(readValues.Headers.ContainsKey("X-ClusterId"));
        Assert.IsTrue(readValues.Headers.ContainsValue("cluster id"));
        Assert.IsTrue(result.Headers.ContainsKey("X-ClusterId"));
        Assert.IsTrue(result.Headers.ContainsValue("cluster id"));
        Assert.IsTrue(readValues.Headers.ContainsKey("Content-Type"));
        Assert.IsTrue(readValues.Headers.ContainsValue("content type"));
        Assert.IsTrue(result.Headers.ContainsKey("Content-Type"));
        Assert.IsTrue(result.Headers.ContainsValue("content type"));
        Assert.IsTrue(readValues.Headers.ContainsKey("Content-Encoding"));
        Assert.IsTrue(readValues.Headers.ContainsValue("content encoding"));
        Assert.IsTrue(result.Headers.ContainsKey("Content-Encoding"));
        Assert.IsTrue(result.Headers.ContainsValue("content encoding"));
        Assert.IsTrue(readValues.Headers.ContainsKey("X-CorrelationId"));
        Assert.IsTrue(readValues.Headers.ContainsValue("correlation id"));
        Assert.IsTrue(result.Headers.ContainsKey("X-CorrelationId"));
        Assert.IsTrue(result.Headers.ContainsValue("correlation id"));
        Assert.IsTrue(readValues.Headers.ContainsKey("X-Expiration"));
        Assert.IsTrue(readValues.Headers.ContainsValue("100"));
        Assert.IsTrue(result.Headers.ContainsKey("X-Expiration"));
        Assert.IsTrue(result.Headers.ContainsValue("100"));
        Assert.IsTrue(readValues.Headers.ContainsKey("X-MessageId"));
        Assert.IsTrue(readValues.Headers.ContainsValue("message id"));
        Assert.IsTrue(result.Headers.ContainsKey("X-MessageId"));
        Assert.IsTrue(result.Headers.ContainsValue("message id"));
        Assert.IsTrue(readValues.Headers.ContainsKey("Custom-Header"));
        Assert.IsTrue(readValues.Headers.ContainsValue("custom header"));
        Assert.IsTrue(result.Headers.ContainsKey("Custom-Header"));
        Assert.IsTrue(result.Headers.ContainsValue("custom header"));
    }

    [TestMethod]
    public void TestPublishAsByteArray()
    {
        Connection connection = new()
        {
            Host = _testHost,
            Username = "agent",
            Password = "agent123",
            RoutingKey = _queue,
            QueueName = _queue,
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
            Headers = _headers
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
    public void TestPublishAsString_WithoutHeaders()
    {
        Connection connection = new()
        {
            Host = _testHost,
            Username = "agent",
            Password = "agent123",
            RoutingKey = _queue,
            QueueName = _queue,
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
            Host = _testHost,
            Username = "agent",
            Password = "agent123",
            RoutingKey = _queue,
            QueueName = _queue,
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
            Host = _testUri,
            RoutingKey = _queue,
            QueueName = _queue,
            Create = false,
            Durable = false,
            AutoDelete = false,
            AuthenticationMethod = AuthenticationMethod.URI,
        };

        Input input = new()
        {
            DataString = "test message",
            InputType = InputType.String,
            Headers = _headers
        };

        var readValues = new Helper.ReadValues();
        var result = RabbitMQ.Publish(input, connection);
        Helper.ReadMessage(readValues, connection);

        Assert.IsNotNull(readValues.Message);
        Assert.AreEqual("test message", readValues.Message);
        Assert.AreEqual("String", result.DataFormat);
        Assert.AreEqual("test message", result.DataString);
        Assert.IsTrue(result.DataByteArray.SequenceEqual(Encoding.UTF8.GetBytes("test message")));
    }
    
    [TestMethod]
    public async Task TestBurstURIConnection()
    {
        var counter = 30;
        
        Connection connection = new()
        {
            Host = _testUri,
            RoutingKey = _queue,
            QueueName = _queue,
            Create = false,
            Durable = false,
            AutoDelete = false,
            AuthenticationMethod = AuthenticationMethod.URI,
        };

        var progress = new ConcurrentDictionary<int, (bool success, Exception? exception)>();
        
        var tasks = Enumerable.Range(0, counter)
            .Select(count => Task.Factory.StartNew(() =>
            {
                try
                {
                    Input input = new() { DataString = "test message", InputType = InputType.String, Headers = _headers };

                    var readValues = new Helper.ReadValues();
                    var result = RabbitMQ.Publish(input, connection);
                    Helper.ReadMessageFactory(readValues, connection);

                    Assert.IsNotNull(readValues.Message);
                    Assert.AreEqual("test message", readValues.Message);
                    Assert.AreEqual("String", result.DataFormat);
                    Assert.AreEqual("test message", result.DataString);
                    Assert.IsTrue(result.DataByteArray.SequenceEqual(Encoding.UTF8.GetBytes("test message")));

                    progress[count] = (true, null);
                }
                catch (Exception e)
                {
                    progress[count] = (false, e);
                }
            }))
            .ToList();

        await Task.WhenAll(tasks);

        Assert.IsTrue(progress.Count == counter, "Number of tasks were not completed");
        Assert.IsTrue(progress.All(p => p.Value.success),
            $@"Not all tasks were completed successfully: {string.Join(", ",progress.Where(pair => pair.Value.exception != null).Select(pair => pair.Value.exception!.Message) ) }");
    }
    
    [TestMethod]
    public async Task TestBurstForURIConnection()
    {
        var counter = 30;
        
        Connection connection = new()
        {
            Host = _testUri,
            RoutingKey = _queue,
            QueueName = _queue,
            Create = false,
            Durable = false,
            AutoDelete = false,
            AuthenticationMethod = AuthenticationMethod.URI,
        };

        var progress = new ConcurrentDictionary<int, (bool success, Exception? exception)>();
        
        foreach (var count in Enumerable.Range(0, counter))
        {
            try
            {
                Input input = new() { DataString = "test message", InputType = InputType.String, Headers = _headers };

                //var readValues = new Helper.ReadValues();
                var result = RabbitMQ.Publish(input, connection);
                //Helper.ReadMessage(readValues, connection);

                //Assert.IsNotNull(readValues.Message);
                //Assert.AreEqual("test message", readValues.Message);
                //Assert.AreEqual("String", result.DataFormat);
                //Assert.AreEqual("test message", result.DataString);
                //Assert.IsTrue(result.DataByteArray.SequenceEqual(Encoding.UTF8.GetBytes("test message")));

                progress[count] = (true, null);
            }
            catch (Exception e)
            {
                progress[count] = (false, e);
            }
        }

        Assert.IsTrue(progress.Count == counter, "Number of tasks were not completed");
        Assert.IsTrue(progress.All(p => p.Value.success),
            $@"Not all tasks were completed successfully: {string.Join(", ",progress.Where(pair => pair.Value.exception != null).Select(pair => pair.Value.exception!.Message) ) }");
    }
}