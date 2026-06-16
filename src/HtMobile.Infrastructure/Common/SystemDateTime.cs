using HtMobile.Application.Common.Interfaces;

namespace HtMobile.Infrastructure.Common;

public class SystemDateTime : IDateTime
{
    public DateTime Now => DateTime.Now;
}
