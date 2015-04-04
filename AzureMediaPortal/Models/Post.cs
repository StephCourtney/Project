using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AzureMediaPortal.Models 
{
    public class Post 
    {
        public int PostID { get; set; }
        public int VideoID { get; set; }
        public string UserID { get; set; }
        [Display(Name="Comment: ")]
        public string MessageBody { get; set; }
        public string CommentTime {get; set;}
        
    }
}