namespace MusicCatalogService.Core.Spotify;

/// <summary>
/// Represents the result of a token operation, which can either succeed with a token or fail
/// </summary>
public class TokenResult
{
    public string Token { get; private set; }
    
    public bool IsSuccess { get; private set; }
    
    private TokenResult(string token, bool isSuccess)
    {
        Token = token;
        IsSuccess = isSuccess;
    }
    
    public static TokenResult Success(string token) => new TokenResult(token, true);
    
    public static TokenResult Failure() => new TokenResult(null, false);
}