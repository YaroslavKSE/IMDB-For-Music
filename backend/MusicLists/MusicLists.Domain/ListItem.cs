namespace MusicLists.Domain;

public class ListItem
{
    public string SpotifyId { get; set; }
    public int Number { get; set; }

    public ListItem(string spotifyId, int number)
    {
        SpotifyId = spotifyId;
        Number = number;
    }
}