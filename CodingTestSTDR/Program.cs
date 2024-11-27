
namespace CodingTestSTDR;

using CodingTestSTDR.HackerNews;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var configuration = builder.Configuration;
        var cacheOptions = configuration.GetRequiredSection("CacheOptions").Get<HackerNewsCacheOptions>()!;

        var services = builder.Services;
        services.AddHttpClient();
        services.AddSingleton(cacheOptions);
        services.AddSingleton<IHackerNewsClient, HackerNewsClient>();

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

}