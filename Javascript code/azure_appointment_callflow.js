// JavaScript source code
function Callflow_contact_appointmentwizardform_onload(executionContext) {  
'use strict'; 
    var formContext = executionContext.getFormContext();
    //URL from power automate flow after saving which trigger onload.
    var flowUrl = "https://prod-131.westus.logic.azure.com:443/workflows/bc29e78c33454cfab5eb038dfe7c130b/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=F6LbWBT9FfkT0GAAkibB_Plm9GVrqlFAqPyGr9NNZ4Y";
    var input = JSON.stringify();
    var req = new XMLHttpRequest();
    //using post method to retrieve data
    req.open("POST", flowUrl, true);
    req.setRequestHeader('Content-Type', 'application/json');
    req.send(input);
}
