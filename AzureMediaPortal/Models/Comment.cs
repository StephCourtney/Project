using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureMediaPortal.Models {
    public class Comment {
        public int CommentID { get; set; }

        public string CommentText { get; set; }

        public System.DateTime CommentTime { get; set; }

        public UserProfile UserProfile { get; set; }
    }
}