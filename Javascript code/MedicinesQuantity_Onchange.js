function MedicationRequest_MedicinesQuantity_Onchange(executionContext) {
    'use strict';
    var formContext = executionContext.getFormContext();

    if (formContext.getAttribute("hcp_medicationordersonetomanyid").getValue() !== null) {       
        var tabletLookup = formContext.getAttribute("hcp_medicationordersonetomanyid").getValue()[0].id;
        tabletLookup = tabletLookup.substring(1, 37);
        //retrieving the records based on condition
        Xrm.WebApi.online.retrieveRecord("msemr_medicationrequest", tabletLookup, "?$select=hcp_priceperitem,msemr_name").then(
            function success(result) {
                var hcp_priceperitem = result["hcp_priceperitem"];
                var hcp_priceperitem_formatted = result["hcp_priceperitem@OData.Community.Display.V1.FormattedValue"];
                var msemr_name = result["msemr_name"];
            },
            function (error) {
                Xrm.Utility.alertDialog(error.message);
            }
        );
        if (formContext.getAttribute("hcp_quantity") !== null && formContext.getAttribute("hcp_quantity").getValue() !== null) {
            var Quantity = formContext.getAttribute("hcp_quantity").getValue();
        }
        //updating the hcp_price value by priceperitem*quantity.
        if (hcp_priceperitem !== null && Quantity !== null) {
            var price = hcp_priceperitem_formatted * Quantity;
            formContext.getAttribute("hcp_price").setValue(price);

        }


    }

}