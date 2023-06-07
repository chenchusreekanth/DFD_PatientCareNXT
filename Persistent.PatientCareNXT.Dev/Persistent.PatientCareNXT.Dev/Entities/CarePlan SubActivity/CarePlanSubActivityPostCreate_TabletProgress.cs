using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk.Query;
using System.Data;


namespace Persistent.PatientCareNXT.Dev
{
    public class CarePlanSubActivityPostCreate_TabletProgress : IPlugin
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
                    Entity careplansubactivity = (Entity)context.InputParameters["Target"];
                    // Verify that the target entity represents an careplansubactivity.
                    // If not, this plug-in was not registered correctly.
                    if (careplansubactivity.LogicalName != "hcp_careplansubactivity")
                        return;
                    tracingService.Trace("Entity Logical Name =" + careplansubactivity.LogicalName);
                    Guid contactId = careplansubactivity.GetAttributeValue<EntityReference>("hcp_patient").Id;
                    tracingService.Trace("Contact Guid ="+ contactId);                    
                    EntityCollection careplanSubActivities = getCareplanSubActivities(contactId, service);
                    tracingService.Trace("careplanSubActivities =" + careplanSubActivities);
                    //Creating Database table
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
                    //Updating each record with exact data
                    foreach (Entity carePlanActivity in careplanSubActivities.Entities)
                    {
                        if (carePlanActivity.Attributes.Contains("hcp_activitydate") && carePlanActivity.Attributes["hcp_activitydate"] != null)
                        {
                            int month_Num = ((DateTime)carePlanActivity.Attributes["hcp_activitydate"]).Month;
                            double month_Percent = dbCal.AsEnumerable().Where(row => row.Field<int>("Month") == month_Num).Sum(row => row.Field<double>("Percent"));
                            tracingService.Trace("Monthly Percentage ="+ month_Percent);
                            Entity carePlan = new Entity("hcp_careplansubactivity");
                            carePlan.Attributes["hcp_careplansubactivityid"] = carePlanActivity.Id;
                            carePlan.Attributes["hcp_progressbymonth"] = month_Percent;
                            service.Update(carePlan);
                            tracingService.Trace("CarePlan SubActivity Updated Successfully");
                        }

                    }


                }

            }
            catch (InvalidPluginExecutionException ex) { throw new InvalidPluginExecutionException(ex.Message); }
            catch (Exception ex) { throw new InvalidPluginExecutionException(ex.Message);}
            }
        public EntityCollection getCareplanSubActivities(Guid contactId, IOrganizationService service)
        {

            QueryExpression CareplansubactivityQuery = new QueryExpression("hcp_careplansubactivity");
            CareplansubactivityQuery.ColumnSet = new ColumnSet("hcp_careplansubactivityid", "hcp_name", "hcp_activitystatus", "hcp_activitydate");
            CareplansubactivityQuery.Criteria.AddCondition("hcp_name", ConditionOperator.Like, "%Tablets%");
            CareplansubactivityQuery.Criteria.AddCondition("hcp_patient", ConditionOperator.Equal, contactId);
            EntityCollection carePlanActivities = service.RetrieveMultiple(CareplansubactivityQuery);
            return carePlanActivities;

        }
    }
}
