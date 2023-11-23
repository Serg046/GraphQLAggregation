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
                (User u, Passport p) => new { u.FirstName, u.LastName, u.Age, u.Kids, Passport = p });
        });
    }
}

