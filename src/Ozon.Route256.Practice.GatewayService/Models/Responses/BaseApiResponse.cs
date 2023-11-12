namespace Ozon.Route256.Practice.GatewayService.Models.Responses;

public class BaseApiResponse
{
    public bool IsSucceeded { get; set; }

    public string Message { get; set; }

    public BaseApiResponse(bool isSucceeded, string message)
    {
        IsSucceeded = isSucceeded;
        Message = message;
    }

    public static BaseApiResponse Success(string message = "Ok")
    {
        return new BaseApiResponse(isSucceeded: true, message);
    }

    public static BaseApiResponse Error(string errorMessage)
    {
        return new BaseApiResponse(isSucceeded: false, errorMessage);
    }
}
