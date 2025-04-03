using System.ComponentModel.DataAnnotations;

namespace GeeYeangSore.ViewModels
{
    public class CLoginViewModel
    {
        [Required(ErrorMessage = "請輸入帳號")]
        public string txtAccount { get; set; }

        [Required(ErrorMessage = "請輸入密碼")]
        public string txtPassword { get; set; }
    }
}