using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class LinqExtensions
{
    /// <summary>
    /// Orders an IQueryable using a specified property and method
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="property"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string property, string method)
    {
        string methodName = string.Empty;
        switch (method)
        {
            case "asc":
                methodName = "OrderBy";
                break;
            case "desc":
                methodName = "OrderByDescending";
                break;
        }
        return ApplyOrder<T>(source, property, methodName);
    }
    private static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodType)
    {
        string[] props = property.Split('.');
        Type type = typeof(T);
        ParameterExpression arg = Expression.Parameter(type, "x");
        Expression expr = arg;
        foreach (string prop in props)
        {
            // use reflection (not ComponentModel) to mirror LINQ
            PropertyInfo pi = type.GetProperty(prop);
            expr = Expression.Property(expr, pi);
            type = pi.PropertyType;
        }
        Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
        LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

        object result = typeof(Queryable).GetMethods().Single(
                method => method.Name == methodType
                        && method.IsGenericMethodDefinition
                        && method.GetGenericArguments().Length == 2
                        && method.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), type)
                .Invoke(null, new object[] { source, lambda });

        return (IOrderedQueryable<T>)result;
    }

    /// <summary>
    /// Applies an Equals expression to an IQueryable using a specified property and value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="property"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static IQueryable<T> Where<T>(this IQueryable<T> source, string property, object value)
    {
        var param = Expression.Parameter(typeof(T));

        string[] props = property.Split('.');
        Type type = typeof(T);
        ParameterExpression arg = Expression.Parameter(type, "x");
        Expression leftExpression = arg;
        foreach (string prop in props)
        {
            // use reflection (not ComponentModel) to mirror LINQ
            PropertyInfo pi = type.GetProperty(prop);
            leftExpression = Expression.Property(leftExpression, pi);
            type = pi.PropertyType;
        }

        dynamic typedValue = Convert.ChangeType(value, type);

        BinaryExpression condition = Expression.Equal(leftExpression, Expression.Constant(typedValue));

        return source.Where(Expression.Lambda<Func<T, bool>>(condition, param));
    }
    /// <summary>
    /// Applies an Equals expression to an IQueryable using a specified property, operator and value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="property"></param>
    /// <param name="op"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static IQueryable<T> Where<T>(this IQueryable<T> source, string property, string op, object value)
    {
        var param = Expression.Parameter(typeof(T));

        string[] props = property.Split('.');
        Type type = typeof(T);
        ParameterExpression arg = Expression.Parameter(type, "x");
        Expression leftExpression = arg;
        foreach (string prop in props)
        {
            // use reflection (not ComponentModel) to mirror LINQ
            PropertyInfo pi = type.GetProperty(prop);
            leftExpression = Expression.Property(leftExpression, pi);
            type = pi.PropertyType;
        }

        dynamic typedValue = Convert.ChangeType(value, type);


        Expression condition;
        switch (op)
        {
            case "eq":
                condition = Expression.Equal(leftExpression, Expression.Constant(typedValue));
                break;

            case "neq":
                condition = Expression.NotEqual(leftExpression, Expression.Constant(typedValue));
                break;

            case "contains":
                condition = Expression.Call(
                    leftExpression,
                    type.GetMethod("Contains", new[] { typeof(string) }),
                    Expression.Constant(typedValue));
                break;

            case "doesnotcontain":
                condition = Expression.Not(Expression.Call(
                        leftExpression,
                        type.GetMethod("Contains", new[] { typeof(string) }),
                        Expression.Constant(typedValue))
                    );
                break;

            case "startswith":
                condition = Expression.Call(
                        leftExpression,
                        type.GetMethod("StartsWith", new[] { typeof(string) }),
                        Expression.Constant(typedValue));
                break;

            case "endswith":
                condition = Expression.Call(
                        leftExpression,
                        type.GetMethod("EndsWith", new[] { typeof(string) }),
                        Expression.Constant(typedValue));
                break;

            case "gte":
                condition = Expression.GreaterThanOrEqual(leftExpression, Expression.Constant(typedValue));
                break;

            case "gt":
                condition = Expression.GreaterThan(leftExpression, Expression.Constant(typedValue));
                break;

            case "lte":
                condition = Expression.LessThanOrEqual(leftExpression, Expression.Constant(typedValue));
                break;

            case "lt":
                condition = Expression.LessThan(leftExpression, Expression.Constant(typedValue));
                break;

            default:
                condition = Expression.Equal(leftExpression, Expression.Constant(typedValue));
                break;
        }

        return source.Where(Expression.Lambda<Func<T, bool>>(condition, param));
    }

    public static Expression<Func<TSource, bool>> ToOrExpression<TSource, TList>(this List<TList> list, string propertyName)
    {
        var param = Expression.Parameter(typeof(TSource), "x");

        var props = propertyName.Split('.');
        var type = typeof(TSource);
        var arg = Expression.Parameter(type, "x");
        Expression leftExpression = arg;
        foreach (var prop in props)
        {
            // use reflection (not ComponentModel) to mirror LINQ
            var pi = type.GetProperty(prop);
            leftExpression = Expression.Property(leftExpression, pi);
            type = pi.PropertyType;
        }

        var cond = Expression.Equal(leftExpression, Expression.Constant(list[0]));

        for (int i = 1; i < list.Count; i++)
        {
            var orCond = Expression.Equal(leftExpression, Expression.Constant(list[i]));

            cond = Expression.Or(cond, orCond);
        }

        return Expression.Lambda<Func<TSource, bool>>(cond, param);
    }
    public static Expression<Func<TSource, bool>> ToAndExpression<TSource, TList>(this List<TList> list, string propertyName)
    {
        var param = Expression.Parameter(typeof(TSource), "x");

        var props = propertyName.Split('.');
        var type = typeof(TSource);
        var arg = Expression.Parameter(type, "x");
        Expression leftExpression = arg;
        foreach (var prop in props)
        {
            // use reflection (not ComponentModel) to mirror LINQ
            var pi = type.GetProperty(prop);
            leftExpression = Expression.Property(leftExpression, pi);
            type = pi.PropertyType;
        }

        var cond = Expression.Equal(leftExpression, Expression.Constant(list[0]));

        for (int i = 1; i < list.Count; i++)
        {
            var orCond = Expression.Equal(leftExpression, Expression.Constant(list[i]));

            cond = Expression.And(cond, orCond);
        }

        return Expression.Lambda<Func<TSource, bool>>(cond, param);
    }

    /// <summary>
    /// Returns the only result, or a specified default value.
    /// </summary>
    /// <typeparam name="T">The type of result to return.</typeparam>
    /// <param name="list">The list of results to pull a single record from.</param>
    /// <param name="defaultValue">The default value if no single result can be returned.</param>
    /// <returns>Either a single result, or the provided default result.</returns>
    public static T SingleOr<T>(this IEnumerable<T> list, T defaultValue) where T : class
    {
        return list.SingleOrDefault() ?? defaultValue;
    }

    /// <summary>
    /// Returns the first result, or a specified default value.
    /// </summary>
    /// <typeparam name="T">The type of result to return.</typeparam>
    /// <param name="list">The list of results to pull first record from.</param>
    /// <param name="defaultValue">The default value if no single result can be returned.</param>
    /// <returns>Either a first result, or the provided default result.</returns>
    public static T FirstOr<T>(this IEnumerable<T> list, T defaultValue) where T : class
    {
        return list.FirstOrDefault() ?? defaultValue;
    }
}