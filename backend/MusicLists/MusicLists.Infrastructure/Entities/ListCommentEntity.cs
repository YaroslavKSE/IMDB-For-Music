using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MusicLists.Infrastructure.Entities;

public class ListCommentEntity
{
    [Key]
    public Guid CommentId { get; set; }
    //index
    public Guid ListId { get; set; }
    //index
    public string UserId { get; set; }
    public DateTime CommentedAt { get; set; }
    public string CommentText { get; set; }

    // Navigation property
    [ForeignKey("ListId")]
    public virtual ListEntity List { get; set; }
}