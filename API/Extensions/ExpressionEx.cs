using System.Linq.Expressions;
using API.Models.Application;

namespace API.Extensions;

public static class ExpressionEx
{
    public static Expression<Func<T, bool>> CombineWith<T>(
        this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2,
        Func<Expression, Expression, BinaryExpression> operation)
    {
        var parameter = expr1.Parameters[0];
        var modifiedExpr2 = new ExpressionParameterReplacer(expr2.Parameters[0], parameter).Visit(expr2.Body);
        var combinedBody = operation(expr1.Body, modifiedExpr2);
        return Expression.Lambda<Func<T, bool>>(combinedBody, parameter);
    }
}