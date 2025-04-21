using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MusicLists.Infrastructure.Entities;

public class ListEntity
{
    [Key]
    public Guid ListId { get; set; }
    //index
    public string UserId { get; set; }
    public string ListType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ListName { get; set; }
    public string ListDescription { get; set; }
    public bool IsRanked { get; set; }
    //index
    public bool IsScoreDirty { get; set; } = true;
    //index
    public float HotScore { get; set; } = 0;

    // Navigation properties
    public virtual ICollection<ListItemEntity> Items { get; set; } = new List<ListItemEntity>();
    public virtual ICollection<ListLikeEntity> Likes { get; set; } = new List<ListLikeEntity>();
    public virtual ICollection<ListCommentEntity> Comments { get; set; } = new List<ListCommentEntity>();
}