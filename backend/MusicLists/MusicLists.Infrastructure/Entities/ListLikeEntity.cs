using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MusicLists.Infrastructure.Entities;

public class ListLikeEntity
{
    [Key]
    public Guid LikeId { get; set; }
    public Guid ListId { get; set; }
    public string UserId { get; set; }
    public DateTime LikedAt { get; set; }

    // Navigation property
    [ForeignKey("ListId")]
    public virtual ListEntity List { get; set; }
}