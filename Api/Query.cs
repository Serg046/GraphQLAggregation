using GraphQL.Types;
using System.Data;

namespace Api;

public class Query : ObjectGraphType
{
    public Query(IDbConnection dbConnection)
    {
        Field<ListGraphType<UserType>>("users")
            .ResolveAsync(async context => await dbConnection.QueryAsync<User>("select {0} from users {1}", context));
    }
}

