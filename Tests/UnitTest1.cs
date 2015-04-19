using System;
using AzureMediaPortal;
using AzureMediaPortal.Models;
using AzureMediaPortal.Controllers;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests {
    [TestClass]
    public class UnitTest1 
    {
        // Test to ensure the correct video is returned when a user clicks
        // on the video link
        [TestMethod]
        public void TestPublicVideoPlaybackData() 
        {
            var controller = new MediaController();
            var result = controller.PublicVideoPlayback(6) as ViewResult;
            var video = (Tuple<MediaElement,Post>)result.ViewData.Model;
            Assert.AreEqual("Perfect Plank", video.Item1.Title);
        }

        [TestMethod]
        public void TestPublicVideoPlaybackData() 
        {
            var controller = new MediaController();
            var result = controller.PublicVideoPlayback(6) as ViewResult;
            var video = (Tuple<MediaElement, Post>)result.ViewData.Model;
            Assert.AreEqual("Perfect Plank", video.Item1.Title);
        }
    }
}
