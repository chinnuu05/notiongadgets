/* Sets CSS variables to the ones specificed in Lively Wallpaper Customize options */

var ApiKey, BackgroundSource, ThemeColor, OpacityLevel;

function livelyPropertyListener(name, val) {

    switch (name) {
        case "apiKey":
            ApiKey = val;
            break;
        case "imgSelect":
            console.log(val);
            BackgroundSource = val;
            SetBackgroundSource();
            break; 
        case "themeColor":
            ThemeColor = val;
            break;
        case "transparencyLevel":
            OpacityLevel = val;
            SetOpacity();
            break;
    }
}

function SendAPIKeyToServer() {
    // send the API key to localhost:1234
}

function SetThemeColor() {
    // change color of buttons, settings, and other shit

}

function SetBackgroundSource() {


    console.log("In SetBackgroundSource()");
    BackgroundSource = BackgroundSource.replace("\\", "/");
    var extension = BackgroundSource.split('.')[1];

    console.log(extension);


    // check if image or video
    if (extension == "jpg" || extension == "png" || extension == "jpeg" || extension == "webp" ) {
        
        console.log("It's an image");
        // remove current video 

        document.getElementById("background-video-container").style.display = "none";


        console.log(document.documentElement.style.getPropertyValue("--background-image"));
        
        var path = "url(\"..\\" + BackgroundSource + "\")";
        path = path.replace("\\", "/");
        console.log("set image source to: " + path);
        document.documentElement.style.setProperty('--background-image', path);

        $('body').addClass('background-image');
    }

    else if (extension == "mp4" || extension == "webm" || extension == "wmv" || extension == "mov" ) 
    {
        // remove current image if there is one
        if ('background-image' in document.getElementById("bootstrap-overrides").classList) {
            console.log("current background is an image, removign");

            document.getElementById("bootstrap-overrides").classList.remove('background-image');
        }


        console.log("it's a video");
        $('#background-video-container').css("display", "block");
        $('#background-video').attr('src', BackgroundSource);
        var video = document.getElementById("video-elem");
        var source = document.getElementById("background-video");

        video.play();
        console.log({
          src: source.getAttribute('src'),
          type: source.getAttribute('type'),
        });
        
        setTimeout(function() {
          video.pause();
        
          source.setAttribute('src', BackgroundSource);
          source.setAttribute('type', 'video/' + extension);
        
          video.load();
          video.play();
          console.log({
            src: source.getAttribute('src'),
            type: source.getAttribute('type'),
          });
        }, 3000);
    } 


    // if image add wallpaper-imag class to body

    // if video
    // document.getElementById("#background-video-container").style.display = "block";
    // document.getElementById("#background-video").source = BackgroundSource;


}

function SetOpacity() {
    document.documentElement.style.setProperty('--opacity', OpacityLevel/100);

}


$(document).ready(function() {
    // console.log(window.getComputedStyle(document.documentElement).getPropertyValue('--background-image'));
    // console.log(document.documentElement.style.getPropertyValue("--opacity"));

})