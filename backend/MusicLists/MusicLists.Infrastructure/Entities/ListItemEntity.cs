using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MusicLists.Infrastructure.Entities;

public class ListItemEntity
{
    [Key]
    public Guid ListItemId { get; set; }
    public Guid ListId { get; set; }
    //index
    public string ItemId { get; set; }
    public int Number { get; set; }

    // Navigation property
    [ForeignKey("ListId")]
    public virtual ListEntity List { get; set; }
}