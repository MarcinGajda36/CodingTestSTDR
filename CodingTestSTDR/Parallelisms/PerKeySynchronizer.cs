namespace CodingTestSTDR.Parallelisms;

public class PerKeySynchronizer : IDisposable
{
    private readonly SemaphoreSlim[] pool;
    private bool disposedValue;

    public PerKeySynchronizer(int maxDegreeOfParallelism = 17)
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

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Array.ForEach(pool, pool_ => pool_.Dispose());
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
