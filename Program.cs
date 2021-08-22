using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using net_6_ef.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ToxicityDb");
builder.Services.AddDbContext<toxicityContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "net_6_ef", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "net_6_ef v1"));
app.UseHttpsRedirection();

app.MapGet("/toxicity", async (toxicityContext _dbContext) => {
    return await _dbContext.ToxicityAnnotations.ToListAsync();
});

app.MapPost("/toxicity", async (toxicityContext _dbContext, ToxicityAnnotation input) => {
    await _dbContext.ToxicityAnnotations.AddAsync(input);
    await _dbContext.SaveChangesAsync();
    return input;
});

app.MapPut("/toxicity", async (toxicityContext _dbContext, ToxicityAnnotation input) => {
    var entity = await _dbContext.ToxicityAnnotations.FindAsync(input.RevId, input.WorkerId);
    if(entity == null)
    {
        Console.WriteLine("Add");
        await _dbContext.ToxicityAnnotations.AddAsync(input);
        await _dbContext.SaveChangesAsync();
        return input;
    }

    Console.WriteLine("Update");
    _dbContext.Entry(entity).CurrentValues.SetValues(input);
    await _dbContext.SaveChangesAsync();
    return input;
});

app.MapDelete("/toxicity", async (toxicityContext _dbContext, decimal revId, decimal workerId) => {
    var entity = await _dbContext.ToxicityAnnotations.FindAsync(revId, workerId);
    if(entity == null)
    {
        return false;
    }

    _dbContext.ToxicityAnnotations.Remove(entity);
    await _dbContext.SaveChangesAsync();
    return true;
});

app.Run();
