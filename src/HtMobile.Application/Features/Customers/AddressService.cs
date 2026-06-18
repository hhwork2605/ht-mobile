using HtMobile.Application.Common.Interfaces;
using HtMobile.Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Application.Features.Customers;

/// <summary>1 địa chỉ nhận hàng của khách.</summary>
public record AddressDto(long Id, string Recipient, string Phone, string AddressLine, bool IsDefault);

/// <summary>Dữ liệu thêm/sửa địa chỉ.</summary>
public record AddressInput(string Recipient, string Phone, string AddressLine, bool IsDefault);

/// <summary>Sổ địa chỉ nhận hàng của Customer (CRUD + đặt mặc định). Mọi thao tác giới hạn theo customerId (sở hữu).</summary>
public class AddressService
{
    private readonly IApplicationDbContext _db;

    public AddressService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<AddressDto>> ListAsync(long customerId, CancellationToken ct = default)
        => await _db.Addresses.AsNoTracking()
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.Id)
            .Select(a => new AddressDto(a.Id, a.Recipient, a.Phone, a.AddressLine, a.IsDefault))
            .ToListAsync(ct);

    /// <summary>Địa chỉ mặc định (hoặc mới nhất) để tự điền checkout; null nếu chưa có.</summary>
    public Task<AddressDto?> GetDefaultAsync(long customerId, CancellationToken ct = default)
        => _db.Addresses.AsNoTracking()
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.Id)
            .Select(a => new AddressDto(a.Id, a.Recipient, a.Phone, a.AddressLine, a.IsDefault))
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(long customerId, AddressInput input, CancellationToken ct = default)
    {
        var hasAny = await _db.Addresses.AnyAsync(a => a.CustomerId == customerId, ct);
        var makeDefault = input.IsDefault || !hasAny;   // địa chỉ đầu tiên auto mặc định
        if (makeDefault) await ClearDefaultAsync(customerId, ct);

        _db.Addresses.Add(new Address
        {
            CustomerId = customerId,
            Recipient = input.Recipient.Trim(),
            Phone = input.Phone.Trim(),
            AddressLine = input.AddressLine.Trim(),
            IsDefault = makeDefault,
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> UpdateAsync(long customerId, long id, AddressInput input, CancellationToken ct = default)
    {
        var addr = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == customerId, ct);
        if (addr is null) return false;

        if (input.IsDefault && !addr.IsDefault) await ClearDefaultAsync(customerId, ct);
        addr.Recipient = input.Recipient.Trim();
        addr.Phone = input.Phone.Trim();
        addr.AddressLine = input.AddressLine.Trim();
        if (input.IsDefault) addr.IsDefault = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(long customerId, long id, CancellationToken ct = default)
    {
        var addr = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == customerId, ct);
        if (addr is null) return false;
        _db.Addresses.Remove(addr);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> SetDefaultAsync(long customerId, long id, CancellationToken ct = default)
    {
        var addr = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == customerId, ct);
        if (addr is null) return false;
        await ClearDefaultAsync(customerId, ct);
        addr.IsDefault = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private async Task ClearDefaultAsync(long customerId, CancellationToken ct)
    {
        var current = await _db.Addresses.Where(a => a.CustomerId == customerId && a.IsDefault).ToListAsync(ct);
        foreach (var a in current) a.IsDefault = false;
    }
}
