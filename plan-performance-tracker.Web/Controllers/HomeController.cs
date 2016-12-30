using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PlanPerformance.Business.Entities;
using DataIntegrationHub.Business.Entities;

namespace PlanPerformance.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home";
            var plans = DataIntegrationHub.Business.Entities.Plan.Get()
                .FindAll(x => x.IsManagedPlan)
                .OrderBy(x => x.Name).ToList();
            return View(plans);
        }

        public ActionResult Plans()
        {
            ViewBag.Title = "Plans";
            var plans = DataIntegrationHub.Business.Entities.Plan.Get()
                .FindAll(x => x.IsManagedPlan)
                .OrderBy(x => x.Name).ToList();
            return View(plans);
        }

        public ActionResult Plan()
        {
            var id = Request.QueryString["id"];

            if (String.IsNullOrWhiteSpace(id))
            {
                throw new Exception("Cannot create plans from this application. Please do so in CRM.");
            }
            else
            {
                var plan = new DataIntegrationHub.Business.Entities.Plan(Guid.Parse(id));
                ViewBag.Title = plan.Name;
                return View(plan);
            }

        }

        public ActionResult GoalMetrics()
        {
            ViewBag.Title = "Goal Metrics";
            var goalMetrics = PlanPerformance.Business.Entities.GoalMetric.Get();
            return View(goalMetrics);
        }

        public ActionResult GoalMetric()
        {
            var id = Request.QueryString["id"];
            var goalId = Request.QueryString["goalId"];
            var goalMetric = new PlanPerformance.Business.Entities.GoalMetric();

            if (!String.IsNullOrWhiteSpace(goalId))
            {
                goalMetric.GoalId = Guid.Parse(goalId);
            }

            if (String.IsNullOrWhiteSpace(id))
            {
                ViewBag.Title = "New Plan Metric";
                goalMetric.ValueAsOf = DateTime.Now;
            }
            else
            {
                goalMetric = new PlanPerformance.Business.Entities.GoalMetric(Guid.Parse(id));
                //ViewBag.Title = planMetric.Name;
            }

            return View(goalMetric);
        }

        public ActionResult SaveGoalMetric(FormCollection collection)
        {
            var existingRecord = Boolean.Parse(collection["goal-metric-existing-record"].ToString());
            var id = Guid.Parse(collection["goal-metric-id"].ToString());
            var goalId = collection["goal-metric-goalid"].ToString();
            var planId = collection["goal-metric-planid"].ToString();
            var industry = collection["goal-metric-industry"].ToString();
            var value = collection["goal-metric-value"].ToString();
            var valueAsOf = collection["goal-metric-value-as-of"].ToString();

            PlanPerformance.Business.Entities.GoalMetric goalMetric;
            if (existingRecord == false)
            {
                goalMetric = new PlanPerformance.Business.Entities.GoalMetric();
            }
            else
            {
                goalMetric = new PlanPerformance.Business.Entities.GoalMetric(id);
            }

            goalMetric.GoalId = Guid.Parse(goalId);
            goalMetric.PlanId = Guid.Parse(planId);
            goalMetric.Industry = industry;
            goalMetric.Value = Decimal.Parse(value);
            goalMetric.ValueAsOf = DateTime.Parse(valueAsOf);
            goalMetric.SaveRecordToDatabase(new Guid("17F6FCEB-CF02-E411-9726-D8D385C29900"));

            return View();
        }
        
        public ActionResult DeleteGoalMetric(FormCollection collection)
        {
            var id = collection["goal-metric-id"].ToString();
            var goalMetric = new PlanPerformance.Business.Entities.GoalMetric();

            if (String.IsNullOrWhiteSpace(id))
            {
                return View();
            }
            else
            {
                goalMetric = new PlanPerformance.Business.Entities.GoalMetric(Guid.Parse(id));
                goalMetric.DeleteRecordFromDatabase();
            }

            return View();
        }

        public ActionResult PlanMetrics()
        {
            ViewBag.Title = "Plan Metrics";
            var planMetrics = PlanPerformance.Business.Entities.PlanMetric.Get();
            return View(planMetrics);
        }

        public ActionResult PlanMetric()
        {
            var id = Request.QueryString["id"];
            var planId = Request.QueryString["planId"];
            var planMetric = new PlanPerformance.Business.Entities.PlanMetric();

            if (!String.IsNullOrWhiteSpace(planId))
            {
                planMetric.PlanId = Guid.Parse(planId);
            }

            if (String.IsNullOrWhiteSpace(id))
            {
                ViewBag.Title = "New Plan Metric";
                planMetric.ValueAsOf = DateTime.Now;
            }
            else
            {
                planMetric = new PlanPerformance.Business.Entities.PlanMetric(Guid.Parse(id));
                //ViewBag.Title = planMetric.Name;
            }

            return View(planMetric);
        }

        public ActionResult SavePlanMetric(FormCollection collection)
        {
            var existingRecord = Boolean.Parse(collection["plan-metric-existing-record"].ToString());
            var id = Guid.Parse(collection["plan-metric-id"].ToString());
            var goalId = collection["plan-metric-goalid"].ToString();
            var planId = collection["plan-metric-planid"].ToString();
            var value = collection["plan-metric-value"].ToString();
            var valueAsOf = collection["plan-metric-value-as-of"].ToString();
            var isBaseline = collection["plan-metric-is-baseline"].ToString().Replace(",false","");

            PlanPerformance.Business.Entities.PlanMetric planMetric;
            if (existingRecord == false)
            {
                planMetric = new PlanPerformance.Business.Entities.PlanMetric();
            }
            else
            {
                planMetric = new PlanPerformance.Business.Entities.PlanMetric(id);
            }

            planMetric.GoalId = Guid.Parse(goalId);
            planMetric.PlanId = Guid.Parse(planId);
            planMetric.Value = Decimal.Parse(value);
            planMetric.ValueAsOf = DateTime.Parse(valueAsOf);
            planMetric.IsBaseline = bool.Parse(isBaseline);
            planMetric.SaveRecordToDatabase(new Guid("17F6FCEB-CF02-E411-9726-D8D385C29900"));

            return View();
        }

        public ActionResult DeletePlanMetric(FormCollection collection)
        {
            var id = collection["plan-metric-id"].ToString();
            var planMetric = new PlanPerformance.Business.Entities.PlanMetric();

            if (String.IsNullOrWhiteSpace(id))
            {
                return View();
            }
            else
            {
                planMetric = new PlanPerformance.Business.Entities.PlanMetric(Guid.Parse(id));
                planMetric.DeleteRecordFromDatabase();
            }

            return View();
        }

        public ActionResult Goals()
        {
            ViewBag.Title = "Goals";
            var goals = PlanPerformance.Business.Entities.Goal.Get();
            return View(goals);
        }

        public ActionResult Goal()
        {
            var id = Request.QueryString["id"];
            var goal = new PlanPerformance.Business.Entities.Goal();

            if (String.IsNullOrWhiteSpace(id))
            {
                ViewBag.Title = "New Goal";
            }
            else
            {
                goal = new PlanPerformance.Business.Entities.Goal(Guid.Parse(id));
                ViewBag.Title = goal.Name;
            }

            return View(goal);
        }

        public ActionResult SaveGoal(FormCollection collection)
        {
            var existingRecord = Boolean.Parse(collection["goal-existing-record"].ToString());
            var id = Guid.Parse(collection["goal-id"].ToString());
            var name = collection["goal-name"].ToString();
            var type = collection["goal-type"].ToString();
            var team = collection["goal-team"].ToString();
            var frequency = collection["goal-frequency"].ToString();

            PlanPerformance.Business.Entities.Goal goal;
            if (existingRecord == false)
            {
                goal = new PlanPerformance.Business.Entities.Goal();
            }
            else
            {
                goal = new PlanPerformance.Business.Entities.Goal(id);
            }

            goal.Name = name;
            goal.Type = type;
            goal.Team = team;
            goal.Frequency = frequency;
            goal.SaveRecordToDatabase(new Guid("17F6FCEB-CF02-E411-9726-D8D385C29900"));

            return View();
        }

        public ActionResult DeleteGoal(FormCollection collection)
        {
            var id = collection["goal-id"];
            var goal = new PlanPerformance.Business.Entities.Goal();

            if (String.IsNullOrWhiteSpace(id))
            {
                return View();
            }
            else
            {
                goal = new PlanPerformance.Business.Entities.Goal(Guid.Parse(id));
                goal.DeleteRecordFromDatabase();
            }

            return View();
        }
    }
}
