using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Runtime.Serialization;


namespace Persistent.PatientCareNXT.Dev
{
    public class AppointmentEMRPostCreate_SpecialityonContact : IPlugin
    {
        string SpecialityName;
        EntityReference contact;
        public void Execute(IServiceProvider serviceProvider)
        {
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
                    Entity entity = (Entity)context.InputParameters["Target"];
                    // Verify that the target entity represents an account.
                    // If not, this plug-in was not registered correctly.
                    if (entity.LogicalName != "msemr_appointmentemr")
                        return;
                    Guid appointmentId = context.PrimaryEntityId; // Account guid

                    if (context.PostEntityImages.Contains("PostImage") && context.PostEntityImages["PostImage"] is Entity)
                    {
                        Entity postMessageImage = (Entity)context.PreEntityImages["PostImage"];
                        contact = postMessageImage.Contains("msemr_actorpatient") ? postMessageImage.GetAttributeValue<EntityReference>("msemr_actorpatient") : null;
                        tracingService.Trace("contact" + contact.Id);

                    }
                    Entity Appointment = GetLatestAppointment(contact.Id, service, tracingService);
                    if (Appointment != null && Appointment.Contains("cr58e_msemr_practitionerrolespecialty") == true && Appointment.GetAttributeValue<EntityReference>("cr58e_msemr_practitionerrolespecialty") != null)
                    {
                        EntityReference speciality = Appointment.GetAttributeValue<EntityReference>("cr58e_msemr_practitionerrolespecialty");
                        Entity entSpeciality = service.Retrieve("msemr_practitionerrolespecialty", speciality.Id, new ColumnSet(true));
                        SpecialityName = entSpeciality.GetAttributeValue<string>("msemr_name");
                        tracingService.Trace("SpecialityName" + SpecialityName);
                        tracingService.Trace("SpecialityName" + speciality.Name);
                        Entity ContactUpdate = new Entity("contact");
                        ContactUpdate.Attributes["contactid"] = contact.Id;
                        ContactUpdate.Attributes["hcp_speciality"] = SpecialityName;
                        service.Update(ContactUpdate);

                    }


                }
            }
            catch (Exception ex)
            {
            }

        }
        public Entity GetLatestAppointment(Guid contactId, IOrganizationService service, ITracingService tracingService)
        {
            var query_msemr_actorpatient = contactId;

            // Instantiate QueryExpression query
            var query = new QueryExpression("msemr_appointmentemr");
            query.TopCount = 1;
            // Add columns to query.ColumnSet
            query.ColumnSet.AddColumns("cr58e_msemr_practitionerrolespecialty", "msemr_endtime", "msemr_actorpatient");

            // Add conditions to query.Criteria
            query.Criteria.AddCondition("msemr_actorpatient", ConditionOperator.Equal, query_msemr_actorpatient);

            // Add orders
            query.AddOrder("msemr_endtime", OrderType.Descending);

            Entity Appointment = service.RetrieveMultiple(query).Entities[0];
            tracingService.Trace("Appointment :" + Appointment);
            return Appointment;
        }
    }
}