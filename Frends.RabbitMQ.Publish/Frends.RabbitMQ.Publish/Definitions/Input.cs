using System.ComponentModel.DataAnnotations;

namespace Frends.RabbitMQ.Publish.Definitions;

/// <summary>
/// Input parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Data input type.
    /// </summary>
    /// <example>String, Byte Array</example>
    public InputType InputType { get; set; }

    /// <summary>
    /// Data payload in byte array format.
    /// </summary>
    /// <example>54 65 73 74 20 6d 65 73 73 61 67 65</example>
    [UIHint(nameof(InputType), "", InputType.ByteArray)]
    public byte[] DataByteArray { get; set; } = null;

    /// <summary>
    /// Data payload in string format.
    /// </summary>
    /// <example>Test message</example>
    [UIHint(nameof(InputType), "", InputType.String)]
    [DisplayFormat(DataFormatString = "Text")]
    public string DataString { get; set; }

    /// <summary>
    /// List of headers to be added to the request.
    /// </summary>
    /// <example>foo, bar</example>
    public Header[] Headers { get; set; }
}

/// <summary>
/// Header values.
/// </summary>
public class Header
{
    /// <summary>
    /// Header's name.
    /// </summary>
    /// <example>foo</example>
    public string Name { get; set; }

    /// <summary>
    /// Header's value.
    /// </summary>
    /// <example>bar</example>
    public string Value { get; set; }
}