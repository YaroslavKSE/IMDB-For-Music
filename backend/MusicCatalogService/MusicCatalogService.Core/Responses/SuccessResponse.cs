namespace MusicCatalogService.Core.Responses;

public class SuccessResponse
{
    public string Message { get; set; }
    public object Data { get; set; }
}

// Specific success responses
public class SaveItemSuccessResponse : SuccessResponse
{
    public Guid CatalogItemId { get; set; }
    
    public SaveItemSuccessResponse(Guid catalogItemId, string message = "Item saved successfully")
    {
        CatalogItemId = catalogItemId;
        Message = message;
    }
}