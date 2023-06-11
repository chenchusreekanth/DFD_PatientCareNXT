using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Runtime.Serialization;
using System.ServiceModel;
using Microsoft.Xrm.Sdk.Query;

namespace Template_creation
{
    public sealed class ContactDietSuggestion : CodeActivity
    {
        [Input("HYPERTENSION Template")]
        public InArgument<string> inTemplateName1 { get; set; }

        [Input("ONCOLOGY")]
        public InArgument<string> inTemplateName2 { get; set; }

        [Input("CARDIOLOGY")]
        public InArgument<string> inTemplateName3 { get; set; }


        [Input("FAMILY MEDICINE")]
        public InArgument<string> inTemplateName4 { get; set; }

        [Input("GASTROLOGY")]
        public InArgument<string> inTemplateName5 { get; set; }

        [Input("PHYSICAL THERAPY")]
        public InArgument<string> inTemplateName6 { get; set; }

        [Input("PULMONARY")]
        public InArgument<string> inTemplateName7 { get; set; }


        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();

            if (tracingService == null)
            {
                throw new InvalidPluginExecutionException("Failed to retrieve tracing service.");
            }

            // Create the context
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();


            if (context == null)
            {
                throw new InvalidPluginExecutionException("Failed to retrieve workflow context.");
            }

            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                int HyperTension = 0;
                int Oncology = 0;
                int Cardiology = 0;
                int FamilyMedicine = 0;
                int Gastrology = 0;
                int PhysicalTherapy = 0;
                int Pulmonary = 0;

                string templateName1 = inTemplateName1.Get<string>(executionContext);
                tracingService.Trace("templateName1:" + templateName1);
                string templateName2 = inTemplateName2.Get<string>(executionContext);
                string templateName3 = inTemplateName3.Get<string>(executionContext);
                string templateName4 = inTemplateName4.Get<string>(executionContext);
                string templateName5 = inTemplateName5.Get<string>(executionContext);
                string templateName6 = inTemplateName6.Get<string>(executionContext);
                string templateName7 = inTemplateName7.Get<string>(executionContext);

                Guid contactId = context.PrimaryEntityId;
                tracingService.Trace("contactid:" + contactId);
                EntityCollection observations = retreiveAllObservations(contactId, service);
                tracingService.Trace("observations.Entities.Count:" + observations.Entities.Count);
                if (observations.Entities.Count > 0)
                {
                    foreach (Entity observation in observations.Entities)
                    {
                        tracingService.Trace("observation for loop");
                        if (observation.GetAttributeValue<EntityReference>("msemr_bodysite") != null)
                        {
                            tracingService.Trace("Bodysite");

                            EntityReference bodySite = observation.GetAttributeValue<EntityReference>("msemr_bodysite");
                            Entity codabledisease = service.Retrieve(bodySite.LogicalName, bodySite.Id, new ColumnSet("msemr_name"));
                            tracingService.Trace("codabledisease:" + codabledisease);
                            string name = codabledisease.GetAttributeValue<string>("msemr_name");
                            tracingService.Trace("msemr_name:" + name);
                            switch (name)
                            {
                                case "HYPERTENSION":
                                    HyperTension++;
                                    break;
                                case "ONCOLOGY":
                                    Oncology++;
                                    break;
                                case "CARDIOLOGY":
                                    Cardiology++;
                                    break;
                                case "FAMILY MEDICINE":
                                    FamilyMedicine++;
                                    break;
                                case "GASTROLOGY":
                                    Gastrology++;
                                    break;
                                case "PHYSICAL THERAPY":
                                    PhysicalTherapy++;
                                    break;
                                case "PULMONARY":
                                    Pulmonary++;
                                    break;

                                default:
                                    break;

                            }
                        }
                    }

                }

                tracingService.Trace("HyperTension:" + HyperTension);
                tracingService.Trace("templateName1:" + templateName1);

                if (HyperTension > 0 && templateName1 != null)
                {
                    EntityCollection templates = GetTemplate(templateName1, service);
                    tracingService.Trace("templates.Entities.Count:" + templates.Entities.Count);
                    if (templates.Entities.Count > 0)
                    {
                        Entity TemplateRecord = templates.Entities[0];
                        CreateWordTemplate(TemplateRecord, contactId, service, tracingService);
                        tracingService.Trace("word created");

                    }
                }
                if (Oncology > 0 && templateName2 != null)
                {
                    EntityCollection templates = GetTemplate(templateName2, service);
                    if (templates.Entities.Count > 0)
                    {
                        Entity TemplateRecord = templates.Entities[0];
                        CreateWordTemplate(TemplateRecord, contactId, service, tracingService);

                    }
                }
                if (Cardiology > 0 && templateName3 != null)
                {
                    EntityCollection templates = GetTemplate(templateName3, service);
                    if (templates.Entities.Count > 0)
                    {
                        Entity TemplateRecord = templates.Entities[0];
                        CreateWordTemplate(TemplateRecord, contactId, service, tracingService);

                    }
                }
                if (FamilyMedicine > 0 && templateName4 != null)
                {
                    EntityCollection templates = GetTemplate(templateName4, service);
                    if (templates.Entities.Count > 0)
                    {
                        Entity TemplateRecord = templates.Entities[0];
                        CreateWordTemplate(TemplateRecord, contactId, service, tracingService);

                    }
                }
                if (Gastrology > 0 && templateName5 != null)
                {
                    EntityCollection templates = GetTemplate(templateName5, service);
                    if (templates.Entities.Count > 0)
                    {
                        Entity TemplateRecord = templates.Entities[0];
                        CreateWordTemplate(TemplateRecord, contactId, service, tracingService);

                    }
                }
                if (PhysicalTherapy > 0 && templateName6 != null)
                {
                    EntityCollection templates = GetTemplate(templateName6, service);
                    if (templates.Entities.Count > 0)
                    {
                        Entity TemplateRecord = templates.Entities[0];
                        CreateWordTemplate(TemplateRecord, contactId, service, tracingService);

                    }
                }
                if (Pulmonary > 0 && templateName7 != null)
                {
                    EntityCollection templates = GetTemplate(templateName7, service);
                    if (templates.Entities.Count > 0)
                    {
                        Entity TemplateRecord = templates.Entities[0];
                        CreateWordTemplate(TemplateRecord, contactId, service, tracingService);

                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> e)
            {
                tracingService.Trace("Exception: {0}", e.ToString());

                throw;
            }
        }
        public EntityCollection retreiveAllObservations(Guid contactId, IOrganizationService service)
        {
            QueryExpression observationsQuery = new QueryExpression("msemr_observation");
            observationsQuery.ColumnSet = new ColumnSet(true);
            observationsQuery.Criteria.AddCondition("msemr_subjecttypepatient", ConditionOperator.Equal, contactId);
            EntityCollection observationsColl = service.RetrieveMultiple(observationsQuery);
            return observationsColl;
        }
        public EntityCollection GetTemplate(string templateName, IOrganizationService service)
        {
            QueryExpression template = new QueryExpression("documenttemplate");
            template.ColumnSet = new ColumnSet("name");
            template.Criteria.AddCondition("name", ConditionOperator.Equal, templateName);
            EntityCollection templateRetrieve = service.RetrieveMultiple(template);
            return templateRetrieve;
        }
        public void CreateWordTemplate(Entity TemplateRecord, Guid contactId, IOrganizationService service, ITracingService tracingService)
        {
            OrganizationRequest createDocRequest = new OrganizationRequest("SetWordTemplate");

            createDocRequest["Target"] = new EntityReference("contact", contactId);
            createDocRequest["SelectedTemplate"] = new EntityReference("documenttemplate", TemplateRecord.Id);
            var response = service.Execute(createDocRequest);
            tracingService.Trace("Executed for template");
        }
    }
}