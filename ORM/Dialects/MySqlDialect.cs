namespace skroy.ORM.Dialects;

internal class MySqlDialect : Dialect
{
	public override string GetDbType(Type type)
	{
		throw new NotImplementedException();
	}
}
