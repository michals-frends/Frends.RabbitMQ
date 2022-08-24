using RabbitMQ.Client;

namespace Frends.RabbitMQ.Read.Definitions;

/// <summary>
/// AMQP parameters.
/// </summary>
public class ConnectionHelper
{
    /// <summary>
    /// AMQP connection parameters.
    /// </summary>
    public IConnection AMQPConnection { get; set; } = null;

    /// <summary>
    /// AMQP model parameters.
    /// </summary>
    public IModel AMQPModel { get; set; } = null;
}