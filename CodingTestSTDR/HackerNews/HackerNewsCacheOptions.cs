namespace CodingTestSTDR.HackerNews;

public sealed record HackerNewsCacheOptions(int MaxSize, TimeSpan AbsoluteExpirationRelativeToNow);