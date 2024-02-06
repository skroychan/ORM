using System.Reflection;

namespace skroy.ORM.Helpers;

internal static class TypeHelper
{
    public static Type GetUnderlyingType(PropertyInfo propertyInfo, out bool isNullable)
    {
        var propertyType = propertyInfo.PropertyType;
        var underlyingType = Nullable.GetUnderlyingType(propertyType);
        isNullable = underlyingType != null;
        if (isNullable)
            return underlyingType;

        isNullable = propertyType == typeof(string)
            || new NullabilityInfoContext().Create(propertyInfo).WriteState is NullabilityState.Nullable;
        return propertyType;
    }
}
