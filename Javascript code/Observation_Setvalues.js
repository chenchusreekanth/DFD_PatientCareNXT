// JavaScript source code
function Observation_DrugEfficiency_Onchange(executionContext) {
    'use strict';
    var formContext = executionContext.getFormContext();
    var formType = formContext.ui.getFormType();
    //fetch and assign all values to variables
    if (formContext.getAttribute("msemr_valuesampleddatatypeupperlimit") !== null && formContext.getAttribute("msemr_valuesampleddatatypeupperlimit").getValue() !== null) {
        var highLimit = formContext.getAttribute("msemr_valuesampleddatatypeupperlimit").getValue();
    }
    if (formContext.getAttribute("msemr_valuesampleddatatypelowerlimit") !== null && formContext.getAttribute("msemr_valuesampleddatatypelowerlimit").getValue() !== null) {
        var lowLimit = formContext.getAttribute("msemr_valuesampleddatatypelowerlimit").getValue();
    }
    if (formContext.getAttribute("msemr_valuerangelowlimit") !== null && formContext.getAttribute("msemr_valuerangelowlimit").getValue() !== null) {
        var ReflowLimit = formContext.getAttribute("msemr_valuerangelowlimit").getValue();
    }
    if (formContext.getAttribute("msemr_valuerangehighlimit") !== null && formContext.getAttribute("msemr_valuerangehighlimit").getValue() !== null) {
        var RefhighLimit = formContext.getAttribute("msemr_valuerangehighlimit").getValue();
    }
    //calculate the percentage value based on fetched values using formula
    var percentage = (highLimit / lowLimit) * (ReflowLimit / RefhighLimit) * 100;   
    if (percentage !== null) {
         //percentage <100 then value update directly in the field (hcp_drugefficiency)
        if (percentage < 100) {
            formContext.getAttribute("hcp_drugefficiency").setValue(percentage);
        }
         //percentage >100 then value again reduced from the static value(200) update in the field (hcp_drugefficiency)
        else {
            percentage = 200 - percentage;
            formContext.getAttribute("hcp_drugefficiency").setValue(percentage);
        }
    }
}

function Observation_updateCareplanReference_Onchange(executionContext) {
    'use strict';
    var formContext = executionContext.getFormContext();
    if (formContext.getAttribute("hcp_careplan").getValue() !== null) {
        //lookup record GUID
        var carePlanid = formContext.getAttribute("hcp_careplan").getValue()[0].id;
        //condition to fetch respective record using WebApi
        Xrm.WebApi.online.retrieveRecord("msemr_careplan", carePlanid, "?$select=hcp_referencerangehighvalue,hcp_referencerangelowvalue").then(
            function success(result) {
                var hcp_referencerangehighvalue = result["hcp_referencerangehighvalue"];
                var hcp_referencerangehighvalue_formatted = result["hcp_referencerangehighvalue@OData.Community.Display.V1.FormattedValue"];
                var hcp_referencerangelowvalue = result["hcp_referencerangelowvalue"];
                var hcp_referencerangelowvalue_formatted = result["hcp_referencerangelowvalue@OData.Community.Display.V1.FormattedValue"];
                //value from careplan assign to respective observation record
                if (hcp_referencerangehighvalue !== null) {
                   formContext.getAttribute("msemr_valuerangehighlimit").setValue(hcp_referencerangehighvalue);
                }
                if (hcp_referencerangelowvalue !== null) {
                    formContext.getAttribute("msemr_valuerangelowlimit").setValue(hcp_referencerangelowvalue);
                }

            },
            function (error) {
                Xrm.Navigation.openAlertDialog(error.message);
            }
        );
    }
}
