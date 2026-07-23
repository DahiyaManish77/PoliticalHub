using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.ViewModels
{
    public class AccountRegisterVM
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(200)]
        [Display(Name = "Email address")]
        public string EmailAddress { get; set; }

        [Phone]
        [StringLength(20)]
        [Display(Name = "Mobile number")]
        public string MobileNumber { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }
    }
}
