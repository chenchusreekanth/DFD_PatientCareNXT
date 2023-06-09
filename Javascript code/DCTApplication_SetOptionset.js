// JavaScript source code
function ChangeStatusofDCTApplication() {
    var Value = Xrm.Page.getAttribute("statuscode").getValue();
    if (Value != null) {
        Xrm.Page.getAttribute("statuscode").setValue(800850001);
    }
}