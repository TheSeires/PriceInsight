using System.Linq.Expressions;
using System.Text;

namespace API.Models.Application;

public static class ExpressionKeyGenerator
{
    public static string GetKey<T>(Expression<Func<T, bool>> expression)
    {
        var visitor = new ExpressionStringBuilderVisitor();
        visitor.Visit(expression);
        return visitor.ToString();
    }
}

public class ExpressionStringBuilderVisitor : ExpressionVisitor
{
    private StringBuilder _sb = new();

    public override string ToString() => _sb.ToString();

    protected override Expression VisitBinary(BinaryExpression node)
    {
        Visit(node.Left);
        _sb.Append(GetOperatorString(node.NodeType));
        Visit(node.Right);
        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
        {
            _sb.Append(node.Member.Name);
        }
        else
        {
            var value = Expression.Lambda(node).Compile().DynamicInvoke();
            _sb.Append(value == null ? "null" : value.ToString());
        }
        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        _sb.Append(node.Value == null ? "null" : node.Value.ToString());
        return node;
    }

    private string GetOperatorString(ExpressionType type)
    {
        switch (type)
        {
            case ExpressionType.Equal:
                return "==";
            case ExpressionType.NotEqual:
                return "!=";
            case ExpressionType.AndAlso:
                return "&&";
            case ExpressionType.OrElse:
                return "||";
            case ExpressionType.GreaterThan:
                return ">";
            case ExpressionType.GreaterThanOrEqual:
                return ">=";
            case ExpressionType.LessThan:
                return "<";
            case ExpressionType.LessThanOrEqual:
                return "<=";
            default:
                return type.ToString();
        }
    }
}
