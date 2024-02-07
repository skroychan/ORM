using System.Linq.Expressions;

namespace skroy.ORM.Helpers;

internal static class ExpessionHelper
{
	public static string GetMemberName<T, P>(Expression<Func<T, P>> selector)
	{
		var expression = selector.Body;
		if (expression is UnaryExpression ue && ue.NodeType == ExpressionType.Convert)
			expression = ue.Operand;
		var memberExpression = (MemberExpression)expression;
		return memberExpression.Member.Name;
	}
}
