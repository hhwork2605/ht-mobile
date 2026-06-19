using HtMobile.Application.Common.Interfaces;

namespace HtMobile.Infrastructure.Storage;

/// <summary>
/// Lưu file xuống đĩa cục bộ dưới <c>{webRoot}/uploads/&lt;folder&gt;</c>, phục vụ qua static files
/// (host gọi <c>UseStaticFiles()</c>). Trả đường dẫn tương đối <c>/uploads/&lt;folder&gt;/&lt;file&gt;</c>.
/// </summary>
public sealed class LocalFileStorage : IFileStorage
{
    private const string PublicPrefix = "/uploads";
    private readonly string _uploadsRoot;   // {webRoot}/uploads

    public LocalFileStorage(string webRootPath)
        => _uploadsRoot = Path.Combine(webRootPath, "uploads");

    public async Task<string> SaveAsync(Stream content, string fileName, string folder, CancellationToken ct = default)
    {
        var safeFolder = SanitizeSegment(folder);
        var ext = Path.GetExtension(fileName);
        if (ext.Length > 10) ext = "";                 // bỏ phần mở rộng bất thường
        var name = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";

        var dir = Path.Combine(_uploadsRoot, safeFolder);
        Directory.CreateDirectory(dir);

        var fullPath = Path.Combine(dir, name);
        await using (var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            await content.CopyToAsync(fs, ct);

        return $"{PublicPrefix}/{safeFolder}/{name}";
    }

    public Task DeleteAsync(string urlOrPath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(urlOrPath)) return Task.CompletedTask;

        // Chấp nhận URL tuyệt đối (http://host/uploads/...) lẫn tương đối (/uploads/...).
        var idx = urlOrPath.IndexOf(PublicPrefix + "/", StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return Task.CompletedTask;          // không thuộc kho này

        var relative = urlOrPath[(idx + PublicPrefix.Length + 1)..]; // bỏ "/uploads/"
        var combined = Path.GetFullPath(Path.Combine(_uploadsRoot, relative.Replace('/', Path.DirectorySeparatorChar)));

        // Chặn path traversal: chỉ xoá trong _uploadsRoot.
        var rootFull = Path.GetFullPath(_uploadsRoot);
        if (combined.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase) && File.Exists(combined))
            File.Delete(combined);

        return Task.CompletedTask;
    }

    private static string SanitizeSegment(string s)
    {
        var cleaned = new string((s ?? "").Where(c => char.IsLetterOrDigit(c) || c is '-' or '_').ToArray());
        return string.IsNullOrEmpty(cleaned) ? "misc" : cleaned.ToLowerInvariant();
    }
}
