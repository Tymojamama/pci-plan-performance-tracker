using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataIntegrationHub.Business.Entities;
using PlanPerformance.Business.Entities;

namespace PlanPerformance.Business.Utilities
{
    public class PerformanceCalculator
    {
        public readonly List<Plan> Plans;
        public readonly List<PlanMetric> PlanMetrics;
        public readonly List<Goal> Goals;
        public readonly List<GoalMetric> GoalMetrics;

        public PerformanceCalculator()
        {
            this.Plans = Plan.Get();
            this.PlanMetrics = PlanMetric.Get();
            this.Goals = Goal.Get();
            this.GoalMetrics = GoalMetric.Get();
        }

        public PerformanceCalculator(string team)
        {
            this.Plans = Plan.Get();
            this.PlanMetrics = PlanMetric.Get();
            this.Goals = Goal.Get().FindAll(x => x.Team == team);
            this.GoalMetrics = GoalMetric.Get();
        }

        /// <summary>
        /// Reflects that the plan outperforms all comperable goal metrics for
        /// active goals in the database.
        /// </summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        public bool IsGreenPlan(Plan plan)
        {
            var result = true;
            foreach (var goal in this.Goals)
            {
                if (Outperforms(plan, goal) == false)
                {
                    return false;
                }
            }

            return result;
        }

        public bool IsYellowPlan(Plan plan)
        {
            return !(this.IsGreenPlan(plan) || this.IsRedPlan(plan));
        }

        public bool IsRedPlan(Plan plan)
        {
            var result = false;
            foreach (var goal in this.Goals)
            {
                if (OutperformsBarely(plan, goal) == false && Outperforms(plan, goal) == false)
                {
                    return true;
                }
            }

            return result;
        }

        public bool Outperforms(Plan plan, Goal goal)
        {
            // If plan doesn't have any metrics, it passes
            var metrics = this.PlanMetrics.FindAll(x => x.PlanId == plan.PlanId);
            if (metrics.Count == 0)
            {
                return true;
            }

            // If plan doesn't have any metrics for the team, it passes
            var metric = metrics.FindAll(x => x.GoalId == goal.Id).OrderByDescending(x => x.ValueAsOf).FirstOrDefault();
            if (metric == null)
            {
                return true;
            }

            // Find the latest goal metric for the goal and plan
            var goalMetrics = this.GoalMetrics
                .FindAll(x => x.PlanId == plan.PlanId)
                .FindAll(x => x.ValueAsOf.Date <= metric.ValueAsOf.Date);
            var goalMetric = goalMetrics.OrderByDescending(x => x.ValueAsOf).FirstOrDefault();

            // If no goal metric for goal and plan, it passes
            if (goalMetric == null)
            {
                return true;
            }
            
            return Outperforms(metric, goalMetric);
        }

        public bool Outperforms(PlanMetric planMetric, GoalMetric goalMetric)
        {
            var goal = this.Goals.Find(x => x.Id == goalMetric.GoalId);
            if (goal == null)
            {
                return true;
            }

            switch (goal.Team)
            {
                case "Investment Services":
                    return (planMetric.Value > goalMetric.Value);
                case "RetireAdvisers":
                    return (planMetric.Value > goalMetric.Value);
                case "Vendor Services":
                    return (planMetric.Value < goalMetric.Value);
                default:
                    return false;
            }
        }

        public bool OutperformsBarely(Plan plan, Goal goal)
        {
            // If plan doesn't have any metrics, it passes
            var metrics = this.PlanMetrics.FindAll(x => x.PlanId == plan.PlanId);
            if (metrics.Count == 0)
            {
                return true;
            }

            // If plan doesn't have any metrics for the team, it passes
            var metric = metrics.FindAll(x => x.GoalId == goal.Id).OrderByDescending(x => x.ValueAsOf).FirstOrDefault();
            if (metric == null)
            {
                return true;
            }

            // Find the latest goal metric for the goal and plan
            var goalMetrics = this.GoalMetrics
                .FindAll(x => x.PlanId == plan.PlanId)
                .FindAll(x => x.ValueAsOf.Date <= metric.ValueAsOf.Date);
            var goalMetric = goalMetrics.OrderByDescending(x => x.ValueAsOf).FirstOrDefault();

            // If no goal metric for goal and plan, it passes
            return (goalMetric != null && OutperformsBarely(metric, goalMetric));
        }

        public bool OutperformsBarely(PlanMetric planMetric, GoalMetric goalMetric)
        {
            var goal = this.Goals.Find(x => x.Id == goalMetric.GoalId);
            if (goal == null)
            {
                return true;
            }

            var metrics = this.PlanMetrics.FindAll(x => x.GoalId == goal.Id && x.ValueAsOf.Date <= planMetric.ValueAsOf.Date).Select(x => x.Value).ToArray();
            var average = metrics.Average();
            var sumOfSquaresOfDifferences = metrics.Select(val => (val - average) * (val - average)).Sum();
            var stdev = (decimal)Math.Sqrt((double)(sumOfSquaresOfDifferences / metrics.Length));

            switch (goal.Team)
            {
                case "Investment Services":
                    return (planMetric.Value + stdev > goalMetric.Value);
                case "RetireAdvisers":
                    return (planMetric.Value + stdev > goalMetric.Value);
                case "Vendor Services":
                    return (planMetric.Value - stdev < goalMetric.Value);
                default:
                    return false;
            }
        }
    }
}
