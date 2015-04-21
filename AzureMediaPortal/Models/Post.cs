using System.ComponentModel.DataAnnotations;

namespace AzureMediaPortal.Models {
    public class Post {
        public int PostID { get; set; }
        [Display(Name = "Video Title")]
        public string VideoTitle { get; set; }
        public int VideoID { get; set; }
        public string UserID { get; set; }
        [Required]
        [Display(Name = "Comment ")]
        public string MessageBody { get; set; }
        public string CommentTime { get; set; }
    }
}