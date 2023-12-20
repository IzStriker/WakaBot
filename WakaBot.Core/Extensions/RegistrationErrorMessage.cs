namespace WakaBot.Core.Extensions;

[AttributeUsage(AttributeTargets.Field)]
public class RegistrationErrorMessage : Attribute
{
    public string Message { get; }
    public bool StopOnError { get; set; }

    /// <summary>
    /// Registration Error Message
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="stopOnError">Whether or not to continue to print other errors</param>
    public RegistrationErrorMessage(string message, bool stopOnError = false)
    {
        this.Message = message;
        this.StopOnError = stopOnError;
    }

}