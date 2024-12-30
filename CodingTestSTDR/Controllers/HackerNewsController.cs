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
    public async Task<ActionResult<HackerNewsStory[]>> GetBestStoriesAsync(int storyCount, CancellationToken cancellationToken)
    {
        if (storyCount < 1)
        {
            return BadRequest(new { Message = "Story count has to be at least 1.", RequestedStoryCount = storyCount });
        }

        return Ok(await hackerNewsService.GetBestStoriesAsync(storyCount, cancellationToken));
    }

    [HttpGet("GetStory/{storyId}")]
    public async Task<ActionResult<HackerNewsStory>> GetStoryAsync(long storyId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await hackerNewsService.GetStoryAsync(storyId, cancellationToken));
        }
        catch (HackerNewsNotFoundException notFound)
        {
            return NotFound(new { Message = $"StoryId: '{notFound.ItemId}' was not found.", RequestedStoryId = storyId });
        }
    }
}
