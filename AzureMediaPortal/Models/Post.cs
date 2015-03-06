using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureMediaPortal.Models 
{
    public class Post 
    {
        public int PostID { get; set; }
        public int VideoID { get; set; }
        public string UserID { get; set; }
        public string MessageBody { get; set; }
        public IEnumerable<Comment> Replies { get; set; }
    }
}