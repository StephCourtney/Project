using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AzureMediaPortal.Models
{
    public class MediaElement
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string FileUrl { get; set; }
        public string AssetId { get; set; }
        public bool IsPublic { get; set; }
        public int VideoID { get; set;}
        [ForeignKey("VideoID")]
        public List<Post> VideoPost {get; set;}
    }

}