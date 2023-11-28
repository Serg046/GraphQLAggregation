using Dapper.Contrib.Extensions;
using GraphQL.DataLoader;
using GraphQL.Types;
using System.Data;

namespace Api;

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
    public UserType(IDbConnection dbConnection, IDataLoaderContextAccessor dataLoaderAccessor)
    {
        Field(x => x.FirstName);
        Field(x => x.LastName);
        Field(x => x.Age);
        Field(x => x.Kids);
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
