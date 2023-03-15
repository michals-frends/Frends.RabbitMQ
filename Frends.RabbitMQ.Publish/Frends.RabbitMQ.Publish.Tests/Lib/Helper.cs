using RabbitMQ.Client;
using System.Text;
using Frends.RabbitMQ.Publish.Definitions;

namespace Frends.RabbitMQ.Publish.Tests;
internal class Helper
{
    internal static void ReadMessage(ReadValues readValues, Connection connection)
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
            if (rcvMessage.BasicProperties.IsHeadersPresent())
            {
                foreach (var header in rcvMessage.BasicProperties.Headers.ToList())
                {
                    if (header.Value.GetType() == typeof(byte[]))
                        data[header.Key] = Encoding.UTF8.GetString((byte[])header.Value);
                    else
                        data[header.Key] = header.Value.ToString();
                }
                readValues.Headers = data;
            }
        }
    }

    internal static void DeleteQuorumQueue(string uri, string queue, string? exchange = null)
    {
        var factory = new ConnectionFactory { Uri = new Uri(uri) };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDelete(queue, false, false);
        if (exchange != null)
            channel.ExchangeDelete(exchange, ifUnused: false);
    }

    internal class ReadValues
    {
        public string Message { get; set; } = "";
        public ulong Tag { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    }
}

