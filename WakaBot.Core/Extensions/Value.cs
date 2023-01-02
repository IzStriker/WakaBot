using System.ComponentModel.DataAnnotations;

namespace WakaBot.Core.Extensions;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class Value : ValidationAttribute
{
    public string Payload { get; } = string.Empty;
    public Value(string value)
    {
        this.Payload = value;
    }
}