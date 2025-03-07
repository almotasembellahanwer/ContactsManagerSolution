using System.ComponentModel.DataAnnotations;
namespace ContactsManager.Core.DTO
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Person name can't be blank")]
        public string PersonName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email can't be blank")]
        [EmailAddress(ErrorMessage ="You should write a proper email address")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Phone can't be blank")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Phone number should contain numbers only")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password can't be blank")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "Confirm password can't be blank")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
