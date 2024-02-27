namespace skroy.ORM.Dialects;

internal class SqliteDialect : SqlDialect
{
	public override string AutoIncrement => "autoincrement";


	public override string GetDbType(Type type)
	{
		if (type.IsEnum)
			return "text";

		var typeCode = Type.GetTypeCode(type);
		switch (typeCode)
		{
			case TypeCode.Byte:
			case TypeCode.SByte:
			case TypeCode.Int16:
			case TypeCode.UInt16:
			case TypeCode.Int32:
			case TypeCode.UInt32:
			case TypeCode.Int64:
			case TypeCode.UInt64:
			case TypeCode.Boolean:
				return "integer";
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
				return "real";
			case TypeCode.DateTime:
			case TypeCode.Char:
			case TypeCode.String:
				return "text";
			default:
				if (typeCode == TypeCode.Object)
				{
					var nullableType = Nullable.GetUnderlyingType(type);
					if (nullableType != null)
						return GetDbType(nullableType);
				}
				throw new ArgumentOutOfRangeException(nameof(type), typeCode, "type is not supported by database");
		}
	}
}
