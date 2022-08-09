using System;
using System.Linq;

namespace TwitterCommander.Helper;

[AttributeUsage(AttributeTargets.Property)]
public class IsExpressionPropertyAttribute : Attribute
{
    protected internal static bool IsExpressionProperty(string propertyName, object sender)
    {
        var isConfig =
            sender.GetType()
                .GetProperty(propertyName)?
                .CustomAttributes.Any(x => x.AttributeType == typeof(IsExpressionPropertyAttribute)) ?? false;

        return isConfig;

    }
}