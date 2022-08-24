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