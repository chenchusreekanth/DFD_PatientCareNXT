using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk.Query;

namespace Persistent.PatientCareNXT.Dev
{
    public class ContactPreUpdate_SetTabletsActivity : IPlugin
    {
        string ActivityName;
        int? ExPercentage;
        int? MePercentage;
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
                    Entity Contact = (Entity)context.InputParameters["Target"];
                    // Verify that the target entity represents an Contact.
                    // If not, this plug-in was not registered correctly.
                    if (Contact.LogicalName != "contact")
                        return;  
                    tracingService.Trace("Contact Logical Namer =" + Contact.LogicalName);
                    Guid contactId = context.PrimaryEntityId;
                    tracingService.Trace("Contact Guid =" + contactId);
                    // Retrieving Contact
                    Entity contact = service.Retrieve("contact", contactId, new ColumnSet(true));
                    if (contact.Attributes.Contains("hcp_exercisedetails"))
                    {
                        ExPercentage = contact.GetAttributeValue<Int32>("hcp_exercisedetails");
                        tracingService.Trace("Exercise Percentage =" + ExPercentage);
                    }
                    if (contact.Attributes.Contains("hcp_progressofmedication"))
                    {
                        MePercentage = contact.GetAttributeValue<Int32>("hcp_progressofmedication");
                        tracingService.Trace("Progress of medication =" + MePercentage);
                    }
                    if (contactId != null)
                    {
                        EntityCollection carePlanActivities = getAllRelatedRecords(contactId, service);
                        {
                            tracingService.Trace("CarePlan Activities Retrieved");
                            foreach (Entity carePlanActivity in carePlanActivities.Entities)
                            {
                                if (carePlanActivity.Attributes.Contains("msemr_description"))
                                {
                                    ActivityName = carePlanActivity.GetAttributeValue<string>("msemr_description");

                                }

                            }

                        }
                    }
                    if (context.Depth == 1)
                    {
                        if (ActivityName != null)
                        {
                            Contact["hcp_medicationdetails"] = ActivityName;
                            
                        }
                        if (ExPercentage != null)
                        {
                            Contact["hcp_exercisedetailspercentage"] = ExPercentage + "%";

                        }
                        if (MePercentage != null)
                        {
                            Contact["hcp_medicationprogress"] = MePercentage + "%";
                        }
                        tracingService.Trace("Contact Updated Successfully");

                    }

                }

            }
            catch (InvalidPluginExecutionException ex) { throw new InvalidPluginExecutionException(ex.Message); }
            catch (Exception ex) { throw new InvalidPluginExecutionException(ex.Message); }
        }

        public EntityCollection getAllRelatedRecords(Guid ContactId, IOrganizationService service)
        {
            
            QueryExpression CareplanactivityQuery = new QueryExpression("msemr_careplanactivity");
            CareplanactivityQuery.ColumnSet = new ColumnSet("msemr_careplanactivityid", "msemr_description", "createdon", "msemr_activitycategory", "msemr_activitydefinitiontypeactivitydefinition", "msemr_activitystartdate", "msemr_activityenddate", "msemr_activitylocation", "msemr_activitydefinitiontypeplandefinition");
            CareplanactivityQuery.Criteria.AddCondition("msemr_patient", ConditionOperator.Equal, ContactId);
            CareplanactivityQuery.Criteria.AddCondition("msemr_description", ConditionOperator.Like, "%Tablets%");
            EntityCollection carePlanActivities = service.RetrieveMultiple(CareplanactivityQuery);
            return carePlanActivities;
        }

    }
}
