/// <reference path="jquery-1.8.2.js" />
/// <reference path="_references.js" />


var maxRetries = 3;
// 1 mb
var blockLength = 1048576;
var numberOfBlocks = 1;
var currentChunk = 1;
var retryAfterSeconds = 3;

$(document).ready(function () {
    $(document).on("click", "#fileUpload", beginUpload);
    $(document).on("click", "#saveDetails", saveDetails);
    $("#detailsPanel").hide();
    $("#progressBar").progressbar(0);

});

$(document).ready(function () {
    $("#searchValue").autocomplete({
        source: Url.Action("Media/GetSearchList")
    });
});

// gets the selected file to be uploaded, when it is found
// it calls the uploadmedadata function which calculates
// how many chunks will be uploaded based on a 1mb chunk size
var beginUpload = function () {
    var fileControl = document.getElementById("selectFile");
    if (fileControl.files.length > 0) {
        for (var i = 0; i < fileControl.files.length; i++) {
            uploadMetaData(fileControl.files[i], i);
        }
    }
}

// Called when "Save" button is pressed on UI
// waits for JsonResult from mediacontroller
// if the save was successful, the title is added and "Saved Successfully"
// is displayed along with the video. if not then "Saved failed" is displayed
var saveDetails = function () {
    var dataPost = {
        "Title": $("#title").val(),
        "AssetId": $("#assetId").val()
    }
    $.ajax({
        type: "POST",
        async: false,
        contentType: "application/json",
        data: JSON.stringify(dataPost),
        url: "/Media/Save"
    }).done(function (state) {
        if (state.Saved == true) {
            displayStatusMessage("Saved Successfully");
            $("#detailsPanel").hide();
            mediaPlayer.initFunction("videoDisplayPane", state.StreamingUrl);
        }
        else {
            displayStatusMessage("Saved Failed");
        }
    });
}

// the number of chunks is calculated here and 
// all of the metadata is set here too by sending it in a URL
// if the metadat cannot be sent, an error message is displayed
// the file and the block length is then sent to sendFile
var uploadMetaData = function (file, index) {
    var size = file.size;
    numberOfBlocks = Math.ceil(file.size / blockLength);
    var name = file.name;
    currentChunk = 1;

    $.ajax({
        type: "POST",
        async: false,
        url: "/Media/SetMetadata?blocksCount=" + numberOfBlocks + "&fileName=" + name + "&fileSize=" + size,
    }).done(function (state) {
        console.log(state);
        if (state === true) {
            $("#fileUpload").hide();
            displayStatusMessage("Starting Upload");
            sendFile(file, blockLength);
        }
    }).fail(function () {
        $("#fileUpload").show()
        displayStatusMessage("Failed to send MetaData");
    });

}

// takes in the file and the size each chunk should be
// then slices the file into chunks of the correct size
// and uploads it.
var sendFile = function (file, chunkSize) {
    var start = 0,
        end = Math.min(chunkSize, file.size),
        retryCount = 0,
        sendNextChunk,
        fileChunk;
    displayStatusMessage("");

    // called recursivly while there are still chunks to upload
    sendNextChunk = function () {
        fileChunk = new FormData();

        if (file.slice) {
            fileChunk.append('Slice', file.slice(start, end));
        }
        else if (file.webkitSlice) {
            fileChunk.append('Slice', file.webkitSlice(start, end));
        }
        else if (file.mozSlice) {
            fileChunk.append('Slice', file.mozSlice(start, end));
        }
        else {
            displayStatusMessage(operationType.UNSUPPORTED_BROWSER);
            return;
        }
        jqxhr = $.ajax({
            async: true,
            url: ('/Media/UploadChunk?id=' + currentChunk),
            data: fileChunk,
            cache: false,
            contentType: false,
            processData: false,
            type: 'POST'
        }).fail(function (request, error) {
            if (error !== 'abort' && retryCount < maxRetries) {
                ++retryCount;
                setTimeout(sendNextChunk, retryAfterSeconds * 1000);
            }

            if (error === 'abort') {
                displayStatusMessage("Aborted");
            }
            else {
                if (retryCount === maxRetries) {
                    displayStatusMessage("Upload timed out.");
                    resetControls();
                    uploader = null;
                }
                else {
                    displayStatusMessage("Resuming Upload.");
                }
            }

            return;
        }).done(function (notice) {
            if (notice.error || notice.isLastBlock) {

                displayStatusMessage(notice.message);
                if (notice.isLastBlock) {
                    $("#assetId").val(notice.assetId);
                    $("#detailsPanel").show();
                }
                return;
            }
            ++currentChunk;
            start = (currentChunk - 1) * blockLength;
            end = Math.min(currentChunk * blockLength, file.size);
            retryCount = 0;
            updateProgress();
            if (currentChunk <= numberOfBlocks) {
                sendNextChunk();
            }
        });
    }
    sendNextChunk();
}

var displayStatusMessage = function (message) {
    $("#statusMessage").text(message);
}

var updateProgress = function () {
    var progress = currentChunk / numberOfBlocks * 100;
    if (progress <= 100) {

        $("#progressBar").progressbar({
            value: parseInt(progress)
        });
        $("#progressBar").css({ 'background': '#dff4e0' });
        $("#progressBar > div").css({ 'background': '#11db3d' });

        displayStatusMessage("Uploaded " + parseInt(progress) + "%");
    }

}


