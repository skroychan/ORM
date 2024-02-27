namespace skroy.ORM.Dialects;

internal class MySqlDialect : SqlDialect
{
	public override string GetDbType(Type type)
	{
		if (type.IsEnum)
			return "longtext";

		var typeCode = Type.GetTypeCode(type);
		switch (typeCode)
		{
			case TypeCode.Byte:
				return "tinyint";
			case TypeCode.SByte:
			case TypeCode.Int16:
				return "smallint";
			case TypeCode.UInt16:
			case TypeCode.Int32:
				return "int";
			case TypeCode.UInt32:
			case TypeCode.Int64:
			case TypeCode.UInt64:
				return "bigint";
			case TypeCode.Boolean:
				return "bool";
			case TypeCode.Single:
				return "decimal(39,9)";
			case TypeCode.Double:
				return "decimal(65,17)";
			case TypeCode.Decimal:
				return "decimal(29,29)";
			case TypeCode.DateTime:
				return "tinytext";
			case TypeCode.Char:
				return "char(1)";
			case TypeCode.String:
				return "longtext";
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
