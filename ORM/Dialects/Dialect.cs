namespace skroy.ORM.Dialects;

internal abstract class Dialect
{
	public virtual string AutoIncrement => "auto_increment";
	public virtual string SelectLastRow => "select last_insert_rowid()";

	public abstract string GetDbType(Type type);
}
