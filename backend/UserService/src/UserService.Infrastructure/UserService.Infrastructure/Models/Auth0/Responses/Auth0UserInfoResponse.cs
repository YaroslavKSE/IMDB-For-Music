using System.Text.Json.Serialization;

namespace UserService.Infrastructure.Models.Auth0.Responses;

public class Auth0UserInfoResponse
{
    [JsonPropertyName("sub")]
    public string Sub { get; set; }
    
    [JsonPropertyName("given_name")]
    public string GivenName { get; set; }
    
    [JsonPropertyName("family_name")]
    public string FamilyName { get; set; }
    
    [JsonPropertyName("nickname")]
    public string Nickname { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("picture")]
    public string Picture { get; set; }
    
    [JsonPropertyName("email")]
    public string Email { get; set; }
    
    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; set; }
}