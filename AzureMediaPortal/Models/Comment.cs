using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureMediaPortal.Models {
    public class Comment {
        public int CommentID { get; set; }

        public String UserID { get; set; }

        public String CommentText { get; set; }

        public String CommentTime { get; set; }

        public int VideoID { get; set; }
    }
}