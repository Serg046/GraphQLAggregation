using Dapper.Contrib.Extensions;
using GraphQL.DataLoader;
using GraphQL.Types;
using System.Data;

namespace InsightsApi;

public class PassportType : ObjectGraphType<Passport>
{
    public PassportType()
    {
        Field(x => x.Id);
        Field(x => x.Number);
    }
}

public class UserType : ObjectGraphType<User>
{
    public UserType(IDbConnection dbConnection)
    {
        Field(x => x.FirstName);
        Field(x => x.LastName);
        Field(x => x.Age);
        Field<PassportType>("passport")
            .ResolveAsync(async ctx => await dbConnection.GetAsync<Passport>(ctx.Source.PassportId));
    }
}

public class UserType2 : ObjectGraphType<User>
{
    public UserType2(IDbConnection dbConnection, IDataLoaderContextAccessor dataLoaderAccessor)
    {
        Field(x => x.FirstName);
        Field(x => x.LastName);
        Field(x => x.Age);
        Field<PassportType>("passport").Resolve(ctx =>
        {
            var loader = dataLoaderAccessor.Context!.GetOrAddBatchLoader<int, Passport>("GetPassportsById",
                async ids =>
                {
                    return (await dbConnection.GetAllAsync<Passport>())
                        .Where(p => ids.Contains(p.Id)).ToDictionary(p => p.Id, p => p);
                });
            return loader.LoadAsync(ctx.Source.PassportId);
        });
    }
}

public class UserType3 : ObjectGraphType
{
    public UserType3()
    {
        Field<StringGraphType>("FirstName");
        Field<StringGraphType>("LastName");
        Field<IntGraphType>("Age");
        Field<PassportType>("passport");
    }
}
