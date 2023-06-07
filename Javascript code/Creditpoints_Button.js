function createButton(id, defaultText, intWidth, onClickEventFunctionName) {
    //debugger;
    var btn = document.createElement("Credit Points");


    //Create a  element, you can also use input, but need to set type to button
    var t = document.createTextNode(defaultText);
    btn.appendChild(t);
    btn.className = "ms-crm-Button";
    btn.id = id;

    if (intWidth != null) {
        btn.style.width = intWidth + "px";
    }
    else {
        //defaulted width
        btn.style.width = "100%";
    }

    btn.onmouseover = onCRMBtnMouseHover;
    btn.onmouseout = onCRMBtnMouseOut;

    btn.onclick = onClickEventFunctionName;

    return btn;
}