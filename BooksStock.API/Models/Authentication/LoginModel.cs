using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BooksStock.API.Models.Authentication
{
    public class LoginModel
    {
        [Required(ErrorMessage = "User login required")]
        public string UserLogin {  get; set; } = string.Empty!;

        [Required(ErrorMessage = "Password is required")]
        [PasswordPropertyText(true)]
        [DataType(DataType.Password)] 
        public string Password { get; set; } = string.Empty!;
    }
}
