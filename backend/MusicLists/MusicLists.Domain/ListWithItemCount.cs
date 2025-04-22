namespace MusicLists.Domain;

public class ListWithItemCount : List
{
    public int TotalItems { get; set; }

    public ListWithItemCount(List list, int totalItems)
        : base(list.ListId, list.ListType, list.ListName, list.ListDescription, list.IsRanked, list.Likes, list.Comments, list.CreatedAt, list.Items)
    {
        TotalItems = totalItems;
        UserId = list.UserId;
    }
}