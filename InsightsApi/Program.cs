using Dapper;
using Dapper.Contrib.Extensions;
using GraphQL;
using GraphQL.Types;
using GraphQLParser.AST;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Linq.Expressions;

var dbConnection = new SqliteConnection("DataSource=:memory:");
await dbConnection.OpenAsync();
await PrepareDatabase(dbConnection);

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IDbConnection>(dbConnection);
var schema = new Schema { Query = new Query() };
schema.Directives.Register(new AggregationDirective(), new SumDirective());
builder.Services.AddGraphQL(b => b
    .AddSchema(schema)
    .AddSystemTextJson());
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
        new User { Id = 2, FirstName = "", LastName = "Smith", Age = 37, PassportId = 2 },
        new User { Id = 3, FirstName = "Paul", LastName = "Jones", Age = 39, PassportId = 3 },
        new User { Id = 4, FirstName = "John", LastName = "Smith", Age = 41, PassportId = 4 },
        new User { Id = 5, FirstName = "Paul", LastName = "Williams", Age = 43, PassportId = 5 }
    });
}

public class Passport
{
    [ExplicitKey]
    public required int Id { get; init; }

    public required string Number { get; init; }
}

public class User
{
    [ExplicitKey]
    public required int Id { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required int Age { get; init; }

    public required int PassportId { get; init; }

    [Computed]
    public Passport? Passport { get; set; }
}

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
    public UserType()
    {
        Field(x => x.FirstName);
        Field(x => x.LastName);
        Field(x => x.Age);
        Field<PassportType>("passport");
    }
}

public class Query : ObjectGraphType
{
    public Query()
    {
        Field<ListGraphType<UserType>>("users")
            .ResolveAsync(async context =>
            {
                var dbConnection = context.RequestServices!.GetRequiredService<IDbConnection>();
                var users = await dbConnection.GetAllAsync<User>();
                var passports = await dbConnection.GetAllAsync<Passport>();
                var dataStorage = await dbConnection.QueryAsync<User, Passport, User>(
                    "select * from Users u inner join Passports p on u.PassportId = p.Id",
                    (u, p) =>
                    {
                        u.Passport = p;
                        return u;
                    });

                if (context.Directives?.TryGetValue(AggregationDirective.NAME, out var directive) == true)
                {
                    if (directive.Arguments.Values.FirstOrDefault().Value is object[] groupBy)
                    {
                        foreach (var groupName in groupBy)
                        {
                            if (FindResolvedType(context.FieldDefinition.ResolvedType) is IObjectGraphType groupProp)
                            {
                                if (groupProp.Fields.Find(groupName.ToString()!) is FieldType groupField && groupField.Metadata["ORIGINAL_EXPRESSION_PROPERTY_NAME"] is string actualName)
                                {
                                    // One value is supported for now
                                    return dataStorage.GroupBy(BuildFilter<User, object>(actualName)).SelectMany(x => x);
                                }
                            }
                        }
                    }
                }

                return dataStorage;
            });
        Field<ListGraphType<PassportType>>("passports")
            .ResolveAsync(async context =>
            {
                var dbConnection = context.RequestServices!.GetRequiredService<IDbConnection>();
                return await dbConnection.GetAllAsync<Passport>();
            });
    }

    private IGraphType? FindResolvedType(IGraphType? type)
    {
        return type is IProvideResolvedType prt && prt.ResolvedType != null
            ? prt.ResolvedType : type;
    }

    private Func<TEntity, TKey> BuildFilter<TEntity, TKey>(string groupBy)
    {
        ParameterExpression param = Expression.Parameter(typeof(TEntity), "g");
        Expression<Func<TEntity, TKey>> exp = Expression.Lambda<Func<TEntity, TKey>>(Expression.Property(param, groupBy), param);
        return exp.Compile();
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

public class SumDirective : Directive
{
    public const string NAME = "sum";

    public SumDirective() : base(NAME, DirectiveLocation.Field)
    {
    }
}
