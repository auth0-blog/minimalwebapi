
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MyTodoWebApi;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITodoRepository, TodoRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { 
        Title = "TODO API",
        Description = "Web APIs for managing a TODO list",
        Version = "v1" 
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, c =>
    {
        c.Authority = $"https://{builder.Configuration["Auth0:Domain"]}";
        c.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidAudience = builder.Configuration["Auth0:Audience"],
            ValidIssuer = $"{builder.Configuration["Auth0:Domain"]}"
        };
    });

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("todo:read-write", p => p.
        RequireAuthenticatedUser().
        RequireClaim("scope", "todo:read-write"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (ctx, next) =>
{
    try
    {
        await next();
    }
    catch(BadHttpRequestException ex)
    {
        ctx.Response.StatusCode = ex.StatusCode;
        await ctx.Response.WriteAsync(ex.Message);
    }
});

app.MapGet("/todo", async (ITodoRepository repo) =>
{
    var todos = await repo.GetAllAsync();
    return todos;
}).RequireAuthorization("todo:read-write");

app.MapGet("/todo/{id}", async (string id, ITodoRepository repo) =>
{
    if (string.IsNullOrWhiteSpace(id))
    {
        throw new BadHttpRequestException("id is required",
          (int)HttpStatusCode.BadRequest);
    }

    var todo = await repo.Get(id);
    if (todo == null)
    {
        throw new BadHttpRequestException("item not found",
                    (int)HttpStatusCode.NotFound);
    }

    return todo;
}).RequireAuthorization("todo:read-write");

app.MapPost("/todo", async (
    HttpRequest req,
    HttpResponse res,
    ITodoRepository repo) =>
{
    if (!req.HasJsonContentType())
    {
        throw new BadHttpRequestException("only application/json supported",
            (int)HttpStatusCode.NotAcceptable);
    }

    var todo = await req.ReadFromJsonAsync<TodoItem>();

    if (todo != null || string.IsNullOrWhiteSpace(todo.description))
    {
        throw new BadHttpRequestException("description is required",
            (int)HttpStatusCode.BadRequest);
    }

    var id = await repo.CreateAsync(todo.description);
    res.StatusCode = (int)HttpStatusCode.Created;
    res.Headers.Location = $"/todo/{id}";
}).RequireAuthorization("todo:read-write");

app.MapDelete("/todo/{id}", async (string id, ITodoRepository repo) =>
{
    if (string.IsNullOrWhiteSpace(id))
    {
        throw new BadHttpRequestException("id is required",
          (int)HttpStatusCode.BadRequest);
    }

    await repo.Delete(id);
}).RequireAuthorization("todo:read-write");

app.MapPut("/todo/{id}", async (string id, HttpRequest req, ITodoRepository repo) =>
{
    if (!req.HasJsonContentType())
    {
        throw new BadHttpRequestException("only application/json supported",
            (int)HttpStatusCode.NotAcceptable);
    }

    var todo = await req.ReadFromJsonAsync<TodoItem>();

    if (todo != null || !todo.completed.HasValue)
    {
        throw new BadHttpRequestException("completed is required",
            (int)HttpStatusCode.BadRequest);
    }

    await repo.Update(id, todo.completed.Value);
}).RequireAuthorization("todo:read-write");

app.Run();
