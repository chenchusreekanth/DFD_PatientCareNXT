// JavaScript source code
function freeAppointment_ScheduleAppointment_onload(executionContext) {
    'use strict';
    debugger;
    var formContext = executionContext.getFormContext();
    var ContactId = formContext.data.entity.getId();//"GUID"
    var GUID = ContactId.substring(1, 37);
    //checking whether the logged in user has Silver badge on hcp_creditpointbadge
    Xrm.WebApi.online.retrieveMultipleRecords("contact", "?$select=contactid,fullname,hcp_creditpointbadge&$filter=contactid eq (" + GUID + ") and  hcp_creditpointbadge eq 'https%3A%2F%2Fpatientcare.powerappsportals.com%2Fhome-page%2Fsilver150.png'").then(
        function success(results) {
            for (var i = 0; i < results.entities.length; i++) {
                var contactid = results.entities[i]["contactid"];
                var fullname = results.entities[i]["fullname"];
                var hcp_creditpointbadge = results.entities[i]["hcp_creditpointbadge"];
                //alert box 
                alert("You have one free Appointment on rewarding Silver badge"); 
                /*Xrm.Utility.confirmDialog("You have one free Appointment on rewarding Silver badge",
                    function () 
                    {
                    },
                        function () {
                            Xrm.Utility.openEntityForm("contact");
                        });*/
            }
            /*var confirmStrings = { confirmButtonLabel: "Proceed", text: "You have one free Appointment on rewarding Silver badge,Click Proceed to book", title: "Rewards" };
            var confirmOptions = { height: 120, width: 260 };
            Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then
            {
                if (success.confirmed)
                    console.log("Proceed");
                else
                    console.log("Not OK");
            }
            return;*/
        },
        function (error) {
            Xrm.Utility.alertDialog(error.message);
        }
    );
}
    