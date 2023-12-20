using System.Reflection;
using Discord.Interactions;

namespace WakaBot.Core.Extensions;

public static class EnumExtension
{
    public static string? GetValue(this Enum property)
    {
        return property.GetType().GetTypeInfo().GetDeclaredField(property.ToString())?.GetCustomAttribute<ValueAttribute>()?.Payload;
    }

    public static string? GetDisplay(this Enum property)
    {
        return property.GetType().GetTypeInfo().GetDeclaredField(property.ToString())?.GetCustomAttribute<ChoiceDisplayAttribute>()?.Name;
    }

    public static (string?, bool) GetRegistrationErrorMessage(this Enum property)
    {
        var error = property.GetType().GetTypeInfo().GetDeclaredField(property.ToString())?.GetCustomAttribute<RegistrationErrorMessage>();
        return (error?.Message, error?.StopOnError ?? false);
    }
}