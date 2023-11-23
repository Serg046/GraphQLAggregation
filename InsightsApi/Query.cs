using GraphQL.Types;
using GraphQL;
using GraphQLParser.AST;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Dapper;
using GraphQLParser;
using Dapper.Contrib.Extensions;

namespace Api;

public class Query : ObjectGraphType
{
    public Query(IDbConnection dbConnection)
    {
        base.Field<ListGraphType<UserType>>("users").ResolveAsync(async context =>
        {
            var query = "select {0} from users {1}";
            if (context.SubFields != null && TryGetGroups(context, out var groups))
            {
                return await dbConnection.QueryAsync<User>(string.Format(
                    query,
                    string.Join(",", GetAggregatedFields(context.SubFields, groups)),
                    $"group by {string.Join(",", groups)}"));
            }

            return await dbConnection.QueryAsync<User>(string.Format(query, "*", ""));
        });

        base.Field<ListGraphType<UserType2>>("users2")
            .ResolveAsync(async context => await dbConnection.GetAllAsync<User>());

        base.Field<ListGraphType<UserType3>>("users3").ResolveAsync(async context =>
        {
            return await dbConnection.QueryAsync(
                "select * from Users u inner join Passports p on u.PassportId = p.Id",
                (User u, Passport p) => new { u.FirstName, u.LastName, u.Age, Passport = p });
        });
    }

    private IEnumerable<string> GetAggregatedFields(Dictionary<string, (GraphQLField Field, FieldType FieldType)> subFields, string[] groups)
    {
        foreach (var field in subFields)
        {
            if (field.Value.Field.Directives?.Find(MaxDirective.NAME) is GraphQLDirective directive)
            {
                if (directive.Arguments?.Find("by") is GraphQLArgument arg && arg.Value is GraphQLStringValue value)
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

    private bool TryGetGroups(IResolveFieldContext context, [NotNullWhen(true)] out string[]? groups)
    {
        if (context.Directives?.TryGetValue(AggregationDirective.NAME, out var directive) == true)
        {
            if (directive.Arguments.Values.FirstOrDefault().Value is object[] groupBy)
            {
                groups = groupBy.Select(g => g.ToString()!).ToArray();
                return true;
            }
        }

        groups = null;
        return false;
    }
}

public class AggregationDirective : Directive
{
    public const string NAME = "aggregation";

    public AggregationDirective() : base(NAME, DirectiveLocation.Field)
    {
        Arguments = new QueryArguments(new QueryArgument<NonNullGraphType<ListGraphType<StringGraphType>>>
        {
            Name = "by"
        });
    }
}

public class MaxDirective : Directive
{
    public const string NAME = "max";

    public MaxDirective() : base(NAME, DirectiveLocation.Field)
    {
        Arguments = new QueryArguments(new QueryArgument<StringGraphType>
        {
            Name = "by"
        });
    }
}
