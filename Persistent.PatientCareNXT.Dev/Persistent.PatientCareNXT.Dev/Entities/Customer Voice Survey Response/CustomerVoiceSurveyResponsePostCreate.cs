using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace Persistent.PatientCareNXT.Dev
{
    public class CustomerVoiceSurveyResponsePostCreate : IPlugin
    {
        Guid UserId;
        public void Execute(IServiceProvider serviceProvider)
        {
            //here it will goes to write the business logic

            //Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)
            serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Obtain the organization service reference.
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);


            try
            {

                if (context.InputParameters.Contains("Target") &&
                               context.InputParameters["Target"] is Entity)
                {
                    // Obtain the target entity from the input parameters.
                    Entity CustomerVoiceSurveyResponse = (Entity)context.InputParameters["Target"];
                    // Verify that the target entity represents an Customer voice survey Response.
                    // If not, this plug-in was not registered correctly.
                    if (CustomerVoiceSurveyResponse.LogicalName != "msfp_surveyresponse")
                        return;
                    tracingService.Trace("Entity Logical Name =" + CustomerVoiceSurveyResponse.LogicalName);

                    // getting from field from Customer Voice Survey Response record
                    EntityCollection from = CustomerVoiceSurveyResponse.GetAttributeValue<EntityCollection>("from");
                    tracingService.Trace("Users count in from field =" + from.Entities.Count);

                    if (from.Entities.Count > 0)
                    {
                        foreach (Entity user in from.Entities)
                        {
                            EntityReference partyId = user.GetAttributeValue<EntityReference>("partyid");
                            UserId = partyId.Id;
                        }
                    }
                    var multiLineStringResult = new StringBuilder();
                    //Response fetching from Customer Voice response entity as YES, NO, MAYBE.
                    var response = multiLineStringResult.AppendLine(CustomerVoiceSurveyResponse.GetAttributeValue<string>("msfp_questionresponseslist")).ToString();
                    string[] lst = response.Split(',');
                    string res = lst[1].Split(':')[1].Replace("\"", "").Replace("}", "").Replace("]", "");
                    tracingService.Trace("Response =" + res);
                    // Updating the Contact with response
                    tracingService.Trace("------------ Updating Contact -----------------------");
                    Entity contactUpdate = new Entity("contact");
                    contactUpdate.Attributes["contactid"] = UserId;
                    contactUpdate.Attributes["hcp_wouldyouliketosetupaphysicianappointment"] = res;
                    service.Update(contactUpdate);
                    tracingService.Trace("Contact Updated");
 
                }

            }
            
            catch (InvalidPluginExecutionException ex) { throw new InvalidPluginExecutionException(ex.Message); }
            catch (Exception ex) { throw new InvalidPluginExecutionException(ex.Message); }
        }
    }
}
