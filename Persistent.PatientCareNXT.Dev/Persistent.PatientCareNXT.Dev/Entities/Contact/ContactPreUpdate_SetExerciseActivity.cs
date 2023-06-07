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
    public class ContactPreUpdate_SetExerciseActivity : IPlugin
    {
        string ActivityName;

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
                    if (Contact.LogicalName != "Contact")
                        return;
                    tracingService.Trace("Contact Logical Namer =" + Contact.LogicalName);
                    Guid contactId = context.PrimaryEntityId;
                    tracingService.Trace("Contact Guid =" + contactId);

                    if (contactId != null)
                    {
                        // Getting Care Plan Activities
                        EntityCollection carePlanActivities = getAllRelatedRecords(contactId, service);
                        {
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
                            // Updating Contact
                            tracingService.Trace("Activity Name =" + ActivityName);
                            Entity contactUpdate = new Entity("contact");//task
                            contactUpdate.Attributes["contactid"] = contactId;
                            contactUpdate.Attributes["hcp_careplanactivity"] = ActivityName;
                            service.Update(contactUpdate);
                            tracingService.Trace("Contact Updated Successfully");

                        }

                    }

                }

            }
            catch (InvalidPluginExecutionException ex) { throw new InvalidPluginExecutionException(ex.Message); }
            catch (Exception ex) { throw new InvalidPluginExecutionException(ex.Message); }
        }

        public EntityCollection getAllRelatedRecords(Guid ContactId, IOrganizationService service)
        {
            var query_msemr_patient = ContactId;
            var query_msemr_description = "Exercise";
            var aw_msemr_title = "Hyper";

            // Instantiate QueryExpression query
            var query = new QueryExpression("msemr_careplanactivity");
            // Add columns to query.ColumnSet
            query.ColumnSet.AddColumns("msemr_careplanactivityid", "msemr_description", "createdon", "msemr_activitycategory", "msemr_activitydefinitiontypeactivitydefinition", "msemr_activitystartdate", "msemr_activityenddate", "msemr_activitylocation", "msemr_activitydefinitiontypeplandefinition");

            // Add filter query.Criteria
            query.Criteria.AddCondition("msemr_patient", ConditionOperator.Equal, query_msemr_patient);
            query.Criteria.AddCondition("msemr_description", ConditionOperator.Like, query_msemr_description);

            // Add link-entity aw
            var aw = query.AddLink("msemr_careplan", "msemr_careplan", "msemr_careplanid");
            aw.EntityAlias = "aw";

            // Add filter aw.LinkCriteria
            aw.LinkCriteria.AddCondition("msemr_title", ConditionOperator.Like, aw_msemr_title);

            EntityCollection carePlanActivity = service.RetrieveMultiple(query);
            return carePlanActivity;
        }

    }
}
