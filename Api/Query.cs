using Dapper;
using GraphQL.Types;
using GraphQLParser.AST;
using System.Data;

namespace Api;

public class Query : ObjectGraphType
{
    public Query(IDbConnection dbConnection)
    {
        Field<ListGraphType<UserType>>("users")
            .ResolveAsync(async context => await dbConnection.QueryAsync<User>("select {0} from users {1}", context));

        Field<ListGraphType<UserType2>>("users2")
            .ResolveAsync(async context => await dbConnection.QueryAsync<User>("select {0} from users {1}", context));

        Field<ListGraphType<UserType3>>("users3").ResolveAsync(async context =>
        {
            return await dbConnection.QueryAsync(
                "select * from Users u inner join Passports p on u.PassportId = p.Id",
                (User u, Passport p) => new { u.FirstName, u.LastName, u.Age, Passport = p });
        });
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
