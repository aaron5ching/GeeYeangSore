using System.ComponentModel.DataAnnotations;

namespace GeeYeangSore.ViewModels
{
    public class NewsViewModel
    {
        [Required(ErrorMessage = "請輸入文章標題")]
        public string HTitle { get; set; }

        [Required(ErrorMessage = "請輸入文章內容")]
        public string HContent { get; set; }
    }
}