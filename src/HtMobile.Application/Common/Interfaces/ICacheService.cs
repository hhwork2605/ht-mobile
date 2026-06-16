namespace HtMobile.Application.Common.Interfaces;

/// <summary>Trừu tượng cache (hiện thực Redis ở Infrastructure).</summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);

    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default);

    /// <summary>Lấy từ cache; nếu chưa có thì gọi <paramref name="factory"/>, lưu lại rồi trả về.</summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null, CancellationToken ct = default);

    Task RemoveAsync(string key, CancellationToken ct = default);
}
