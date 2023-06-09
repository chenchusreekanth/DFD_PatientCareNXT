// JavaScript source code
function RemoveField(ex) {
    var formcontext = ex.getFormContext();
    var Statuscode = formcontext.getAttribute("statuscode").getValue();
    if (Statuscode == "800850001") {
        formcontext.getAttribute("hcp_sitelookupid").setValue(null);
    }
}
