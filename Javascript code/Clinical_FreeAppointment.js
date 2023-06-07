// JavaScript source code
function freeAppointment_clinicalappointmentEMRform_onload(executionContext) {
    'use strict';
    debugger;
    var formContext = executionContext.getFormContext();
    var ContactId = formContext.getAttribute("msemr_actorpatient").getValue()[0].id;
    //removing strings to fetch only GUID
    var GUID = ContactId.substring(1, 37);
    //checking whether the logged in user has Silver badge on hcp_creditpointbadge
    Xrm.WebApi.online.retrieveMultipleRecords("contact", "?$select=contactid,fullname,hcp_creditpointbadge&$filter=contactid eq (" + GUID + ") and  hcp_creditpointbadge eq 'https%3A%2F%2Fpatientcare.powerappsportals.com%2Fhome-page%2Fsilver150.png'").then(
        function success(results) {
            for (var i = 0; i < results.entities.length; i++) {
                var contactid = results.entities[i]["contactid"];
                var fullname = results.entities[i]["fullname"];
                var hcp_creditpointbadge = results.entities[i]["hcp_creditpointbadge"];
                //alert onload of the clinical timeline appointment EMR
                alert(fullname + " have free appointment");
                //inline notification on the form 
                Xrm.Page.ui.setFormNotification("You can avail free appointment.", "INFO", "101");
            }
        },
        function (error) {
            Xrm.Utility.alertDialog(error.message);
        }
    );
}
