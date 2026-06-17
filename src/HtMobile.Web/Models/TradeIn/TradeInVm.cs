using System.ComponentModel.DataAnnotations;

namespace HtMobile.Web.Models.TradeIn;

public class TradeInVm
{
    [Required(ErrorMessage = "Vui lòng chọn dòng máy")]
    public string DeviceKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn tình trạng máy")]
    public string ConditionKey { get; set; } = string.Empty;
}
