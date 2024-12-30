namespace CodingTestSTDR.Parallelisms;

public sealed class PerKeySynchronizer : IDisposable
{
    private SemaphoreSlim[] pool;

    public PerKeySynchronizer(int maxDegreeOfParallelism)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDegreeOfParallelism, 1);
        pool = new SemaphoreSlim[maxDegreeOfParallelism];
        for (var i = 0; i < pool.Length; i++)
        {
            pool[i] = new SemaphoreSlim(1, 1);
        }
    }

    public Task<TResult> SynchronizeAsync<TKey, TArgument, TResult>(
        TKey key,
        TArgument argument,
        Func<TArgument, CancellationToken, Task<TResult>> resultFactory,
        CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        var pool_ = pool;
        ObjectDisposedException.ThrowIf(pool_ == null, this);
        return Core(pool_, key, argument, resultFactory, cancellationToken);

        static async Task<TResult> Core(
            SemaphoreSlim[] pool,
            TKey key,
            TArgument argument,
            Func<TArgument, CancellationToken, Task<TResult>> resultFactory,
            CancellationToken cancellationToken)
        {
            var index = (uint)key.GetHashCode() % pool.Length;
            var semaphore = pool[index];
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                return await resultFactory(argument, cancellationToken);
            }
            finally
            {
                _ = semaphore.Release();
            }
        }
    }

    public void Dispose()
    {
        var original = Interlocked.Exchange(ref pool!, null);
        if (original != null)
        {
            Array.ForEach(original, semaphore => semaphore.Dispose());
            GC.SuppressFinalize(this);
        }
    }
}
