using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BooksStock.API.Models.Authentication
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Unique User Login is Required")]
        public string Login { get; set; } = null!;

        [Required(ErrorMessage = "Unique Email is Required")]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is Required")]
        [DataType(DataType.Password)]
        [PasswordPropertyText(true)]
        public string Password { get; set; } = null!;
    }
}
