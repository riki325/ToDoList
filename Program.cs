using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ToDoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ToDoDbContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("CorsPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwagger(options =>
{
    options.SerializeAsV2 = true;
});

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});


app.MapGet("/todoItems", async (ToDoDbContext db) =>
    await db.Items.ToListAsync()
);

app.MapPost("/todoitems", async (Item item, ToDoDbContext db) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{item.Id}", item);
});

app.MapPut("/todoitems/{id}", async (int id, Boolean isComplete, ToDoDbContext db) =>
{
    var newItem = await db.Items.FindAsync(id);
    if (newItem is null)
        return Results.NotFound();
    newItem.IsComplete = isComplete;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null)
        return Results.NotFound();
    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.Ok(item);

});

app.Run();

