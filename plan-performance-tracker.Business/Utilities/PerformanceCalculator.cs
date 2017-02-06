using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DIH = DataIntegrationHub.Business.Entities;
using PlanPerformance.Business.Entities;

namespace PlanPerformance.Business.Utilities
{
    public class PerformanceCalculator
    {
        public readonly List<DIH.Plan> Plans;
        public readonly List<PlanMetric> PlanMetrics;
        public readonly List<Goal> Goals;
        public readonly List<GoalMetric> GoalMetrics;

        public PerformanceCalculator()
        {
            this.Plans = DIH.Plan.Get();
            this.PlanMetrics = PlanMetric.Get();
            this.Goals = Goal.Get();
            this.GoalMetrics = GoalMetric.Get();
        }

        public PerformanceCalculator(string team)
        {
            this.Plans = DIH.Plan.Get();
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
        public bool IsGreenPlan(DIH.Plan plan)
        {
            foreach (var goal in this.Goals)
            {
                if (Outperforms(plan, goal) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsRedPlan(DIH.Plan plan)
        {
            foreach (var goal in this.Goals)
            {
                if (Outperforms(plan, goal) == false)
                {
                    return true;
                }
            }

            return false;
        }

        public bool Outperforms(DIH.Plan plan, Goal goal)
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
            
            return Outperforms(metric, goal);
        }

        public bool Outperforms(PlanMetric planMetric, Goal goal)
        {
            var goalMetric = planMetric.GetGoalMetric(goal, this.GoalMetrics);
            if (goalMetric == null)
            {
                return true;
            }

            switch (goal.Team)
            {
                case "All":
                    return (planMetric.Value > goalMetric.Value);
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
    }
}
