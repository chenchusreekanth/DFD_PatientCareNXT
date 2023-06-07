using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Data;


namespace Persistent.PatientCareNXT.Dev
{
    public class CarePlanSubActivityPostCreateUpdate_Contact_CreditpointsBadge_Update : IPlugin
    {
        int StatusValue = 935000004;
        int Totalcareplansubactivities;
        int CompletedCarePlanSubactivities;
        int AllCompletedSubactivities;
        int AllSubactivities;
        int daily = 1;
        int biweekly = 2;
        int monthly = 3;
        int DailyActivity;
        int WeeklyActivity;
        int monthlyActivity;

        Guid PatientId;
        Guid CarePlanId;
        string name = string.Empty;
        int ActivityValue;
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
                    Guid initiatingUserId = context.InitiatingUserId;

                    // Obtain the target entity from the input parameters.
                    Entity entity = (Entity)context.InputParameters["Target"];
                    // Verify that the target entity represents an careplansubactivity.
                    // If not, this plug-in was not registered correctly.
                    if (entity.LogicalName != "hcp_careplansubactivity")
                        return;
                    tracingService.Trace("Entity logicalname :" + entity.LogicalName);
                    //Checking whether the entity record were updating or creating. 
                    if (context.MessageName == "Update")
                    {
                        tracingService.Trace("Update form");
                        Guid CarePlanSubActivityid = context.PrimaryEntityId;
                        Entity CarePlanSubActivity = service.Retrieve("hcp_careplansubactivity", CarePlanSubActivityid, new ColumnSet("hcp_patient", "hcp_msemr_careplan"));
                        PatientId = ((EntityReference)CarePlanSubActivity.Attributes["hcp_patient"]).Id;
                        CarePlanId = ((EntityReference)CarePlanSubActivity.Attributes["hcp_msemr_careplan"]).Id;
                        name = ((EntityReference)CarePlanSubActivity.Attributes["hcp_patient"]).Name.ToString();
                        tracingService.Trace("Contact name" + name);
                    }

                    else if (context.MessageName == "Create")
                    {
                        tracingService.Trace("Create form");
                        if (context.OutputParameters.Contains("id")) // careplansubactivity Record GUID
                        {
                           Guid regardingobjectId = new Guid(context.OutputParameters["id"].ToString());//GUID

                            PatientId = ((EntityReference)entity.Attributes["hcp_patient"]).Id;
                            CarePlanId = ((EntityReference)entity.Attributes["hcp_msemr_careplan"]).Id;
                            //name = ((EntityReference)entity.Attributes["hcp_patient"]).Name.ToString();
                            //tracingService.Trace("Contact name" + name);

                        }
                    }
                    
                    if (PatientId != Guid.Empty && CarePlanId != Guid.Empty)
                    {
                        EntityCollection TotalCarePlanSubActivities = getTotalCarePlanSubActivities(PatientId, CarePlanId, service);
                        if (TotalCarePlanSubActivities.Entities.Count > 0)
                        {
                            //Count of total careplan subactivities associate with Careplan of PatientId
                            Totalcareplansubactivities = TotalCarePlanSubActivities.Entities.Count;
                            tracingService.Trace("Totalcareplansubactivities" + TotalCarePlanSubActivities.Entities.Count);
                        }


                        EntityCollection CompletedCarePlanSubActivities = getCompletedCarePlanSubActivities(PatientId, CarePlanId, StatusValue, service);
                        if (CompletedCarePlanSubActivities.Entities.Count > 0)
                        {
                            //Count of total competed careplan subactivities associate with Careplan of PatientId
                            CompletedCarePlanSubactivities = CompletedCarePlanSubActivities.Entities.Count;
                            tracingService.Trace("CompletedCarePlanSubActivities" + CompletedCarePlanSubActivities.Entities.Count);
                        }

                        EntityCollection AllCompletedSubActivities = getAllCompletedCarePlanSubActivities(PatientId, StatusValue, service);
                        if (AllCompletedSubActivities.Entities.Count > 0)
                        {
                            //Count of total completed careplan subactivities associate with PatientID
                            AllCompletedSubactivities = AllCompletedSubActivities.Entities.Count;
                            tracingService.Trace("AllCompletedSubActivities" + AllCompletedSubActivities.Entities.Count);
                        }

                        EntityCollection AllSubActivities = getAllSubActivities(PatientId, service);
                        if (AllSubActivities.Entities.Count > 0)
                        {
                            //Count of total careplan subactivities associate with PatientID
                            AllSubactivities = AllSubActivities.Entities.Count;
                            tracingService.Trace("AllSubActivities" + AllSubActivities.Entities.Count);
                        }

                        //Calculating percentage from all subactivites which are completed by total subactivities associated with PatientId and Careplan
                        int PercentageActivities = (Int32)CompletedCarePlanSubactivities * 100 / Totalcareplansubactivities;                        
                        tracingService.Trace("Activities Percentage =" + PercentageActivities);

                        //Calculating percentage from all subactivites which are completed by total subactivities associated with PatientId
                        int Goalpercentage = (Int32)AllCompletedSubactivities * 100 / AllSubactivities;
                        tracingService.Trace("Goalpercentage =" + Goalpercentage);

                        //Updating the value of PercentageActivities in respective Careplan
                        Entity carePlan = new Entity("msemr_careplan");//task
                        carePlan.Attributes["msemr_careplanid"] = CarePlanId;
                        carePlan.Attributes["hcp_numberofsubactivities"] = Totalcareplansubactivities;
                        carePlan.Attributes["hcp_numberofcompletedsubactivities"] = CompletedCarePlanSubactivities;
                        carePlan.Attributes["hcp_goalprogress"] = PercentageActivities;
                        service.Update(carePlan);
                        tracingService.Trace("Care Plan record updated");

                        //Counting the careplansubactivities which are having timeinterval(Optionset in Careplan Activity) of Daily 
                        EntityCollection DailyCarePlanSubActivities = getDailyWeekMonthCarePlanSubActivities(PatientId, StatusValue, daily, service);
                        if (DailyCarePlanSubActivities.Entities.Count > 0)
                        {
                            DailyActivity = DailyCarePlanSubActivities.Entities.Count;
                        }
                        else
                        {
                            DailyActivity = 0;

                        }
                        //Counting the careplansubactivities which are having timeinterval(Optionset in Careplan Activity) of Weekly
                        EntityCollection WeeklyCarePlanSubActivities = getDailyWeekMonthCarePlanSubActivities(PatientId, StatusValue, biweekly, service);
                        if (WeeklyCarePlanSubActivities.Entities.Count > 0)
                        {


                            WeeklyActivity = WeeklyCarePlanSubActivities.Entities.Count;
                        }
                        else
                        {
                            WeeklyActivity = 0;

                        }
                        //Counting the careplansubactivities which are having timeinterval(Optionset in Careplan Activity) of Monthly
                        EntityCollection MonthlyCarePlanSubActivities = getDailyWeekMonthCarePlanSubActivities(PatientId, StatusValue, monthly, service);
                        if (MonthlyCarePlanSubActivities.Entities.Count > 0)
                        {
                            monthlyActivity = MonthlyCarePlanSubActivities.Entities.Count;
                        }
                        else
                        {
                            monthlyActivity = 0;

                        }

                        // Set Condition Values
                        string hcp_name_Daily = "DailyActivity";
                        int Daily = DailyweeklyMonthly(hcp_name_Daily, service);
                        string query_hcp_nameweekly = "WeeklyActivity";
                        int weekly = DailyweeklyMonthly(query_hcp_nameweekly, service);
                        // Set Condition Values
                        string monthly1 = "MonthlyActivity";
                        int monthlyvalue = DailyweeklyMonthly(monthly1, service);
                        String bronze = "Bronze";
                        int badge1 = DailyweeklyMonthly(bronze, service);
                        String silver = "Silver";
                        int badge2 = DailyweeklyMonthly(silver, service);
                        String gold = "Gold";
                        int badge3 = DailyweeklyMonthly(gold, service);
                        //Calculating the Credit points from completed subactivites with the dynamic values of hcp_configurableitems
                        int Credit = DailyActivity * Daily + WeeklyActivity * weekly + monthlyActivity * monthlyvalue;
                        tracingService.Trace("Credit" + Credit);

                        Entity contact = new Entity("contact");//task
                        contact.Attributes["contactid"] = PatientId;
                        contact.Attributes["hcp_totalsubactivities"] = AllSubactivities;
                        contact.Attributes["hcp_goalprogress"] = Goalpercentage;
                        contact.Attributes["hcp_totalcompletedsubactivities"] = AllCompletedSubactivities;
                        contact.Attributes["hcp_creditpoints"] = Credit;
                       //Setting the Badges in the contact record by comparing the dynamic values of hcp_configurableitems to credit points calculated
                        if (Credit >= 0 && Credit <= badge1)
                        {
                            contact.Attributes["hcp_creditpointbadge"] = "https://patientcare.powerappsportals.com/home-page/bronze150.png";
                        }
                        else if (Credit > badge1 && Credit <= badge2)
                        {
                            contact.Attributes["hcp_creditpointbadge"] = "https://patientcare.powerappsportals.com/home-page/silver150.png";
                        }
                        else if (Credit > badge2 && Credit <= badge3)
                        {
                            contact.Attributes["hcp_creditpointbadge"] = "https://patientcare.powerappsportals.com/home-page/Gold150.png";
                        }
                        service.Update(contact);
                        tracingService.Trace("Contact updated");
                        tracingService.Trace("Query started");
                        QueryExpression query = new QueryExpression
                        {
                            EntityName = "hcp_rewards",
                            ColumnSet = new ColumnSet(true),
                            Criteria = new FilterExpression
                            {
                                Conditions =
                                {
                                    new ConditionExpression
                                    {
                                        AttributeName = "hcp_patientid",
                                        Operator = ConditionOperator.Equal,
                                        Values = { PatientId }
                                    }
                                }
                            }
                        };
                        EntityCollection Rewards = service.RetrieveMultiple(query);
                        if (Rewards.Entities.Count < 1)
                        {
                            Entity Reward = new Entity("hcp_rewards");//task
                            Reward.Attributes["hcp_patientid"] = new EntityReference("contact", PatientId);
                            Reward.Attributes["hcp_name"] = name != null ? name : string.Empty;
                            Reward.Attributes["hcp_earnedpoints"] = Credit;
                            service.Create(Reward);
                            tracingService.Trace("Reward record Created");
                        }
                        else
                        {
                            Entity Reward = new Entity("hcp_rewards");
                            Reward["hcp_rewardsid"] = Rewards.Entities[0].Id;
                            Reward["hcp_earnedpoints"] = Credit;
                            if (Credit >= 5000 && Credit < 10000)
                            {
                                Reward["hcp_rewardname"] = "Medicines";
                            }
                            else if (Credit >= 10000 && Credit < 15000)
                            {
                                Reward["hcp_rewardname"] = "Appointment";
                            }
                            else if (Credit >= 15000)
                            {
                                Reward["hcp_rewardname"] = "Homevisit";
                            }
                            service.Update(Reward);
                            tracingService.Trace("Reward record Updated");
                        }
                    }
                }
            }
            catch (InvalidPluginExecutionException ex) { throw new InvalidPluginExecutionException(ex.Message); }
            catch (Exception ex) { throw new InvalidPluginExecutionException(ex.Message); }
        }
        public EntityCollection getTotalCarePlanSubActivities(Guid PatientId, Guid CarePlanId, IOrganizationService service)
        {
            QueryExpression SubactivityQuery = new QueryExpression("hcp_careplansubactivity");
            SubactivityQuery.ColumnSet = new ColumnSet("hcp_careplansubactivityid", "hcp_name", "createdon", "hcp_activitystatus");
            SubactivityQuery.Criteria.AddCondition("hcp_patient", ConditionOperator.Equal, PatientId);
            SubactivityQuery.Criteria.AddCondition("hcp_msemr_careplan", ConditionOperator.Equal, CarePlanId);
            EntityCollection getTotalCarePlanSubactivities = service.RetrieveMultiple(SubactivityQuery);
            return getTotalCarePlanSubactivities;
        }

        public EntityCollection getAllCompletedCarePlanSubActivities(Guid PatientId, int StatusValue, IOrganizationService service)
        {
            QueryExpression SubactivityQuery = new QueryExpression("hcp_careplansubactivity");
            SubactivityQuery.ColumnSet = new ColumnSet("hcp_careplansubactivityid", "hcp_name", "createdon", "hcp_activitystatus");
            SubactivityQuery.Criteria.AddCondition("hcp_patient", ConditionOperator.Equal, PatientId);
            SubactivityQuery.Criteria.AddCondition("hcp_activitystatus", ConditionOperator.Equal, StatusValue);
            EntityCollection getCompletedCarePlanSubactivities = service.RetrieveMultiple(SubactivityQuery);
            return getCompletedCarePlanSubactivities;


        }

        public EntityCollection getDailyWeekMonthCarePlanSubActivities(Guid PatientId, int StatusValue, int timeinterval, IOrganizationService service)
        {
            // Set Condition Values
            var query_hcp_patient = PatientId;
            var query_hcp_activitystatus = StatusValue;
            var careplanactivity_hcp_timeinterval = timeinterval;

            // Instantiate QueryExpression query
            var query = new QueryExpression("hcp_careplansubactivity");

            // Add columns to query.ColumnSet
            query.ColumnSet.AddColumns("hcp_name", "hcp_careplansubactivityid", "createdon");

            // Add filter query.Criteria
            query.Criteria.AddCondition("hcp_patient", ConditionOperator.Equal, query_hcp_patient);
            query.Criteria.AddCondition("hcp_activitystatus", ConditionOperator.Equal, query_hcp_activitystatus);

            // Add link-entity careplanactivity
            var careplanactivity = query.AddLink("msemr_careplanactivity", "hcp_msemr_careplanactivity", "msemr_careplanactivityid");
            careplanactivity.EntityAlias = "careplanactivity";

            // Add filter careplanactivity.LinkCriteria
            careplanactivity.LinkCriteria.AddCondition("hcp_timeinterval", ConditionOperator.Equal, careplanactivity_hcp_timeinterval);


            EntityCollection getTimeintervalCarePlanSubactivities = service.RetrieveMultiple(query);

            return getTimeintervalCarePlanSubactivities;

        }

        public EntityCollection getCompletedCarePlanSubActivities(Guid PatientId, Guid CarePlanId, int StatusValue, IOrganizationService service)
        {

            QueryExpression SubactivityQuery = new QueryExpression("hcp_careplansubactivity");
            SubactivityQuery.ColumnSet = new ColumnSet("hcp_careplansubactivityid", "hcp_name", "createdon");
            SubactivityQuery.Criteria.AddCondition("hcp_patient", ConditionOperator.Equal, PatientId);
            SubactivityQuery.Criteria.AddCondition("hcp_activitystatus", ConditionOperator.Equal, StatusValue);
            SubactivityQuery.Criteria.AddCondition("hcp_msemr_careplan", ConditionOperator.Equal, CarePlanId);
            EntityCollection getCompletedCarePlanSubactivities = service.RetrieveMultiple(SubactivityQuery);
            return getCompletedCarePlanSubactivities;

        }
        public EntityCollection getAllSubActivities(Guid PatientId, IOrganizationService service)
        {
            QueryExpression SubactivityQuery = new QueryExpression("hcp_careplansubactivity");
            SubactivityQuery.ColumnSet = new ColumnSet("hcp_careplansubactivityid", "hcp_name", "createdon", "hcp_activitystatus");
            SubactivityQuery.Criteria.AddCondition("hcp_patient", ConditionOperator.Equal, PatientId);
            EntityCollection getSubactivities = service.RetrieveMultiple(SubactivityQuery);
            return getSubactivities;

        }
        public int DailyweeklyMonthly(string Activity, IOrganizationService service)
        {

            // Instantiate QueryExpression query
            var query_0 = new QueryExpression("hcp_configurableitems");
            
            // Add columns to query.ColumnSet
            query_0.ColumnSet.AddColumns("hcp_name", "hcp_value");

            // Add filter query.Criteria
            query_0.Criteria.AddCondition("hcp_name", ConditionOperator.Equal, Activity);


            EntityCollection Rewards = service.RetrieveMultiple(query_0);

            if (Rewards.Entities.Count > 0)
            {
                Entity configurableitems = Rewards.Entities[0];
                if (configurableitems.Attributes.Contains("hcp_value").ToString() != null)
                {
                    ActivityValue = Convert.ToInt32(configurableitems.GetAttributeValue<string>("hcp_value"));
                }

            }

            return ActivityValue;
        }
    }
}
