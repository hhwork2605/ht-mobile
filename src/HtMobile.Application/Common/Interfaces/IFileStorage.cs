namespace HtMobile.Application.Common.Interfaces;

/// <summary>
/// Lưu/xoá file nhị phân (ảnh sản phẩm…). Hiện thực ở Infrastructure (local disk / cloud).
/// Trả về <b>đường dẫn tương đối</b> (vd <c>/uploads/products/abc.jpg</c>); nơi gọi tự ghép host
/// thành URL tuyệt đối nếu cần (storefront ở host khác API).
/// </summary>
public interface IFileStorage
{
    /// <summary>Lưu file vào <paramref name="folder"/>, trả đường dẫn công khai tương đối. Tên file sinh ngẫu nhiên, giữ phần mở rộng.</summary>
    Task<string> SaveAsync(Stream content, string fileName, string folder, CancellationToken ct = default);

    /// <summary>Xoá file theo URL/đường dẫn đã lưu (chấp nhận cả URL tuyệt đối lẫn tương đối). No-op nếu không thuộc kho này.</summary>
    Task DeleteAsync(string urlOrPath, CancellationToken ct = default);
}
