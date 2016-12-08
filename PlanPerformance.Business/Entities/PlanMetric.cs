﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using PlanPerformance.Business.Components;
using PensionConsultants.Data.Utilities;

namespace PlanPerformance.Business.Entities
{
    public class PlanMetric : DatabaseEntity
    {
        public Guid GoalId;
        public Guid PlanId;
        public decimal Value;
        public DateTime ValueAsOf;
        public bool IsBaseline;

        private static string _tableName = "PlanMetric";

        public PlanMetric()
            : base(_tableName)
        {

        }

        public PlanMetric(Guid primaryKey)
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
            base.AddColumn("Value", this.Value);
            base.AddColumn("ValueAsOf", this.ValueAsOf);
            base.AddColumn("IsBaseline", this.IsBaseline);
        }

        /// <summary>
        /// Resets the values of all public members to their values in the database.
        /// </summary>
        protected override void SetRegisteredMembers()
        {
            this.GoalId = (Guid)base.GetColumn("GoalId");
            this.PlanId = (Guid)base.GetColumn("PlanId");
            this.Value = (decimal)base.GetColumn("Value");
            this.ValueAsOf = (DateTime)base.GetColumn("ValueAsOf");
            this.IsBaseline = bool.Parse(base.GetColumn("IsBaseline").ToString());
        }

        public static List<PlanMetric> Get()
        {
            var result = new List<PlanMetric>();
            var dataTable = Access.DbAccess.ExecuteSqlQuery("SELECT " + _tableName + "Id FROM " + _tableName);

            foreach (DataRow row in dataTable.Rows)
            {
                var id = Guid.Parse(row[_tableName + "Id"].ToString());
                var goal = new PlanMetric(id);
                result.Add(goal);
            }

            return result;
        }
    }
}
