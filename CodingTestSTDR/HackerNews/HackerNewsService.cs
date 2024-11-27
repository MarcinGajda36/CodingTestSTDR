﻿namespace CodingTestSTDR.HackerNews;

using System.Collections.Immutable;

public interface IHackerNewsService
{
    Task<ImmutableArray<long>> GetBestStoriesAsync(CancellationToken cancellationToken);
    Task<HackerNewsStory> GetStoryAsync(long storyId, CancellationToken cancellationToken);
}

public class HackerNewsService(IHackerNewsClient hackerNewsClient)
    : IHackerNewsService
{
    public Task<ImmutableArray<long>> GetBestStoriesAsync(CancellationToken cancellationToken)
        => hackerNewsClient.GetBestStoriesAsync(cancellationToken);

    public async Task<HackerNewsStory> GetStoryAsync(long storyId, CancellationToken cancellationToken)
    {
        var storyItem = await hackerNewsClient.GetItemAsync(storyId, cancellationToken);
        var time = DateTime.UnixEpoch.AddMilliseconds(storyItem.Time); // maybe? 
        // TODO:
        // 1) parse Time
        // 2) for CommentCount i can go go through all storyItem.Kids and count HackerNewsStoryType.Comment, maybe SelectWhenAllAsync(int maxRequests)?
        throw null;
    }
}