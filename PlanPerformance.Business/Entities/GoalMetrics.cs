using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using PlanPerformance.Business.Components;
using PensionConsultants.Data.Utilities;

namespace PlanPerformance.Business.Entities
{
    public class GoalMetric : DatabaseEntity
    {
        public Guid GoalId;
        public Guid PlanId;
        public string Industry;
        public decimal Value;
        public DateTime ValueAsOf;

        private static string _tableName = "GoalMetric";

        public GoalMetric()
            : base(_tableName)
        {

        }

        public GoalMetric(Guid primaryKey)
            : base(_tableName, primaryKey)
        {
            RefreshMembers();
        }

        /// <summary>
        /// Registers the instance's members with the abstract class in order to perform database operations. Do not register members
        /// that exist within the abstract class (e.g. CreatedOn).
        /// </summary>
        protected override void RegisterMembers()
        {
            base.AddColumn("GoalId", this.GoalId);
            base.AddColumn("PlanId", this.PlanId);
            base.AddColumn("Industry", this.Industry);
            base.AddColumn("Value", this.Value);
            base.AddColumn("ValueAsOf", this.ValueAsOf);
        }

        /// <summary>
        /// Resets the values of all public members to their values in the database.
        /// </summary>
        protected override void SetRegisteredMembers()
        {
            this.GoalId = (Guid)base.GetColumn("GoalId");
            this.PlanId = (Guid)base.GetColumn("PlanId");
            this.Industry = (string)base.GetColumn("Industry");
            this.Value = (decimal)base.GetColumn("Value");
            this.ValueAsOf = (DateTime)base.GetColumn("ValueAsOf");
        }

        public static List<GoalMetric> Get()
        {
            var result = new List<GoalMetric>();
            var dataTable = Access.DbAccess.ExecuteSqlQuery("SELECT " + _tableName + "Id FROM " + _tableName);

            foreach (DataRow row in dataTable.Rows)
            {
                var id = Guid.Parse(row[_tableName + "Id"].ToString());
                var goal = new GoalMetric(id);
                result.Add(goal);
            }

            return result;
        }
    }
}
