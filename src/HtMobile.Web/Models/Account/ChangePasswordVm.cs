using System.ComponentModel.DataAnnotations;

namespace HtMobile.Web.Models.Account;

public class ChangePasswordVm
{
    [Required(ErrorMessage = "Nhập mật khẩu hiện tại.")]
    [Display(Name = "Mật khẩu hiện tại")]
    public string? CurrentPassword { get; set; }

    [Required(ErrorMessage = "Nhập mật khẩu mới.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới tối thiểu 6 ký tự.")]
    [Display(Name = "Mật khẩu mới")]
    public string? NewPassword { get; set; }

    [Required(ErrorMessage = "Nhập lại mật khẩu mới.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu nhập lại không khớp.")]
    [Display(Name = "Nhập lại mật khẩu mới")]
    public string? ConfirmPassword { get; set; }
}
