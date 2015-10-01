using Common;

namespace ExigoService
{
    public interface IMarketConfiguration
    {
        MarketName MarketName { get; }
        IOrderConfiguration Orders { get; }
        IOrderConfiguration BackOfficeOrders { get; }
        IAutoOrderConfiguration AutoOrders { get; }
        IAutoOrderConfiguration BackOfficeAutoOrders { get; }

        IOrderConfiguration EnrollmentOrders { get; }
    }
}