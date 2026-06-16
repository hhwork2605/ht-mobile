using HtMobile.Domain.Common;

namespace HtMobile.Domain.Entities.Customers;

/// <summary>Địa chỉ nhận hàng của khách (CustomerId = ApplicationUser.Id). SPEC §7.</summary>
public class Address : BaseAuditableEntity
{
    public long CustomerId { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
