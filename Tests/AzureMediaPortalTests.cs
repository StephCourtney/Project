using System;
using AzureMediaPortal;
using AzureMediaPortal.Models;
using AzureMediaPortal.Controllers;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests {
    [TestClass]
    public class AzureMediaPortalTests 
    {
        // Test the correct video is returned when a user clicks
        // on the video link
        [TestMethod]
        public void TestPublicVideoPlaybackData() 
        {
            var controller = new MediaController();
            var result = controller.PublicVideoPlayback(6) as ViewResult;
            var video = (Tuple<MediaElement,Post>)result.ViewData.Model;
            Assert.AreEqual("Perfect Plank", video.Item1.Title);
        }

        // Test the comments are being posted on the correct video
        [TestMethod]
        public void TestPostData() 
        {
            var controller = new PostsController();
            var result = controller.Details(10) as ViewResult;
            var post = (Post)result.ViewData.Model;
            Assert.AreEqual(6, post.VideoID);
        }
    }
}
