using RabbitMQ.Client;
using System;

namespace Frends.RabbitMQ.Read.Definitions;

/// <summary>
/// AMQP parameters.
/// </summary>
internal class ConnectionHelper : IDisposable
{
    /// <summary>
    /// AMQP connection parameters.
    /// </summary>
    public IConnection AMQPConnection { get; set; } = null;

    /// <summary>
    /// AMQP model parameters.
    /// </summary>
    public IModel AMQPModel { get; set; } = null;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            AMQPModel?.Close();
            AMQPModel?.Dispose();
            AMQPConnection?.Close();
            AMQPConnection?.Dispose();
        }
    }
}