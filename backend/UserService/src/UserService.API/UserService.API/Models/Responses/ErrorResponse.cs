namespace UserService.API.Models.Responses;

public class ErrorResponse
{
    public string Code { get; set; }
    public string Message { get; set; }
    public string TraceId { get; set; }
}