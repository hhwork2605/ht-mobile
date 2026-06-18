using System.ComponentModel.DataAnnotations;

namespace HtMobile.Web.Models.Account;

public class AddressVm
{
    [Required(ErrorMessage = "Nhập tên người nhận.")]
    [StringLength(100)]
    [Display(Name = "Người nhận")]
    public string? Recipient { get; set; }

    [Required(ErrorMessage = "Nhập số điện thoại.")]
    [RegularExpression(@"^(0|\+84)\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ.")]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Nhập địa chỉ.")]
    [StringLength(250, MinimumLength = 5)]
    [Display(Name = "Địa chỉ")]
    public string? AddressLine { get; set; }

    [Display(Name = "Đặt làm mặc định")]
    public bool IsDefault { get; set; }
}
