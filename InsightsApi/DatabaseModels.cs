using Dapper.Contrib.Extensions;

namespace InsightsApi;

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
}