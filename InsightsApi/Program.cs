using GraphQL;
using GraphQL.Types;
using GraphQLParser.AST;
using Microsoft.AspNetCore.Builder;
using System.Linq.Expressions;

var builder = WebApplication.CreateBuilder(args);
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

public record Passport(int Id, string Number);
public record User(string FirstName, string LastName, int Age, Passport Passport);

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
        var dataStorage = new User[]
        {
            new("John", "Doe", 35, new(1, "abc")),
            new("Paul", "Smith", 37, new(2, "bcd")),
            new("Paul", "Jones", 39, new(3, "cde")),
            new("John", "Smith", 41, new(4, "def")),
            new("Paul", "Williams", 43, new(5, "efg"))
        };

        Field<ListGraphType<UserType>>("users")
            .Resolve(context =>
            {
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
            .Resolve(context => dataStorage.Select(u => u.Passport));
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

    public SumDirective(): base(NAME, DirectiveLocation.Field)
    {
    }
}
