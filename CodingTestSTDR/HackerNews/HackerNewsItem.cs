namespace CodingTestSTDR.HackerNews;

using System.Collections.Immutable;

public enum HackerNewsStoryType
{
    Job = 0,
    Story,
    Comment,
    Poll,
    Pollopt
}
public sealed record HackerNewsItem(string By, long Descendants, long Id, ImmutableArray<long> Kids, int Score, long Time, string Title, HackerNewsStoryType Type, string Url);
public sealed record HackerNewsStory(string Title, string Uri, string PostedBy, DateTime Time, int Score, int CommentCount);
