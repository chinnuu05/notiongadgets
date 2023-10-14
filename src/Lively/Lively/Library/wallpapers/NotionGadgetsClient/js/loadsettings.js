// stored as gadget_pos and gadget_size in local storage
// position: top, left
// size: width, height
function LoadGadgets() {

    console.log("loading gadgets");
    var gadgets = ["WaterGadget", "TaskGadget", "DatabaseGadget"];

    gadgets.forEach(function(gadget) 
    {
        var unparsedPos = localStorage.getItem(gadget + "_pos");
        if (unparsedPos != null) {
            var pos = JSON.parse(unparsedPos);

            $('#' + gadget).css("top", pos[0] + "px");
            $('#' + gadget).css("left", pos[1] + "px");
        } else {
            console.log(gadget + " didn't have position saved. Nothing to load.")
        }

        var unparsedSize = localStorage.getItem(gadget + "_size");

        if (unparsedSize != null) {
            var size = JSON.parse(unparsedSize);

            $('#' + gadget).css("width", size[0] + "px");
            $('#' + gadget).css("height", size[1] + "px");   

            console.log('Set size for ' + gadget);
    
        } else {
            console.log(gadget + " didn't have size saved. Nothing to load.")
        }
    })
}
