using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Frends.RabbitMQ.Publish.Definitions;
using RabbitMQ.Client;

namespace Frends.RabbitMQ.Publish;

/// <summary>
/// RabbitMQ publish task.
/// </summary>
public class RabbitMQ
{
    /// <summary>
    /// Publish message to RabbitMQ queue in UTF8 or byte array format.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.RabbitMQ.Publish)
    /// </summary>
    /// <param name="input">Input parameters</param>
    /// <param name="connection">Connection parameters.</param>
    /// <returns>Object { string DataFormat, string DataString, byte[] DataByteArray, Dictionary&lt;string, string&gt; Headers }</returns>
    public static Result Publish([PropertyTab] Input input, [PropertyTab] Connection connection)
    {
        using var connectionHelper = new ConnectionHelper();

        var dataType = input.InputType.Equals(InputType.ByteArray) ? "ByteArray" : "String";
        var data = input.InputType.Equals(InputType.ByteArray) ? input.DataByteArray : Encoding.UTF8.GetBytes(input.DataString);
        if (data.Length == 0) throw new ArgumentException("Publish: Message data is missing.");

        OpenConnectionIfClosed(connectionHelper, connection);

        if (connection.Create)
        {
            // Create args dictionary for quorum queue arguments
            var args =  new Dictionary<string, object>();
            args.Add("x-queue-type", "quorum");

            connectionHelper.AMQPModel.QueueDeclare(queue: connection.QueueName,
                durable: connection.Durable,
                exclusive: false,
                autoDelete: connection.AutoDelete,
                arguments: connection.Quorum ? args : null);

            if (!string.IsNullOrEmpty(connection.ExchangeName))
            {
                connectionHelper.AMQPModel.QueueBind(queue: connection.QueueName,
                    exchange: connection.ExchangeName,
                    routingKey: connection.RoutingKey,
                    arguments: null);
            }
        }

        var basicProperties = connectionHelper.AMQPModel.CreateBasicProperties();
        basicProperties.Persistent = connection.Durable;
        AddHeadersToBasicProperties(basicProperties, input.Headers);

        var headers = new Dictionary<string, string>();

        if (basicProperties.Headers != null)
            foreach (var head in basicProperties.Headers)
                headers.Add(head.Key.ToString(), head.Value.ToString());

        connectionHelper.AMQPModel.BasicPublish(exchange: connection.ExchangeName,
            routingKey: connection.RoutingKey,
            basicProperties: basicProperties,
            body: data);

        return new Result(dataType,
            !string.IsNullOrEmpty(input.DataString) ? input.DataString : Encoding.UTF8.GetString(input.DataByteArray),
            input.DataByteArray ?? Encoding.UTF8.GetBytes(input.DataString),
            headers);
    }

    private static void OpenConnectionIfClosed(ConnectionHelper connectionHelper, Connection connection)
    {
        // Close connection if hostname has changed.
        if (IsConnectionHostNameChanged(connectionHelper, connection))
        {
            connectionHelper.AMQPModel.Close();
            connectionHelper.AMQPConnection.Close();
        }

        if (connectionHelper.AMQPConnection == null || connectionHelper.AMQPConnection.IsOpen == false)
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

                    if (connection.Port != 0) factory.Port = connection.Port;

                    break;
            }

            if (connection.Timeout != 0) factory.RequestedConnectionTimeout = TimeSpan.FromSeconds(connection.Timeout);

            connectionHelper.AMQPConnection = factory.CreateConnection();
        }

        if (connectionHelper.AMQPModel == null || connectionHelper.AMQPModel.IsClosed)
            connectionHelper.AMQPModel = connectionHelper.AMQPConnection.CreateModel();
    }

    private static bool IsConnectionHostNameChanged(ConnectionHelper connectionHelper, Connection connection)
    {
        // If no current connection, host name is not changed
        if (connectionHelper.AMQPConnection == null || connectionHelper.AMQPConnection.IsOpen == false)
            return false;

        switch (connection.AuthenticationMethod)
        {
            case AuthenticationMethod.URI:
                var newUri = new Uri(connection.Host);
                return (!connectionHelper.AMQPConnection.Endpoint.HostName.Equals(newUri.Host));
            case AuthenticationMethod.Host:
                return (!connectionHelper.AMQPConnection.Endpoint.HostName.Equals(connection.Host));
            default:
                throw new ArgumentException($"IsConnectionHostNameChanged: AuthenticationMethod missing.");
        }
    }

    private static void AddHeadersToBasicProperties(IBasicProperties basicProperties, Header[] headers)
    {
        if (headers == null) return;

        var messageHeaders = new Dictionary<string, object>();

        headers.ToList().ForEach(header =>
        {
            switch (header.Name.ToUpper())
            {
                case "APPID":
                case "HEADER_APPID":
                case "HEADER.APPID":
                    basicProperties.AppId = header.Value;
                    break;

                case "CLUSTERID":
                case "HEADER_CLUSTERID":
                case "HEADER.CLUSTERID":
                    basicProperties.ClusterId = header.Value;
                    break;

                case "CONTENTENCODING":
                case "HEADER_CONTENTENCODING":
                case "HEADER.CONTENTENCODING":
                    basicProperties.ContentEncoding = header.Value;
                    break;

                case "CONTENTTYPE":
                case "HEADER_CONTENTTYPE":
                case "HEADER.CONTENTTYPE":
                    basicProperties.ContentType = header.Value;
                    break;

                case "CORRELATIONID":
                case "HEADER_CORRELATIONID":
                case "HEADER.CORRELATIONID":
                    basicProperties.CorrelationId = header.Value;
                    break;

                case "EXPIRATION":
                case "HEADER_EXPIRATION":
                case "HEADER.EXPIRATION":
                    basicProperties.Expiration = header.Value;
                    break;

                case "MESSAGEID":
                case "HEADER_MESSAGEID":
                case "HEADER.MESSAGEID":
                    basicProperties.MessageId = header.Value;
                    break;

                default:
                    messageHeaders.Add(header.Name, header.Value);
                    break;
            }
        });

        if (messageHeaders.Any())
            basicProperties.Headers = messageHeaders;
    }
}