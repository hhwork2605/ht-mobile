using HtMobile.Application.Common.Interfaces;
using HtMobile.Domain.Entities.Services;
using HtMobile.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Application.Features.StockNotifications;

/// <summary>Kết quả đăng ký theo dõi hàng về.</summary>
public enum StockNotifyResult
{
    Subscribed,
    AlreadySubscribed,
    InvalidContact,
    VariantNotFound,
    StillInStock
}

/// <summary>Đăng ký nhận thông báo khi variant hết hàng có lại (P3-04). Chưa gửi thông báo thật.</summary>
public class StockNotificationService
{
    private readonly IApplicationDbContext _db;

    public StockNotificationService(IApplicationDbContext db) => _db = db;

    public async Task<StockNotifyResult> SubscribeAsync(long variantId, string? contact, CancellationToken ct = default)
    {
        if (!ContactValidator.IsValid(contact)) return StockNotifyResult.InvalidContact;
        contact = contact!.Trim();

        // Chỉ biến thể con (ProductParentId != null) mới theo dõi được; model cha không bán trực tiếp.
        var status = await _db.Products
            .Where(p => p.Id == variantId && p.ProductParentId != null)
            .Select(p => (ProductStatus?)p.Status)
            .FirstOrDefaultAsync(ct);
        if (status is null) return StockNotifyResult.VariantNotFound;
        if (status == ProductStatus.Active) return StockNotifyResult.StillInStock;   // còn hàng → không cần theo dõi
        // Chỉ OutOfStock mới có ý nghĩa theo dõi; Discontinued (ngừng KD) sẽ không có hàng lại → coi như không khả dụng.
        if (status != ProductStatus.OutOfStock) return StockNotifyResult.VariantNotFound;

        var already = await _db.StockNotifications
            .AnyAsync(n => n.ProductId == variantId && n.Contact == contact && !n.Notified, ct);
        if (already) return StockNotifyResult.AlreadySubscribed;   // idempotent

        _db.StockNotifications.Add(new StockNotification
        {
            ProductId = variantId,
            Contact = contact,
            Notified = false
        });
        await _db.SaveChangesAsync(ct);
        return StockNotifyResult.Subscribed;
    }
}
