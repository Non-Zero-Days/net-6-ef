## C# Data Access

### Prerequisites:

- [.NET 6.0](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Visual Studio Code](https://code.visualstudio.com/)
- [Docker Desktop](https://hub.docker.com/editions/community/docker-ce-desktop-windows)

### Loose Agenda:

Introduce CRUD repositories with EF Core in .NET 6 preview 7

### Step by Step

#### Procure a Database

We're going to need a database for today's efforts, so lets leverage an existing docker compose definition to procure ourselves a postgres instance. 

I've prepared a docker compose definition at the [Non-Zero Days wiki-toxicity-database repository](https://github.com/Non-Zero-Days/wiki-toxicity-database) which will spin us up a database for today's exercise.

Clone down that repository, navigate to it in a terminal and run `docker compose up -d` to obtain our database.

#### Setup Playground

Next, let's create a directory for today's exercise and navigate to it in a terminal instance.

#### Install EF dotnet CLI tools

In order to scaffold the database (generate Entity Framework code) in the webapi project you must first install the Entity Framework .NET CLI tools. `dotnet tool install --global dotnet-ef`

#### Spin up a new application

Run `dotnet new webapi` then open the directory in Visual Studio Code.

#### Initialize User Secrets

In order to configure our local development environment to connect to the database, we need to run the following

```cli
dotnet user-secrets init
dotnet user-secrets set ConnectionStrings:ToxicityDb "Username=docker;Password=docker;Host=host.docker.internal;Database=toxicity;"
```

#### Add NuGet Packages

The following commands will add some necessary dependencies for data access with EF and Postgres.

```cli
dotnet add .\net-6-ef.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add .\net-6-ef.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
```

#### Scaffold Database Access

Now we can generate the Entity Framework code.

```cli
dotnet ef dbcontext scaffold "Server=localhost;Database=toxicity;Port=5432;Username=docker;Password=docker;" Npgsql.EntityFrameworkCore.PostgreSQL --output-dir "Infrastructure"
```

#### Configure Program.cs

We'll start by grabbing our configured connection string and adding the DbContext with that configuration.

```C#
var connectionString = builder.Configuration.GetConnectionString("ToxicityDb");
builder.Services.AddDbContext<toxicityContext>(options => options.UseNpgsql(connectionString));
```

Replace `builder.Services.AddControllers();` with `builder.Services.AddEndpointsApiExplorer();`

Move these two lines out of the if statement
```C#
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "net_6_ef v1"));
```

Remove these two lines
```C#
app.UseAuthorization();
app.MapControllers();
```

#### Expose Data Access GET

Now we can add an endpoint. Just above `app.Run();` let's add the following code:
```C#
app.MapGet("/toxicity", async (toxicityContext _dbContext) => {
    return await _dbContext.ToxicityAnnotations.ToListAsync();
});
```

Now let's run the application via `dotnet run` and navigate to [https://localhost:5001/toxicity](https://localhost:5001/toxicity) to see the data from our database.

#### Expose Data Access POST

For a POST endpoint we'll add a paramter to the RequestDelegate for the body of the request and we'll call AddAsync:
```C#
app.MapPost("/toxicity", async (toxicityContext _dbContext, ToxicityAnnotation input) => {
    await _dbContext.ToxicityAnnotations.AddAsync(input);
    await _dbContext.SaveChangesAsync();
    return input;
});
```

#### Expose Data Access PUT

PUT is traditionally an idempotent save, therefore we'll check if an entity exists before doing an add or an update.
```C#
app.MapPut("/toxicity", async (toxicityContext _dbContext, ToxicityAnnotation input) => {
    var entity = await _dbContext.ToxicityAnnotations.FindAsync(input.RevId, input.WorkerId);
    if (entity == null)
    {
        await _dbContext.ToxicityAnnotations.AddAsync(input);
        await _dbContext.SaveChangesAsync();
        return input;
    }

    Console.WriteLine("Updating");
    _dbContext.Entry(entity).CurrentValues.SetValues(input);
    await _dbContext.SaveChangesAsync();
    return input;
});
```

#### Expose Data Access DELETE

DELETE will check if the entity exists and remove it if it does
```C#
app.MapDelete("/toxicity", async (toxicityContext _dbContext, decimal revId, decimal workerId) => {
    var entity = await _dbContext.ToxicityAnnotations.FindAsync(revId, workerId);
    if (entity == null)
    {
        return false;
    }

    _dbContext.ToxicityAnnotations.Remove(entity);
    await _dbContext.SaveChangesAsync();
    return true;
});
```

Let's run the application via `dotnet run` and navigate to [Swagger](https://localhost:5001/swagger) to play with our endpoints.

## Additional Documentation

- [.NET Preview 7 Announcement](https://devblogs.microsoft.com/aspnet/asp-net-core-updates-in-net-6-preview-7/)
- [Entity Framework SaveChangesAsync](https://docs.microsoft.com/en-us/dotnet/api/system.data.entity.dbcontext.savechangesasync)

Congratulations on a non-zero day!
