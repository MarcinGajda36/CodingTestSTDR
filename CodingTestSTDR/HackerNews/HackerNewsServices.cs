namespace CodingTestSTDR.HackerNews;

public static class HackerNewsServices
{
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var cacheOptions = configuration.GetRequiredSection("HackerNewsCacheOptions").Get<HackerNewsCacheOptions>()!;
        var hackerNewsUrl = configuration.GetRequiredSection("HackerNewsUrl").Get<Uri>()!;
        var throttlingOptions = configuration.GetRequiredSection("HackerNewsThrottlingOptions").Get<HackerNewsThrottlingOptions>()!;

        var services = builder.Services;
        services.AddHttpClient<HackerNewsClient>(client => client.BaseAddress = hackerNewsUrl);
        services.AddSingleton(cacheOptions);
        services.AddSingleton(throttlingOptions);
        services.AddSingleton<HackerNewsClient>();
        services.AddSingleton<IHackerNewsClient, ThrottlingHackerNewsClient>();
        services.AddSingleton<IHackerNewsService, HackerNewsService>();

        return builder;
    }
}
