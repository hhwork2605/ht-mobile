using System.ComponentModel.DataAnnotations;

namespace HtMobile.Web.Models.Checkout;

public class CheckoutVm
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2–100 ký tự")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^(0|\+84)\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ (VD: 0901234567)")]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
    [StringLength(250, MinimumLength = 5, ErrorMessage = "Địa chỉ từ 5–250 ký tự")]
    [Display(Name = "Địa chỉ nhận hàng")]
    public string Address { get; set; } = string.Empty;

    /// <summary>store | delivery (Phase 2 đều miễn phí).</summary>
    [RegularExpression("store|delivery", ErrorMessage = "Phương thức giao không hợp lệ")]
    public string ShipMethod { get; set; } = "delivery";

    /// <summary>Ghi chú đơn hàng (tuỳ chọn) — đính kèm vào địa chỉ giao khi đặt (chưa có cột riêng, tránh migration).</summary>
    [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
    [Display(Name = "Ghi chú đơn hàng")]
    public string? Note { get; set; }
}
