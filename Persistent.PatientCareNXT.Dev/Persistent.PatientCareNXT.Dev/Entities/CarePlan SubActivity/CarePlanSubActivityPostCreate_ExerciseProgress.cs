using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk.Query;
using System.Data;

namespace Persistent.PatientCareNXT.Dev 
{
    public class CarePlanSubActivityPostCreate_ExerciseProgress : IPlugin
    {
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
                    Entity Careplansubactivity = (Entity)context.InputParameters["Target"];
                    // Verify that the target entity represents an Careplansubactivity.
                    // If not, this plug-in was not registered correctly.
                    if (Careplansubactivity.LogicalName != "hcp_careplansubactivity")
                        return;
                    tracingService.Trace("Entity Logical Name =" + Careplansubactivity.LogicalName);
                    Guid contactId = Careplansubactivity.GetAttributeValue<EntityReference>("hcp_patient").Id;
                    tracingService.Trace("Contact Guid =" + contactId);
                    EntityCollection careplanSubActivities = getCareplanSubActivities(contactId, service, tracingService);
                    tracingService.Trace("Records Filtered =" + careplanSubActivities);
                    //creating database table 
                    var result = (from d in careplanSubActivities.Entities
                                  group d by
                                  new { Month = ((DateTime)d["hcp_activitydate"]).ToString("MM"), Status = ((Microsoft.Xrm.Sdk.OptionSetValue)d["hcp_activitystatus"]).Value }
                                into g
                                  select new
                                  {
                                      Month = g.Key.Month,
                                      Count = g.Sum(t => Convert.ToUInt16(1)),
                                      Status1 = g.Key.Status
                                  }).ToList();
                    DataTable dbCal = new DataTable();
                    dbCal.Columns.Add("Month", typeof(int));
                    dbCal.Columns.Add("Count", typeof(int));
                    dbCal.Columns.Add("Status", typeof(int));
                    dbCal.Columns.Add("Percent", typeof(double));
                    foreach (var item in result)
                    {
                        dbCal.Rows.Add(item.Month, item.Count, item.Status1);
                    }

                    for (int i = 0; i < dbCal.Rows.Count; i++)
                    {
                        int month = Convert.ToInt16(dbCal.Rows[i]["Month"]);
                        int count = Convert.ToInt16(dbCal.Rows[i]["Count"]);
                        int totalCount = dbCal.AsEnumerable().Where(row => row.Field<int>("Month") == month).Sum(row => row.Field<int>("Count"));
                        double per = (double)count * 100 / totalCount;
                        dbCal.Rows[i]["Percent"] = per;
                    }

                    DataView dv = new DataView(dbCal);
                    dv.RowFilter = "Status = 935000004";
                    dbCal = dv.ToTable();
                    //Update each record with exact data 
                    foreach (Entity carePlanActivity in careplanSubActivities.Entities)
                    {
                        if (carePlanActivity.Attributes.Contains("hcp_activitydate") && carePlanActivity.Attributes["hcp_activitydate"] != null)
                        {
                            int month_Num = ((DateTime)carePlanActivity.Attributes["hcp_activitydate"]).Month;
                            double month_Percent = dbCal.AsEnumerable().Where(row => row.Field<int>("Month") == month_Num).Sum(row => row.Field<double>("Percent"));


                            Entity carePlan = new Entity("hcp_careplansubactivity");

                            carePlan.Attributes["hcp_careplansubactivityid"] = carePlanActivity.Id;
                            carePlan.Attributes["hcp_progressbymonth"] = month_Percent;
                            service.Update(carePlan);
                            tracingService.Trace("Progress by month updated");
                        }

                    }


                }

            }
            catch (InvalidPluginExecutionException ex) { throw new InvalidPluginExecutionException(ex.Message); }
            catch (Exception ex) { throw new InvalidPluginExecutionException(ex.Message); }
        }

        public EntityCollection getCareplanSubActivities(Guid contactId, IOrganizationService service, ITracingService tracingService)
        {
            tracingService.Trace("Query Started");
            QueryExpression query = new QueryExpression
            {
                EntityName = "hcp_careplansubactivity",
                ColumnSet = new ColumnSet("hcp_careplansubactivityid", "hcp_name", "hcp_activitystatus", "hcp_activitydate"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName = "hcp_patient",
                            Operator = ConditionOperator.Equal,
                            Values = { contactId }
                        },
                        new ConditionExpression
                        {
                            AttributeName = "hcp_name",
                            Operator = ConditionOperator.Like,
                            Values = { "%Exercise%" }
                        }

                    }
                }
            };
            EntityCollection carePlanActivities = service.RetrieveMultiple(query);
            tracingService.Trace("carePlan Subactivities count =" + carePlanActivities);
            return carePlanActivities;            
        }
    }
}
