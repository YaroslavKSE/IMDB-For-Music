namespace MusicLists.Domain;

public class List
{
    public Guid ListId { get; set; }
    public string UserId { get; set; }
    public string ListType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ListName { get; set; }
    public string ListDescription { get; set; }
    public bool IsRanked { get; set; }
    public List<ListItem> Items { get; set; }
    public int Likes { get; set; }
    public int Comments { get; set; }

    public List(string userId, string listType, string listName, string listDescription, bool isRanked)
    {
        ListId = Guid.NewGuid();
        UserId = userId;
        ListType = listType;
        ListName = listName;
        ListDescription = listDescription;
        IsRanked = isRanked;
        CreatedAt = DateTime.UtcNow;
        Likes = 0;
        Comments = 0;
        Items = new List<ListItem>();
    }

    public List(Guid listId, string listType, string listName, string listDescription, bool isRanked, int likes, int comments, DateTime createdAt, List<ListItem> items)
    {
        ListId = listId;
        ListType = listType;
        ListName = listName;
        ListDescription = listDescription;
        IsRanked = isRanked;
        Likes = likes;
        Comments = comments;
        CreatedAt = createdAt;
        Items = items;
    }

}