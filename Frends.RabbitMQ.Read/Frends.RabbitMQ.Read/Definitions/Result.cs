using System.Collections.Generic;

namespace Frends.RabbitMQ.Read.Definitions;

/// <summary>
/// Read result(s).
/// </summary>
public class Result
{
    /// <summary>
    /// Message in Base64 format.
    /// </summary>
    /// <example>VGVzdCBtZXNzYWdl, {foo, bar}, 1, 1</example>
    public List<Message> MessagesBase64 { get; private set; } = new List<Message>();

    /// <summary>
    /// Message in UTF8 format.
    /// </summary>
    /// <example>Test message, {foo, bar}, 1, 1</example>
    public List<Message> MessageUTF8 { get; private set; } = new List<Message>();

    internal Result(List<Message> messagesBase64, List<Message> messageUTF8)
    {
        MessagesBase64 = messagesBase64;
        MessageUTF8 = messageUTF8;
    }
}

/// <summary>
/// Message's data.
/// </summary>
public class Message
{
    /// <summary>
    /// Message.
    /// </summary>
    /// <example>Test message.</example>
    public string Data { get; set; }

    /// <summary>
    /// Message's header(s).
    /// </summary>
    /// <example>foo, bar</example>
    public Dictionary<string, string> Headers { get; set; }

    /// <summary>
    /// Message count.
    /// </summary>
    /// <example>1</example>
    public uint MessagesCount { get; set; }

    /// <summary>
    /// Message's delivery tag.
    /// </summary>
    /// <example>1</example>
    public ulong DeliveryTag { get; set; }
}