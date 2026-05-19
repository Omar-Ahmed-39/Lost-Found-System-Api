using LostAndFound.Api.Extensions;
using LostAndFound.Api.Middlewares;
using LostAndFound.Core.Entities;
using LostAndFound.Core.Features.Matches.Commands;
using LostAndFound.Infrastructure.Repository;
using MediatR;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerDocumentation(); // Swashbuckle — sole OpenAPI provider

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(VerifyMatchCommand).Assembly));

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

// Execute Database Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<LostAndFound.Infrastructure.ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<Role>>();

        await LostAndFound.Infrastructure.Configurations.ApplicationDbContextSeed.SeedAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Global exception handling — must be first in the pipeline
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lost and Found API v1"));
}

app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/test-firebase", () =>
{
    var firebaseApp = FirebaseAdmin.FirebaseApp.DefaultInstance;
    return firebaseApp != null
        ? Results.Ok(new { Message = "Firebase initialized successfully!", AppName = firebaseApp.Name })
        : Results.Problem("Firebase not initialized.");
});

app.Run();
