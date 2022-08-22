namespace Frends.RabbitMQ.Read.Definitions;

/// <summary>
/// Authentication methods.
/// </summary>
public enum AuthenticationMethod
{
    /// <summary>
    /// Connect with URI.
    /// </summary>
    URI,

    /// <summary>
    /// Connect with hostname. Username and password are optional.
    /// </summary>
    Host
}

/// <summary>
/// Acknowledge type while reading message.
/// </summary>
public enum ReadAckType
{
#pragma warning disable CS1591 // Self explanatory
    ManualAck,
    AutoAck,
    AutoNack,
    AutoNackAndRequeue,
    AutoReject,
    AutoRejectAndRequeue
#pragma warning restore CS1591 // Self explanatory
}

/// <summary>
/// Acknowledge type for manual operation.
/// </summary>
public enum ManualAckType
{
#pragma warning disable CS1591 // Self explanatory
    Ack,
    Nack,
    NackAndRequeue,
    Reject,
    RejectAndRequeue
#pragma warning restore CS1591 // Self explanatory
}