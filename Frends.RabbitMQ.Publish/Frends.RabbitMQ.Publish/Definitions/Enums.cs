namespace Frends.RabbitMQ.Publish.Definitions;

/// <summary>
/// Data input types.
/// </summary>
public enum InputType
{
#pragma warning disable CS1591 // self explanatory.
    String,
    ByteArray
#pragma warning restore CS1591 // self explanatory.
}

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