using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;

namespace TGVL.Models
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Số điện thoại không đúng")]
        public string PhoneNumber { get; set; }

        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
        public string Username { get; set; }

        // Edit Profile
        [StringLength(100, ErrorMessage = "{0} phải dài ít nhất {2} kí tự.", MinimumLength = 6)]
        public string Fullname { get; set; }

        [StringLength(100, ErrorMessage = "{0} phải dài ít nhất {2} kí tự.", MinimumLength = 6)]
        public string Address { get; set; }

        [Display(Name = "Date Of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateOfBirth { get; set; }

       
    }

    public class CreateShopViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "{0} phải dài ít nhất {2} kí tự.", MinimumLength = 6)]
        public string Fullname { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} phải dài ít nhất {2} kí tự.", MinimumLength = 6)]
        public string Address { get; set; }
    }

    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    public class SetPasswordViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "{0} phải dài ít nhất {2} kí tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu chưa khớp.")]
        public string ConfirmPassword { get; set; }

        public string Username { get; set; }
        public bool HasPassword { get; set; }
        public string Address { get; set; }
    }

    public class ChangePasswordViewModel
    {
        public string Username { get; set; }
        public bool HasPassword { get; set; }
        public string Address { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} phải dài ít nhất {2} kí tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu mới chưa khớp")]
        public string ConfirmPassword { get; set; }
    }

    //
    // Add phone number
    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Number { get; set; }
    }

    public class VerifyPhoneNumberViewModel
    {
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }

    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    }

    public class ChangeUserNameViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "{0} phải dài ít nhất {2} kí tự.", MinimumLength = 6)]
        [Display(Name = "Username")]
        public string Username { get; set; }
    }

    //
    //
    public class UpdateShopInformation
    {

    }
}