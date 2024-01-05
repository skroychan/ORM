namespace skroy.ORM.Dialects;

public class MySqlDialect : Dialect
{
	public override string GetDbType(Type type)
	{
		throw new NotImplementedException();
	}
}
