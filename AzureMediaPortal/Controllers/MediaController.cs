﻿using AzureMediaPortal.Models;
using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AzureMediaPortal.Controllers {

    public class MediaController : Controller {
        private AzureMediaPortalContext db = new AzureMediaPortalContext();


        // GET: /Media/
        // Return media elements for secified user and list them
        // Will be seen when the user clicks on the "My Videos" tab
        [Authorize]
        public ActionResult Index() {
            var v = db.MediaElements.OrderBy(t => t.Title).Where(m => m.UserId == User.Identity.Name).ToList();
            return View(v);
        }

        // GET: Media/PublicVideos
        // Returns all of the public videos when no search parameter
        // has been entered and on first load.
        // Ordered by title A-Z
        // pagedlist enable paging on homepage, display 4 videos at a time
        [HttpGet]
        public ActionResult PublicVideos(int page = 1) {
            var v = db.MediaElements.OrderBy(t => t.Title).Where(m => m.IsPublic.Equals(true)).ToPagedList(page, 4);
            return View(v);
        }
        // POST: Media/PublicVideos
        // Returns search results. Case insensitive 
        // Ordered by title A-Z
        [HttpPost]
        public ActionResult PublicVideos(string SearchString, int page = 1) {
            IPagedList<MediaElement> videos;
            if (String.IsNullOrEmpty(SearchString)) {
                videos = db.MediaElements.Where(m => m.IsPublic.Equals(true)).OrderBy(t => t.Title).ToPagedList(page, 4);
            }
            else {
                videos = db.MediaElements.Where(v => v.Title.Contains(SearchString) && v.IsPublic.Equals(true)).OrderBy(t => t.Title).ToPagedList(page, 4);
            }

            return View(videos);
        }


        // Add the userID, upload time and url to the mediaelement for saving to the db
        // create a new list of posts for the comments section of the mediaelement
        // add the the mediaelement to the db and save.
        // called when "Save" button is clicked in the UI
        // sends the jsonresult to media-upload.js "saveDetials" function
        [Authorize]
        [HttpPost]
        public JsonResult Save(MediaElement mediaelement) {
            try {
                mediaelement.UserId = User.Identity.Name;
                mediaelement.UploadTime = DateTime.Now.ToString("HH:mm, dd MMM yy");
                mediaelement.FileUrl = GetStreamingUrl(mediaelement.AssetId);
                mediaelement.VideoPost = new List<Post>();
                db.MediaElements.Add(mediaelement);
                db.SaveChanges();
                return Json(new { Saved = true, StreamingUrl = mediaelement.FileUrl });
            }
            catch (Exception) {
                return Json(new { Saved = false });
            }
        }

        // Create the url to access the video. 
        // Creates an access policy which states how long the video can be watched (a year)
        // takes in the assetID as a parameter and finds it in the users list of videos
        // Called in above save method
        [Authorize]
        private string GetStreamingUrl(string assetId) {
            CloudMediaContext context = new CloudMediaContext(ConfigurationManager.AppSettings["MediaAccountName"], ConfigurationManager.AppSettings["MediaAccountKey"]);

            //create access policy for url (365 days)
            var daysForWhichStreamingUrlIsActive = 365;

            // find the asset(video) from the list of assets in media services account.
            var streamingAsset = context.Assets.Where(a => a.Id == assetId).FirstOrDefault();

            // assign the access policy using the stated number of days above
            IAccessPolicy accessPolicy = context.AccessPolicies.Create(streamingAsset.Name, TimeSpan.FromDays(daysForWhichStreamingUrlIsActive),
                                     AccessPermissions.Read | AccessPermissions.List);

            // create a new string to store the URL
            string streamingUrl = string.Empty;

            // Gather the assetfiles which are associated to the assedId being passed in
            // assetfiles are the video and audio files that make up the asset
            var assetFiles = streamingAsset.AssetFiles.ToList();

            // find the streaming manifest file and create a locator that will be part of the url
            var streamingAssetFile = assetFiles.Where(f => f.Name.ToLower().EndsWith("m3u8-aapl.ism")).FirstOrDefault();

            // find the mp4 file and create a locator that will be used to access the video
            // the access policy is applied here as well
            streamingAssetFile = assetFiles.Where(f => f.Name.ToLower().EndsWith(".mp4")).FirstOrDefault();
            if (string.IsNullOrEmpty(streamingUrl) && streamingAssetFile != null) {
                var locator = context.Locators.CreateLocator(LocatorType.Sas, streamingAsset, accessPolicy);
                var mp4Uri = new UriBuilder(locator.Path);
                mp4Uri.Path += "/" + streamingAssetFile.Name;
                streamingUrl = mp4Uri.ToString();
            }
            return streamingUrl;
        }


        // GET: /Media/Edit/5
        // Edit the selected video, make public or not public
        // change title
        [Authorize]
        public ActionResult Edit(int id = 0) {
            MediaElement mediaelement = db.MediaElements.Find(id);
            if (mediaelement == null) {
                return HttpNotFound();
            }
            if (string.IsNullOrEmpty(mediaelement.FileUrl)) {
                mediaelement.FileUrl = GetStreamingUrl(mediaelement.AssetId);
                db.Entry(mediaelement).State = EntityState.Modified;
                db.SaveChanges();
            }
            return View(mediaelement);
        }


        // POST: 
        // selected video based on the id
        // Tuple used here to display both the mediaelement and post
        // properties in the one view
        public ActionResult PublicVideoPlayback(int id = 0) {
            MediaElement mediaelement = db.MediaElements.Find(id);
            ViewBag.Posts = db.Posts.Where(p => p.VideoID == id).ToList();
            var view1 = mediaelement;
            var view2 = new Post();
            if (mediaelement == null) {
                return HttpNotFound();
            }
            if (string.IsNullOrEmpty(mediaelement.FileUrl)) {
                mediaelement.FileUrl = GetStreamingUrl(mediaelement.AssetId);

                db.SaveChanges();
            }
            return View(Tuple.Create(view1, view2));
        }

        // Save comments to the db including the 
        // userid and time
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult PublicVideoPlayback(Post post, MediaElement media) {
            if (ModelState.IsValid) {
                post.UserID = User.Identity.Name;
                post.VideoID = media.Id;
                post.VideoTitle = VideoTitle(media.Id);
                DateTime CommentTime = DateTime.Now;
                post.CommentTime = CommentTime.AddHours(1.00).ToString("HH:mm, dd MMM yy");
                db.Posts.Add(post);
                db.SaveChanges();
                return RedirectToAction("PublicVideoPlayback");
            }

            return View();
        }

        public string VideoTitle(int vidID) {
            MediaElement m = db.MediaElements.Find(vidID);
            return m.Title;
        }


        // POST: /Media/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit(MediaElement mediaelement) {
            if (ModelState.IsValid) {
                db.Entry(mediaelement).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(mediaelement);
        }


        // GET: /Media/Delete/5
        // returns delete view
        [Authorize]
        public ActionResult Delete(int id = 0) {
            MediaElement mediaelement = db.MediaElements.Find(id);
            if (mediaelement == null) {
                return HttpNotFound();
            }

            return View(mediaelement);
        }


        // POST: /Media/Delete/5
        // Delete from database, must delete posts first,
        // if not then foreign key violation error is displayed
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id) {
            MediaElement mediaelement = db.MediaElements.Find(id);
            Post post = db.Posts.Find(id);
            DeleteMedia(mediaelement.AssetId);
            if (post == null) {
            }
            else {
                db.Posts.Remove(post);
            }

            db.MediaElements.Remove(mediaelement);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // Delete asset from mediaservices account
        // called in DeleteConfirmed above
        [Authorize]
        private void DeleteMedia(string assetId) {
            string mediaAccountName = ConfigurationManager.AppSettings["MediaAccountName"];
            string mediaAccountKey = ConfigurationManager.AppSettings["MediaAccountKey"];
            CloudMediaContext context = new CloudMediaContext(mediaAccountName, mediaAccountKey);
            var streamingAsset = context.Assets.Where(a => a.Id == assetId).FirstOrDefault();
            if (streamingAsset != null) {
                streamingAsset.Delete();
            }
        }

        [Authorize]
        [HttpGet]
        public ActionResult Upload() {
            return View();
        }

        // sets up the Meta Information about the file 
        // includes the file name, its size, number of blocks it’s being broken into and the reference for it in the Storage Blob
        // It also stores the starting time. 
        // Once done it sends back Json with the value true indicating Metadata was accepted and upload can commence.
        // saves the information in a session so it can be used later when upload starts
        // creates storage container
        [Authorize]
        [HttpPost]
        public ActionResult SetMetadata(int blocksCount, string fileName, long fileSize) {
            var container = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=fitvidsblob;AccountKey=XeSJX2R51EB2UIGdIGs4ckoibI3/9tnSBCaF8QfLouZ2fAtjbJS+pPoApJE9+k0cpCeFXfw2ImdqwOF+kovIUQ==");
            var cb = container.CreateCloudBlobClient().GetContainerReference("temporary-media");
            cb.CreateIfNotExists();
            var fileToUpload = new CloudFile() {
                BlockCount = blocksCount,
                FileName = fileName,
                Size = fileSize,
                BlockBlob = cb.GetBlockBlobReference(fileName),
                StartTime = DateTime.Now,
                IsUploadCompleted = false,
                UploadStatusMessage = string.Empty
            };
            Session.Add("CurrentFile", fileToUpload);
            return Json(true);
        }

        // receives the Chunk from the client along with the Slice of file that needs to be uploaded in the Request.
        // after the chunk is recieved, it is sent to uploadcurrentchunk
        // when uploadcurrentchunks has completed, commitallchunks is called
        [Authorize]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UploadChunk(int id) {
            HttpPostedFileBase request = Request.Files["Slice"];
            byte[] chunk = new byte[request.ContentLength];
            request.InputStream.Read(chunk, 0, Convert.ToInt32(request.ContentLength));
            JsonResult returnData = null;
            string fileSession = "CurrentFile";
            if (Session[fileSession] != null) {
                // set the cloudfile metadata stored in the session to a new cloadfile
                CloudFile model = (CloudFile)Session[fileSession];
                returnData = UploadCurrentChunk(model, chunk, id);
                if (returnData != null) {
                    return returnData;
                }
                if (id == model.BlockCount) {
                    return CommitAllChunks(model);
                }
            }
            else {
                returnData = Json(new {
                    error = true,
                    isLastBlock = false,
                    message = string.Format(CultureInfo.CurrentCulture,
                        "Failed to Upload file.", "Session Timed out")
                });
                return returnData;
            }
            return Json(new { error = false, isLastBlock = false, message = string.Empty });
        }

        // takes all the seperate blocks that have uploaded and puts them in the one blob
        // this is done by creating an enumerable of all the blocks and using putblocklist 
        // to move them to the one blob
        // time taken to upload and the size of the file is returned to the client with a success message
        // the CreateMediaAsset metod is called from here
        [Authorize]
        private ActionResult CommitAllChunks(CloudFile model) {
            model.IsUploadCompleted = true;
            bool errorInOperation = false;
            try {
                // enumerable to hold all blocks
                var blockList = Enumerable.Range(1, (int)model.BlockCount).ToList<int>().ConvertAll(rangeElement =>
                            Convert.ToBase64String(Encoding.UTF8.GetBytes(
                                string.Format(CultureInfo.InvariantCulture, "{0:D4}", rangeElement))));

                // store them in the one blob
                model.BlockBlob.PutBlockList(blockList);
                var duration = DateTime.Now - model.StartTime;
                float fileSizeInKb = model.Size / 1024;
                string fileSizeMessage = fileSizeInKb > 1024 ?
                string.Concat((fileSizeInKb / 1024).ToString(CultureInfo.CurrentCulture), " MB") :
                string.Concat(fileSizeInKb.ToString(CultureInfo.CurrentCulture), " KB");
                model.UploadStatusMessage = "File uploaded successfully";

                CreateMediaAsset(model);
            }
            catch (StorageException e) {
                model.UploadStatusMessage = "Failed to Upload file. Exception - " + e.Message;
                errorInOperation = true;
            }
            finally {
                Session.Remove("CurrentFile");
            }
            return Json(new {
                error = errorInOperation,
                isLastBlock = model.IsUploadCompleted,
                message = model.UploadStatusMessage,
                assetId = model.AssetId
            });
        }

        // session data is used here and uploaded along with the chunk that has been passed in
        // chunked byte stream is uploaded as a Block using the PutBlock method
        // the putblock method commits a new block, all blocks make up the blob
        // a retry policy is used here, if upload fails, it waits 10 seconds and retries
        // theis is done 3 times, if it still fails, the exception is sent back to the screen
        [Authorize]
        private JsonResult UploadCurrentChunk(CloudFile model, byte[] chunk, int id) {
            using (var chunkStream = new MemoryStream(chunk)) {
                var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0:D4}", id)));
                try {
                    model.BlockBlob.PutBlock(
                        blockId,
                        chunkStream, null, null,
                        new BlobRequestOptions() {
                            RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(10), 3)
                        },
                        null);
                    return null;
                }
                catch (StorageException e) {
                    Session.Remove("CurrentFile");
                    model.IsUploadCompleted = true;
                    model.UploadStatusMessage = "Failed to Upload file. Exception - " + e.Message;
                    return Json(new { error = true, isLastBlock = false, message = model.UploadStatusMessage });
                }
            }
        }

        // connect to azure and create an asset. This is necessary for 
        // video delivery and for future encoding 
        [Authorize]
        private void CreateMediaAsset(CloudFile model) {
            // get the account information
            string mediaAccountName = ConfigurationManager.AppSettings["MediaAccountName"];
            string mediaAccountKey = ConfigurationManager.AppSettings["MediaAccountKey"];
            string storageAccountName = ConfigurationManager.AppSettings["StorageAccountName"];
            string storageAccountKey = ConfigurationManager.AppSettings["StorageAccountKey"];

            // create mediaservice context
            CloudMediaContext context = new CloudMediaContext(mediaAccountName, mediaAccountKey);

            // create the storageaccount
            var storageAccount = new CloudStorageAccount(new StorageCredentials(storageAccountName, storageAccountKey), true);
            var cloudBlobClient = storageAccount.CreateCloudBlobClient();

            // get the temporary container which holds the video
            var mediaBlobContainer = cloudBlobClient.GetContainerReference(cloudBlobClient.BaseUri + "temporary-media");

            mediaBlobContainer.CreateIfNotExists();

            // Create a new asset and locator
            IAsset asset = context.Assets.Create("NewAsset_" + Guid.NewGuid(), AssetCreationOptions.None);
            IAccessPolicy writePolicy = context.AccessPolicies.Create("writePolicy",
                TimeSpan.FromMinutes(120), AccessPermissions.Write);
            ILocator destinationLocator = context.Locators.CreateLocator(LocatorType.Sas, asset, writePolicy);

            // Get the asset container URI and copy blobs from mediaContainer to assetContainer.
            Uri uploadUri = new Uri(destinationLocator.Path);
            string assetContainerName = uploadUri.Segments[1];
            CloudBlobContainer assetContainer =
                cloudBlobClient.GetContainerReference(assetContainerName);

            // get blob handle
            string fileName = HttpUtility.UrlDecode(Path.GetFileName(model.BlockBlob.Uri.AbsoluteUri));
            var sourceCloudBlob = mediaBlobContainer.GetBlockBlobReference(fileName);
            sourceCloudBlob.FetchAttributes();

            // start the copy from temporary media to new blob which is controlled by the media service
            if (sourceCloudBlob.Properties.Length > 0) {
                IAssetFile assetFile = asset.AssetFiles.Create(fileName);
                var destinationBlob = assetContainer.GetBlockBlobReference(fileName);
                destinationBlob.DeleteIfExists();
                destinationBlob.StartCopyFromBlob(sourceCloudBlob);
                destinationBlob.FetchAttributes();
                if (sourceCloudBlob.Properties.Length != destinationBlob.Properties.Length)
                    model.UploadStatusMessage += "Failed to copy as Media Asset!";
            }
            destinationLocator.Delete();
            writePolicy.Delete();

            // Refresh the asset.
            asset = context.Assets.Where(a => a.Id == asset.Id).FirstOrDefault();

            var ismAssetFiles = asset.AssetFiles.ToList().
            Where(f => f.Name.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
            .ToArray();

            if (ismAssetFiles.Count() != 1)
                throw new ArgumentException("The asset should have only one, .ism file");

            ismAssetFiles.First().IsPrimary = true;
            ismAssetFiles.First().Update();

            //model.UploadStatusMessage += " Created Media Asset '" + asset.Name + "' successfully.";
            model.AssetId = asset.Id;
        }

        protected override void Dispose(bool disposing) {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}