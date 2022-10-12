using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Frends.RabbitMQ.Read.Definitions;
using RabbitMQ.Client;

namespace Frends.RabbitMQ.Read;

/// <summary>
/// RabbitMQ Read task.
/// </summary>
public class RabbitMQ
{
    /// <summary>
    /// Read message(s) from RabbitMQ queue. Message data is byte[] encoded to base64 and UTF8 strings.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.RabbitMQ.Read)
    /// </summary>
    /// <param name="connection">Connection parameters.</param>
    /// <returns>Object { bool Success, Object { string Data, Dictionary&lt;string, string&gt; Headers, uint MessagesCount, ulong DeliveryTag } MessagesBase64, Object { string Data, Dictionary&lt;string, string&gt; Headers, uint MessagesCount, ulong DeliveryTag } MessageUTF8 }</returns>
    public static Result Read([PropertyTab] Connection connection)
    {
        var connectionHelper = new ConnectionHelper();
        var baseList = new List<Message>();
        var stringList = new List<Message>();

        OpenConnectionIfClosed(connectionHelper, connection);

        while (connection.ReadMessageCount-- > 0)
        {
            var rcvMessage = connectionHelper.AMQPModel.BasicGet(queue: connection.QueueName, autoAck: connection.AutoAck == ReadAckType.AutoAck);
            if (rcvMessage != null)
            {
                baseList.Add(new Message
                {
                    Data = Convert.ToBase64String(rcvMessage.Body.ToArray()),
                    Headers = GetResponseHeaderDictionary(rcvMessage.BasicProperties),
                    MessagesCount = rcvMessage.MessageCount,
                    DeliveryTag = rcvMessage.DeliveryTag
                });

                stringList.Add(new Message
                {
                    Data = Encoding.UTF8.GetString(rcvMessage.Body.ToArray()),
                    Headers = GetResponseHeaderDictionary(rcvMessage.BasicProperties),
                    MessagesCount = rcvMessage.MessageCount,
                    DeliveryTag = rcvMessage.DeliveryTag
                });
            }
            else
                break;
        }

        // Auto acking.
        if (connection.AutoAck != ReadAckType.AutoAck && connection.AutoAck != ReadAckType.ManualAck)
        {
            var ackType = ManualAckType.NackAndRequeue;

            switch (connection.AutoAck)
            {
                case ReadAckType.AutoNack:
                    ackType = ManualAckType.Nack;
                    break;

                case ReadAckType.AutoNackAndRequeue:
                    ackType = ManualAckType.NackAndRequeue;
                    break;


                case ReadAckType.AutoReject:
                    ackType = ManualAckType.Reject;
                    break;

                case ReadAckType.AutoRejectAndRequeue:
                    ackType = ManualAckType.RejectAndRequeue;
                    break;
            }

            foreach (var message in baseList)
                AcknowledgeMessage(ackType, message.DeliveryTag, connectionHelper);
        }

        return new Result(true, baseList, stringList);
    }

    private static bool AcknowledgeMessage(ManualAckType ackType, ulong deliveryTag, ConnectionHelper connectionHelper)
    {
        if (connectionHelper == null || connectionHelper.AMQPModel.IsClosed)
            throw new Exception("No connection to RabbitMQ");

        switch (ackType)
        {
            case ManualAckType.Ack:
                connectionHelper.AMQPModel.BasicAck(deliveryTag, multiple: false);
                break;

            case ManualAckType.Nack:
                connectionHelper.AMQPModel.BasicNack(deliveryTag, multiple: false, requeue: false);
                break;

            case ManualAckType.NackAndRequeue:
                connectionHelper.AMQPModel.BasicNack(deliveryTag, multiple: false, requeue: true);
                break;

            case ManualAckType.Reject:
                connectionHelper.AMQPModel.BasicReject(deliveryTag, requeue: false);
                break;

            case ManualAckType.RejectAndRequeue:
                connectionHelper.AMQPModel.BasicReject(deliveryTag, requeue: true);
                break;
        }

        return true;
    }

    private static Dictionary<string, string> GetResponseHeaderDictionary(IBasicProperties basicProperties)
    {
        if (basicProperties == null) return null;

        var allHeaders = new Dictionary<string, string>()
            {
                { "HEADER_APPID",             basicProperties.AppId != null ? basicProperties.AppId : null },
                { "HEADER_CLUSTERID",         basicProperties.ClusterId != null ? basicProperties.ClusterId : null },
                { "HEADER_CONTENTENCODING",   basicProperties.ContentEncoding != null ? basicProperties.ContentEncoding : null },
                { "HEADER_CONTENTTYPE",       basicProperties.ContentType != null ? basicProperties.ContentType : null },
                { "HEADER_CORRELATIONID",     basicProperties.CorrelationId != null ? basicProperties.CorrelationId : null },
                { "HEADER_EXPIRATION",        basicProperties.Expiration != null ? basicProperties.Expiration : null},
                { "HEADER_MESSAGEID",         basicProperties.MessageId != null ? basicProperties.MessageId : null }
            }
        .Where(h => h.Value != null)
        .ToDictionary(h => h.Key, h => h.Value);

        if (basicProperties.IsHeadersPresent())
            basicProperties.Headers.ToList().ForEach(x => allHeaders[x.Key] = Encoding.UTF8.GetString(x.Value as byte[]));

        return allHeaders;
    }

    private static void OpenConnectionIfClosed(ConnectionHelper connectionHelper, Connection connection)
    {
        // Close connection if hostname has changed.
        if (IsConnectionHostNameChanged(connectionHelper, connection))
            connectionHelper.AMQPModel.Close();

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
                return (connectionHelper.AMQPConnection.Endpoint.HostName != newUri.Host);
            case AuthenticationMethod.Host:
                return (connectionHelper.AMQPConnection.Endpoint.HostName != connection.Host);
            default:
                throw new ArgumentException($"IsConnectionHostNameChanged: AuthenticationMethod missing.");
        }
    }
}