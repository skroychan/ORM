namespace skroy.ORM.Mapper;

internal record Index(IEnumerable<Column> Columns, bool IsUnique);
