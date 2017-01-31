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
    public class Plan : DatabaseEntity
    {
        public string ActionPlan;

        private static string _tableName = "Plan";

        public Plan()
            : base(_tableName)
        {

        }

        public Plan(Guid primaryKey)
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
            base.AddColumn("ActionPlan", this.ActionPlan);
        }

        /// <summary>
        /// Resets the values of all public members to their values in the database.
        /// </summary>
        protected override void SetRegisteredMembers()
        {
            this.ActionPlan = (String)base.GetColumn("ActionPlan");
        }
    }
}
