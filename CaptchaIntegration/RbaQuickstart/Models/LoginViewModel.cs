using System.ComponentModel.DataAnnotations;

namespace RbaQuickstart.Models
{
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public string? ReturnUrl { get; set; }
        public bool ShowCaptcha { get; set; }

    }
}
