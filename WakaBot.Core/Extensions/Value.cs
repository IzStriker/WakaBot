using System.ComponentModel.DataAnnotations;

namespace WakaBot.Core.Extensions;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ValueAttribute : ValidationAttribute
{
    public string Payload { get; } = string.Empty;
    public ValueAttribute(string value)
    {
        this.Payload = value;
    }
}