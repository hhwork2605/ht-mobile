namespace HtMobile.Application.Features.Checkout.Dtos;

/// <summary>Kết quả đặt hàng thành công.</summary>
public record PlaceOrderResult(long OrderId)
{
    /// <summary>Mã đơn hiển thị cho khách (không thêm cột DB).</summary>
    public string Code => $"SD{OrderId:D6}";
}
