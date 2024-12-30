namespace CodingTestSTDR.HackerNews;

using System.Collections.Immutable;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using CodingTestSTDR.Parallelisms;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

public interface IHackerNewsClient
{
    Task<ImmutableArray<long>> GetBestStoriesIdsAsync(CancellationToken cancellationToken);
    Task<HackerNewsItem> GetItemAsync(long itemId, CancellationToken cancellationToken);
}

public class ThrottlingHackerNewsClient(
    HackerNewsClient hackerNewsClient,
    HackerNewsThrottlingOptions throttlingOptions)
    : IHackerNewsClient
{
    private readonly PerKeySynchronizer keySynchronizer = new(throttlingOptions.MaxDegreeOfParallelism);
    public Task<ImmutableArray<long>> GetBestStoriesIdsAsync(CancellationToken cancellationToken)
        => keySynchronizer.SynchronizeAsync(
            nameof(GetBestStoriesIdsAsync),
            hackerNewsClient,
            (hackerNewsClient, token) => hackerNewsClient.GetBestStoriesIdsAsync(token),
            cancellationToken);

    public Task<HackerNewsItem> GetItemAsync(long itemId, CancellationToken cancellationToken)
        => keySynchronizer.SynchronizeAsync(
            itemId,
            (hackerNewsClient, itemId),
            (clientIdPair, token) =>
            {
                var (client, id) = clientIdPair;
                return client.GetItemAsync(id, token);
            },
            cancellationToken);
}

public class HackerNewsClient(
    IHttpClientFactory clientFactory,
    HackerNewsCacheOptions cacheOptions)
    : IHackerNewsClient
{
    private readonly MemoryCache cache = new(Options.Create(new MemoryCacheOptions { SizeLimit = cacheOptions.MaxSize }));
    private readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public Task<ImmutableArray<long>> GetBestStoriesIdsAsync(CancellationToken cancellationToken)
    {
        const string BestStories = "v0/beststories.json";
        return GetFromHackerNewsAsync(
            BestStories,
            async (client, cancellationToken) =>
            {
                var ids = await client.GetFromJsonAsync<long[]>(BestStories, jsonOptions, cancellationToken);
                return ids?.ToImmutableArray() ?? throw new NullReferenceException($"Expected: array of Ids from: {BestStories} but got null");
            },
            cancellationToken);
    }

    public Task<HackerNewsItem> GetItemAsync(long storyId, CancellationToken cancellationToken)
    {
        var itemUrl = $"/v0/item/{storyId}.json";
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
