using System.ComponentModel.DataAnnotations;

namespace MusicCatalogService.Core.Requests;

public class BatchIdsRequest
{
    [Required]
    public List<string> Ids { get; set; } = new();
}