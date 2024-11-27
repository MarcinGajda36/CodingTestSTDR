namespace CodingTestSTDR.HackerNews;

using System.Collections.Immutable;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

public interface IHackerNewsClient
{
    Task<ImmutableArray<long>> GetBestStoriesAsync(CancellationToken cancellationToken);
    Task<HackerNewsItem> GetItemAsync(long itemId, CancellationToken cancellationToken);
    Task<HackerNewsStory> GetStoryAsync(long storyId, CancellationToken cancellationToken);
}

public class HackerNewsClient(
    IHttpClientFactory clientFactory,
    HackerNewsCacheOptions cacheOptions)
    : IHackerNewsClient
{
    private const string IdPlaceholder = "{Placeholder}";

    private readonly Uri baseAddress = new("https://hacker-news.firebaseio.com/");
    private readonly MemoryCache cache = new(Options.Create(new MemoryCacheOptions { SizeLimit = cacheOptions.MaxSize }));

    public Task<ImmutableArray<long>> GetBestStoriesAsync(CancellationToken cancellationToken)
    {
        const string BestStoriesV0 = "v0/beststories.json";
        return GetFromHackerNewsAsync(
            BestStoriesV0,
            async (client, cancellationToken) =>
            {
                var bestStories = await client.GetFromJsonAsync<long[]>(BestStoriesV0, cancellationToken);
                return bestStories?.ToImmutableArray() ?? throw new NullReferenceException($"Expected: array of Ids from: {BestStoriesV0} but got null");
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
                var bestStories = await client.GetFromJsonAsync<HackerNewsItem>(itemUrl, cancellationToken);
                return bestStories ?? throw new NullReferenceException($"Expected: {nameof(HackerNewsItem)} from: {itemUrl} but got null");
            },
            cancellationToken);
    }

    public async Task<HackerNewsStory> GetStoryAsync(long storyId, CancellationToken cancellationToken)
    {
        var storyItem = await GetItemAsync(storyId, cancellationToken);

        // TODO
        throw null;
    }

    private Task<TResult> GetFromHackerNewsAsync<TResult>(
        string requestUri,
        Func<HttpClient, CancellationToken, ValueTask<TResult>> resultFactory,
        CancellationToken cancellationToken)
        => cache.GetOrCreateAsync(requestUri, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = cacheOptions.AbsoluteExpirationRelativeToNow;
            entry.Size = 1;
            using var client = clientFactory.CreateClient();
            client.BaseAddress = baseAddress;
            var result = await resultFactory(client, cancellationToken);
            return result;
        })!;
}
