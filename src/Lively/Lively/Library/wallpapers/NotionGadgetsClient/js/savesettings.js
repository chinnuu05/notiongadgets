/* update local storage every time gadgets are resized or repositioned */
$( ".gadget").resizable({
    resize: function(e, ui) {
        var instance = $(this).resizable('instance');
        var size = [ ui.size.width, ui.size.height ];
        // console.log(size);
        localStorage.setItem(e.target.id + "_size", JSON.stringify(size));
        // JSON.parse(localStorage.getItem(e.target.id + "_size"));
    }
});

$( ".gadget" ).draggable({
    drag: function(e, ui) {
        var instance = $(this).draggable('instance');
        var pos = [ ui.position.top, ui.position.left ];
        localStorage.setItem(e.target.id + "_pos", JSON.stringify(pos));

        // JSON.parse(localStorage.getItem(e.target.id + "_size"));
    }});







