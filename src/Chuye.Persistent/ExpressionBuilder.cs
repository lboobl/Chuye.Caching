using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent {
    public static class ExpressionBuilder {
        // http://stackoverflow.com/questions/671968/retrieving-property-name-from-lambda-expression
        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> selector) {
            LambdaExpression lambda = selector as LambdaExpression;
            if (lambda == null)
                throw new ArgumentNullException("method");

            MemberExpression memberExpr = null;
            if (lambda.Body.NodeType == ExpressionType.Convert) {
                memberExpr =
                    ((UnaryExpression)lambda.Body).Operand as MemberExpression;
            }
            else if (lambda.Body.NodeType == ExpressionType.MemberAccess) {
                memberExpr = lambda.Body as MemberExpression;
            }

            if (memberExpr == null) {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' not refers to a property.",
                    selector.ToString()));
            }
            var propInfo = memberExpr.Member as PropertyInfo;
            Type sourceType = typeof(TSource);
            if (!propInfo.ReflectedType.IsAssignableFrom(sourceType)) {
                throw new ArgumentException(string.Format(
                    "Expresion '{0}' refers to a property that is not from type {1}.",
                    selector.ToString(), sourceType));
            }

            return propInfo;
        }

        public static PropertyInfo GetPropertyInfo<TSource>(Expression<Func<TSource, Object>> selector) {
            LambdaExpression lambda = selector as LambdaExpression;
            if (lambda == null)
                throw new ArgumentNullException("method");

            MemberExpression memberExpr = null;
            if (lambda.Body.NodeType == ExpressionType.Convert) {
                memberExpr =
                    ((UnaryExpression)lambda.Body).Operand as MemberExpression;
            }
            else if (lambda.Body.NodeType == ExpressionType.MemberAccess) {
                memberExpr = lambda.Body as MemberExpression;
            }

            if (memberExpr == null) {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' not refers to a property.",
                    selector.ToString()));
            }
            return memberExpr.Member as PropertyInfo;
        }
    }
}
