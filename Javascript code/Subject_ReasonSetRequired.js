// JavaScript source code
function SetfieldRequired(ex) {
    var formcontext = ex.getFormContext();
    var Value = formcontext.getAttribute("statuscode").getValue();
    alert(Value)
    if (Value == 800850001) {
        formcontext.getAttribute("hcp_reasonfordroppingout").setRequiredLevel("required");
    }
}