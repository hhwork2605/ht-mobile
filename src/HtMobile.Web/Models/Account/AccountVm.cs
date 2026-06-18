using HtMobile.Application.Features.Customers;
using HtMobile.Application.Features.Orders.Dtos;

namespace HtMobile.Web.Models.Account;

public class AccountVm
{
    public string? FullName { get; set; }
    public string Email { get; set; } = string.Empty;
    public IReadOnlyList<OrderSummaryDto> Orders { get; set; } = Array.Empty<OrderSummaryDto>();
    public IReadOnlyList<AddressDto> Addresses { get; set; } = Array.Empty<AddressDto>();

    /// <summary>Tab mở sẵn: orders | addresses | info | password.</summary>
    public string ActiveTab { get; set; } = "orders";
}
