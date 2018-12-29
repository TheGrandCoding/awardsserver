// currently does nothing
function terminateAll() {
    var hiddenDocs = document.getElementsByClassName("hidden");
    for (var i = 0; i < hiddenDocs.length; ++i) {
        var item = hiddenDocs[i];
        item.innerHTML = '[Removed]';
    }
}
terminateAll();
