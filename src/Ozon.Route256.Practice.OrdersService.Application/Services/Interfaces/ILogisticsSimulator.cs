namespace Ozon.Route256.Practice.OrderService.Application.Services;

public interface ILogisticsSimulator
{
    /// <summary>
    /// Отправляет запрос на отмену указанного заказа.
    /// </summary>
    /// <param name="orderId">ID заказа</param>
    /// <returns>Сообщение об ошибке, или пустая строка в случае успеха</returns>
    public Task<string> CancelOrder(long orderId);
}
