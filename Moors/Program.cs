using Microsoft.EntityFrameworkCore;
using Moors.Data;
using Moors.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<TmdbService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=moors.db"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("https://localhost:7145", "http://localhost:7145")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var tmdbService = scope.ServiceProvider.GetRequiredService<TmdbService>();
    context.Database.EnsureCreated();

    CsvMovieImporter.ImportMovies(
        context,
        @"C:\Users\Alexandra.W520\OneDrive\Documente\OneDrive\Desktop\Moors\MoorsRecommendationSystem\movies.csv");
    await tmdbService.EnrichMissingMoviesAsync(context);
}


app.Run();