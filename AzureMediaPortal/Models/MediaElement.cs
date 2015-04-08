using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AzureMediaPortal.Models
{
    public class MediaElement
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        [Display(Name = "Video Title")]
        public string Title { get; set; }
        public string FileUrl { get; set; }
        public string AssetId { get; set; }
        [Display(Name = "Public")]
        public bool IsPublic { get; set; }
        public int VideoID { get; set;}
        [ForeignKey("VideoID")]
        public List<Post> VideoPost {get; set;}
        [Display(Name = "Upload Time and Date")]
        public string UploadTime { get; set; }
    }

}