namespace CodingTestSTDR.HackerNews;

public class HackerNewsNotFoundException(long itemId)
    : Exception(message: $"Id: '{itemId}' not found.")
{
    public long ItemId { get; } = itemId;
}
