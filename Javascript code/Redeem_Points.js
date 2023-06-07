function getCurrentbalance_rewardpointsform_redeem_onchange(executionContext) {
    'use strict';
    var formContext = executionContext.getFormContext();
    try {
        var redeem = 0;
        var earnedpoints = 0;
        var redeemedpoints = 0;
        var Currentbalance = 0;
        //get the values of fields (hcp_redeempoints) 
        if (formContext.getAttribute("hcp_redeem") !== null && formContext.getAttribute("hcp_redeem").getValue() !== null) {
            redeem = formContext.getAttribute("hcp_redeem").getValue(); 
        }
        if (formContext.getAttribute("hcp_earnedpoints") !== null && formContext.getAttribute("hcp_earnedpoints").getValue() !== null) {
            earnedpoints = formContext.getAttribute("hcp_earnedpoints").getValue(); 
        }
        if (formContext.getAttribute("hcp_redeemedpoints") !== null && formContext.getAttribute("hcp_redeemedpoints").getValue() !== null) {
            redeemedpoints = formContext.getAttribute("hcp_redeemedpoints").getValue(); 
        }
        if (formContext.getAttribute("hcp_balance") !== null && formContext.getAttribute("hcp_balance").getValue() !== null) {
            Currentbalance = formContext.getAttribute("hcp_balance").getValue(); 
        }
        //condition
        if (redeem <= Currentbalance && redeem >= 0) {
            redeemedpoints = redeemedpoints + redeem;
            formContext.getAttribute("hcp_redeemedpoints").setValue(redeemedpoints);
            var balance = earnedpoints - redeemedpoints;
            if (formContext.getAttribute("hcp_balance") !== null) {
                formContext.getAttribute("hcp_balance").setValue(balance); 
            }
            formContext.getAttribute("hcp_redeem").setValue(null);
        }
        else {
            alert("Please enter valid points");
            formContext.getControl("hcp_redeem").setFocus(); // set focus
            executionContext.getEventArgs().preventDefault(); // this syntax is controlling Save Operation

        }

    }
    catch (e) {
        Xrm.Utility.alertDialog(e.message);
    }
}
