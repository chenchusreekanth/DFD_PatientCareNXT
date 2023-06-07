// JavaScript source code
function Contact_setExerciseValueWithPercentage_Onload(executionContext) {
    'use strict';
    var formContext = executionContext.getFormContext();
    //fetching value of the field
    var progress = formContext.getAttribute("hcp_exercisedetails").getValue();
    //adding special character to the string
    var progressValue = progress + "%";
    if (formContext.getAttribute("hcp_exercisedetails").getValue !== null)
        try {
            //updating the fetched value with percentage
            formContext.getAttribute("hcp_exercisedetailspercentage").setValue(progressValue);
        }
        catch (e) {

        }
}
function Contact_setMedicationValueWithPercentage_Onload(executionContext) {
    'use strict';
    var formContext = executionContext.getFormContext();
    //fetching value of the field
    var medication = formContext.getAttribute("hcp_progressofmedication").getValue();
    //adding special character to the string
    var medicationValue = medication + "%";
    if (formContext.getAttribute("hcp_progressofmedication").getValue !== null)
        try {
            //updating the fetched value with percentage
            formContext.getAttribute("hcp_medicationprogress").setValue(medicationValue);
        }
        catch (e) {

        }
} 
