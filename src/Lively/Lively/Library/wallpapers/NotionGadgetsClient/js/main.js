var tasks = [];
var inputs = [];
var inputStates = [];

$(document).ready(function() { 




    LoadGadgets();
    $.getJSON("http://localhost:1234/tasks", function(data) {
        LoadTaskWidget(data);


    });




});

function RefreshTasks() {
    $.getJSON("http://localhost:1234/tasks", function(data) {
        LoadTaskWidget(data);


    });
}


$('#addToDatabaseBtn').click(function() { 
    $(this).prop("disabled", true);
    // add spinner to button
    $(this).html(
      `<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`
    );

    var inputText = $('#databaseText').val();
    // console.log("sending: " + inputText);
    

    $.post("http://localhost:1234/addtodb/", inputText, function() {
        
        console.log("got response from add db");
        $('#addToDatabaseBtn').html('Add');
        $('#addToDatabaseBtn').prop('disabled', false);
    }, "json");
})

// function UpdateTasks(blockID, ) {
//     console.log("updating tasks");
//     console.log(tasks);

//     $(tasks).each(function(index) {
//         // retrieve checked status
//         // retrieve span
//         $('#')
        
        
//     });
// }


function LoadTaskWidget(data) {
    // $('#four').prop("checked", true);

    $('.task-container').empty();

    /* Add tasks in this format
    <input type="checkbox"> 
    <label class="task">
    <span id="checkOne" class="checkbox"></span>
    <span class="task-text" id="strikeOne">Check Me</span>
    */
    var count = 0;
    $.each(data, function(key, val) {


        var checkboxName = count + 'check';
        
        var input = document.createElement('input');
        input.type = "checkbox";
        input.id = count.toString();
        input.checked = val.IsChecked;

        var label = document.createElement('label');
        label.htmlFor = input.id;
        label.id = count + "task";
        label.classList.add("task");

        var boxSpan = document.createElement('span');
        boxSpan.id = count + "check";
        boxSpan.setAttribute('notion-id', val.ID);

        boxSpan.classList.add("checkbox");

        var textSpan = document.createElement('span');
        textSpan.id = count + "strike";
        textSpan.textContent = " " + val.PlainText;
        textSpan.classList.add("task-text");

        tasks.push(count + "task");
        inputs.push(count);

        inputStates.push(val.IsChecked);

        $('.task-container').append(input, label);
        $("#" + count + "task").append(boxSpan, textSpan);  

        $("#" + count + "task").click(function(e) {
            setTimeout(function() {

                // $.each(inputs, function(index) {
                //     // console.log("disabled all inputs");
                //     $('#' + index).prop('disabled', true);
                // })

                var isChecked;
                var c = document.getElementById(checkboxName);
                
                var notionId = c.getAttribute("notion-id");
    
                var pseudo = window.getComputedStyle(c, ':before');
                var value = pseudo.getPropertyValue("content");
                
                // console.log(value);
                if (value == "none") {
    
                    isChecked = false;
                } else {
                    isChecked = true;
                }
    
    
                var req = {
                    blockID: notionId,
                };
    
                // check if pseuedo element matches input state
                var index = parseInt(e.target.id);
    
    
    
                if (inputStates[index] != isChecked) {
                    // console.log("valid click");
                } else {
                    // console.log("invalid");
                }
    
                // console.log(isChecked);
                $.get("http://localhost:1234/updatetasks/" + notionId + "|" + isChecked, function(response) {
                    
                
                    console.log(response);

                    if (response != "No-Error") {
                        // console.log("creating alert");
                        // var div = document.createElement("div");
                        // div.classList.add("alert");
                        // div.classList.add("alert-danger");
                        // div.classList.add("fixed-alert");
                        // div.classList.add("alert-dismissable");
                        // div.classList.add("fade");
                        // div.classList.add("show");

                        // var close = document.createElement("button");
                        // close.classList.add("close");
                        // close.setAttribute("data-bs-dismiss", "close");
                        // close.setAttribute("aria-label", "close");

                        // div.append(close);

                        // var text = document.createElement("span");
                        // text.innerText = "&times;";
                        // close.append(text);

                        // div.append(document.createTextNode("Slow down, the Notion API can't keep up!"))

                        // div.style.zIndex = 99;
                        // $('#bootstrap-overrides').append(div);

                    }
                    // $.each(inputs, function(index) {
                    //     // console.log("enabling all inputs");
                    //     $('#' + index).prop('disabled', false);
                    // })
                });
    
            }, 500);
        });

        count++;    

        
    });
}