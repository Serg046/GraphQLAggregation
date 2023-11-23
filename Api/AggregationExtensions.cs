using Dapper;
using GraphQL;
using GraphQL.Types;
using GraphQLParser;
using GraphQLParser.AST;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Api;

internal static class DbConnectionExtensions
{
    public static async Task<IEnumerable<dynamic>> QueryAsync(this IDbConnection dbConnection, string query, IResolveFieldContext context, object? param = null)
    {
        var cmd = new CommandDefinition(GetDapperSqlTemplate(query, context), parameters: param);
        return await dbConnection.QueryAsync(cmd);
    }

    public static async Task<IEnumerable<T>> QueryAsync<T>(this IDbConnection dbConnection, string query, IResolveFieldContext context, object? param = null)
    {
        var cmd = new CommandDefinition(GetDapperSqlTemplate(query, context), parameters: param);
        return await dbConnection.QueryAsync<T>(cmd);
    }

    private static string GetDapperSqlTemplate(string query, IResolveFieldContext context)
    {
        string fields, groupByClause;
        if (context.SubFields != null && TryGetGroups(context, out var groups))
        {
#if DEBUG
            if (!Regex.IsMatch(query, @"\{0\}"))
            {
                throw new InvalidOperationException("You should use {0} for the selecting fields in the query template");
            }

            if (!Regex.IsMatch(query, @"\{1\}"))
            {
                throw new InvalidOperationException("You should use {1} for the group by clause in the query template");
            }
#endif
            groupByClause = $"group by {string.Join(",", groups)}";
            fields = string.Join(",", GetAggregatedFields(context.SubFields, groups));
        }
        else
        {
            fields = "*";
            groupByClause = string.Empty;
        }
        
        return string.Format(query, fields, groupByClause);
    }

    private static bool TryGetGroups(IResolveFieldContext context, [NotNullWhen(true)] out string[]? groups)
    {
        if (context.Directives?.TryGetValue(AggregationDirective.NAME, out var directive) == true)
        {
            if (directive.Arguments.TryGetValue(AggregationDirective.ARG_NAME, out var argument) && argument.Value is object[] groupBy)
            {
                groups = groupBy.Select(g => g.ToString()!).ToArray();
                return true;
            }
        }

        groups = null;
        return false;
    }

    private static IEnumerable<string> GetAggregatedFields(Dictionary<string, (GraphQLField Field, FieldType FieldType)> subFields, string[] groups)
    {
        foreach (var field in subFields)
        {
            if (TryGetAggrDirective(field.Value.Field, out var directive))
            {
                if (directive.Arguments?.Find(AggrDirective.ARG_NAME) is GraphQLArgument arg && arg.Value is GraphQLStringValue value)
                {
                    yield return $"{directive.Name}({value.Value}) as {value.Value}";
                }
                else
                {
                    yield return $"{directive.Name}({field.Key}) as {field.Key}";
                }
            }
            else if (groups.Contains(field.Key))
            {
                yield return field.Key;
            }
        }
    }

    private static bool TryGetAggrDirective(GraphQLField field, [NotNullWhen(true)]out GraphQLDirective? directive)
    {
        var names = AggrDirective.Directives.Select(d => d.Name);
        if (field.Directives != null && field.Directives.FirstOrDefault(d => names.Contains(d.Name.StringValue)) is GraphQLDirective d)
        {
            directive = d;
            return true;
        }

        directive = null;
        return false;
    }
}

public class AggregationDirective : Directive
{
    public const string ARG_NAME = "by";
    public const string NAME = "aggregation";

    public AggregationDirective() : base(NAME, DirectiveLocation.Field)
    {
        Arguments = new QueryArguments(new QueryArgument<NonNullGraphType<ListGraphType<StringGraphType>>>
        {
            Name = ARG_NAME
        });
    }
}

public abstract class AggrDirective : Directive
{
    public const string ARG_NAME = "by";

    protected AggrDirective(string name) : base(name, DirectiveLocation.Field)
    {
        Arguments = new QueryArguments(new QueryArgument<StringGraphType>
        {
            Name = ARG_NAME
        });
    }

    public static AggrDirective[] Directives { get; } = new AggrDirective[]
    { 
        new AvgDirective(),
        new MaxDirective(),
        new MinDirective(),
        new SumDirective()
    };
}

public class AvgDirective : AggrDirective
{
    public AvgDirective() : base("avg")
    {
    }
}

public class MaxDirective : AggrDirective
{
    public MaxDirective() : base("max")
    {
    }
}

public class MinDirective : AggrDirective
{
    public MinDirective() : base("min")
    {
    }
}

public class SumDirective : AggrDirective
{
    public SumDirective() : base("sum")
    {
    }
}

