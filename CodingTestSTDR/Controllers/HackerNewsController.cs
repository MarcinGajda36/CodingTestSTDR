namespace CodingTestSTDR.Controllers;

using System.Collections.Immutable;
using CodingTestSTDR.HackerNews;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public sealed class HackerNewsController(IHackerNewsClient hackerNewsClient)
    : ControllerBase
{
    [HttpGet(template: "GetBestStories")]
    public Task<ImmutableArray<long>> GetBestStoriesAsync(CancellationToken cancellationToken)
        => hackerNewsClient.GetBestStoriesAsync(cancellationToken);

    [HttpGet(template: "GetItem/{itemId}")]
    public Task<HackerNewsItem> GetItemAsync(long itemId, CancellationToken cancellationToken)
        => hackerNewsClient.GetItemAsync(itemId, cancellationToken);

    [HttpGet(template: "GetStory/{storyId}")]
    public Task<HackerNewsStory> GetStoryAsync(long storyId, CancellationToken cancellationToken)
        => hackerNewsClient.GetStoryAsync(storyId, cancellationToken);
}
