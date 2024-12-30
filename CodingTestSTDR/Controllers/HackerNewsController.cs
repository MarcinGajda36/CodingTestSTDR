namespace CodingTestSTDR.Controllers;

using System.Collections.Immutable;
using CodingTestSTDR.HackerNews;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public sealed class HackerNewsController(IHackerNewsService hackerNewsService)
    : ControllerBase
{
    [HttpGet("GetBestStoriesIds")]
    public Task<ImmutableArray<long>> GetBestStoriesIdsAsync(CancellationToken cancellationToken)
        => hackerNewsService.GetBestStoriesIdsAsync(cancellationToken);

    [HttpGet("GetBestStories/{storyCount}")]
    public Task<HackerNewsStory[]> GetBestStoriesAsync(int storyCount, CancellationToken cancellationToken)
        => hackerNewsService.GetBestStoriesAsync(storyCount, cancellationToken);

    [HttpGet("GetStory/{storyId}")]
    public Task<HackerNewsStory> GetStoryAsync(long storyId, CancellationToken cancellationToken)
        => hackerNewsService.GetStoryAsync(storyId, cancellationToken);
}
