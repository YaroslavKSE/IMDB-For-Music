namespace UserService.API.Models.Requests
{
    public class RegisterUserRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; } 
        public string Name { get; set; }
        public string Surname { get; set; }
    }
}