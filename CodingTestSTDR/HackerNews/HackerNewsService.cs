namespace CodingTestSTDR.HackerNews;

using System.Collections.Immutable;
using System.Threading;

public interface IHackerNewsService
{
    Task<HackerNewsStory[]> GetBestStoriesAsync(int storiesCount, CancellationToken cancellationToken);
    Task<ImmutableArray<long>> GetBestStoriesIdsAsync(CancellationToken cancellationToken);
    Task<HackerNewsStory> GetStoryAsync(long storyId, CancellationToken cancellationToken);
}

public class HackerNewsService(IHackerNewsClient hackerNewsClient)
    : IHackerNewsService
{
    public Task<ImmutableArray<long>> GetBestStoriesIdsAsync(CancellationToken cancellationToken)
        => hackerNewsClient.GetBestStoriesIdsAsync(cancellationToken);

    public async Task<HackerNewsStory[]> GetBestStoriesAsync(int storiesCount, CancellationToken cancellationToken)
    {
        var allBestStoriesIds = await GetBestStoriesIdsAsync(cancellationToken);
        var countBestStoriesIds = allBestStoriesIds.Take(storiesCount);
        var stories = await Task.WhenAll(countBestStoriesIds.Select(id => GetStoryAsync(id, cancellationToken)));
        Array.Sort(stories, (lStory, rStory) => lStory.Score.CompareTo(rStory.Score));
        return stories;
    }

    public async Task<HackerNewsStory> GetStoryAsync(long storyId, CancellationToken cancellationToken)
    {
        var storyItem = await hackerNewsClient.GetItemAsync(storyId, cancellationToken);
        var time = DateTime.UnixEpoch.AddSeconds(storyItem.Time);
        var kids = await Task.WhenAll(
            storyItem.Kids.Select(id => hackerNewsClient.GetItemAsync(id, cancellationToken)));
        var commentCount = kids.Count(item => item.Type is HackerNewsStoryType.Comment);
        return new HackerNewsStory(storyItem.Title,
            storyItem.Url,
            storyItem.By,
            time,
            storyItem.Score,
            commentCount);
    }
}
