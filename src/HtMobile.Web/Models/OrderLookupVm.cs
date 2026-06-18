using System.ComponentModel.DataAnnotations;
using HtMobile.Application.Features.Orders.Dtos;

namespace HtMobile.Web.Models;

/// <summary>Tra cứu đơn cho khách vãng lai: mã đơn + SĐT đã dùng khi đặt.</summary>
public class OrderLookupVm
{
    [Required(ErrorMessage = "Nhập mã đơn hàng.")]
    [Display(Name = "Mã đơn hàng")]
    public string? Code { get; set; }

    [Required(ErrorMessage = "Nhập số điện thoại đặt hàng.")]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    /// <summary>Đã bấm tra cứu (để phân biệt lần đầu vào trang vs sau khi tìm).</summary>
    public bool Searched { get; set; }

    /// <summary>Kết quả đơn (null = không tìm thấy / sai thông tin).</summary>
    public OrderSummaryDto? Result { get; set; }
}
