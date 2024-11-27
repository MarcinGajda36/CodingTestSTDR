namespace CodingTestSTDR.HackerNews;

using System.Collections.Immutable;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

public interface IHackerNewsClient
{
    Task<ImmutableArray<long>> GetBestStoriesAsync(CancellationToken cancellationToken);
    Task<HackerNewsItem> GetItemAsync(long itemId, CancellationToken cancellationToken);
}

public class HackerNewsClient(
    IHttpClientFactory clientFactory,
    HackerNewsCacheOptions cacheOptions)
    : IHackerNewsClient
{
    private const string IdPlaceholder = "{Placeholder}";

    private readonly MemoryCache cache = new(Options.Create(new MemoryCacheOptions { SizeLimit = cacheOptions.MaxSize }));
    private readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public Task<ImmutableArray<long>> GetBestStoriesAsync(CancellationToken cancellationToken)
    {
        const string BestStoriesV0 = "v0/beststories.json";
        return GetFromHackerNewsAsync(
            BestStoriesV0,
            async (client, cancellationToken) =>
            {
                var ids = await client.GetFromJsonAsync<long[]>(BestStoriesV0, jsonOptions, cancellationToken);
                return ids?.ToImmutableArray() ?? throw new NullReferenceException($"Expected: array of Ids from: {BestStoriesV0} but got null");
            },
            cancellationToken);
    }

    public Task<HackerNewsItem> GetItemAsync(long storyId, CancellationToken cancellationToken)
    {
        const string ItemTemplate = $"/v0/item/{IdPlaceholder}.json";
        var itemUrl = ItemTemplate.Replace(IdPlaceholder, storyId.ToString());
        return GetFromHackerNewsAsync(
            itemUrl,
            async (client, cancellationToken) =>
            {
                var item = await client.GetFromJsonAsync<HackerNewsItem>(itemUrl, jsonOptions, cancellationToken);
                return item ?? throw new NullReferenceException($"Expected: {nameof(HackerNewsItem)} from: {itemUrl} but got null");
            },
            cancellationToken);
    }

    private Task<TResult> GetFromHackerNewsAsync<TResult>(
        string requestUri,
        Func<HttpClient, CancellationToken, ValueTask<TResult>> resultFactory,
        CancellationToken cancellationToken)
        => cache.GetOrCreateAsync(requestUri, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = cacheOptions.AbsoluteExpirationRelativeToNow;
            entry.Size = 1;
            using var client = clientFactory.CreateClient(nameof(HackerNewsClient));
            var result = await resultFactory(client, cancellationToken);
            return result;
        })!;
}
