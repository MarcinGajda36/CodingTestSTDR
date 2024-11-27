namespace CodingTestSTDR.HackerNews;

public static class HackerNewsServices
{
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var cacheOptions = configuration.GetRequiredSection("CacheOptions").Get<HackerNewsCacheOptions>()!;
        var hackerNewsUrl = configuration.GetRequiredSection("HackerNewsUrl").Get<Uri>()!;

        var services = builder.Services;
        services.AddHttpClient<HackerNewsClient>(client => client.BaseAddress = hackerNewsUrl);
        services.AddSingleton(cacheOptions);
        services.AddSingleton<IHackerNewsClient, HackerNewsClient>();
        services.AddSingleton<IHackerNewsService, HackerNewsService>();

        return builder;
    }
}
