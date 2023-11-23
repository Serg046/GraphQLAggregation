using Api;
using Dapper.Contrib.Extensions;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

var dbConnection = new SqliteConnection("DataSource=:memory:");
await dbConnection.OpenAsync();
await PrepareDatabase(dbConnection);

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IDbConnection>(dbConnection);
builder.Services.AddGraphQL(b => b
    .AddSystemTextJson()
    .AddSchema<ApiSchema>()
    .AddGraphTypes(typeof(ApiSchema).Assembly)
    .AddDataLoader());

var app = builder.Build();
app.UseGraphQL("/graphql");
app.UseGraphQLPlayground(
    "/",
    new GraphQL.Server.Ui.Playground.PlaygroundOptions
    {
        GraphQLEndPoint = "/graphql",
        SubscriptionsEndPoint = "/graphql",
    });
await app.RunAsync();

static async Task PrepareDatabase(IDbConnection dbConnection)
{
    var cmd = dbConnection.CreateCommand();
    cmd.CommandText = """
        create table Users(Id int, FirstName nvarchar(100), LastName nvarchar(100), Age int, PassportId int);
        create table Passports(Id int, Number nvarchar(100));
        """;
    cmd.ExecuteNonQuery();
    await dbConnection.InsertAsync(new Passport[]
    {
        new Passport { Id = 1, Number = "abc" },
        new Passport { Id = 2, Number = "bcd" },
        new Passport { Id = 3, Number = "cde" },
        new Passport { Id = 4, Number = "def" },
        new Passport { Id = 5, Number = "efg" }
    });

    await dbConnection.InsertAsync(new User[]
    {
        new User { Id = 1, FirstName = "John", LastName = "Doe", Age = 35, PassportId = 1 },
        new User { Id = 2, FirstName = "Paul", LastName = "Smith", Age = 37, PassportId = 2 },
        new User { Id = 3, FirstName = "Paul", LastName = "Jones", Age = 39, PassportId = 3 },
        new User { Id = 4, FirstName = "John", LastName = "Smith", Age = 41, PassportId = 4 },
        new User { Id = 5, FirstName = "Paul", LastName = "Williams", Age = 43, PassportId = 5 }
    });
}

public class ApiSchema : Schema
{
    public ApiSchema(IServiceProvider services, Query query) : base(services)
    {
        Query = query;
        Directives.Register(new AggregationDirective(), new MaxDirective());
    }
}


