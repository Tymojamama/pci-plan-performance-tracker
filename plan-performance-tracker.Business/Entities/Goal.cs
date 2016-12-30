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
    public class Goal : DatabaseEntity
    {
        public string Name;
        public string Type;
        public string Team;
        public string Frequency;

        private static string _tableName = "Goal";

        public Goal()
            : base(_tableName)
        {

        }

        public Goal(Guid primaryKey)
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
            base.AddColumn("Name", this.Name);
            base.AddColumn("Type", this.Type);
            base.AddColumn("Team", this.Team);
            base.AddColumn("Frequency", this.Frequency);
        }

        /// <summary>
        /// Resets the values of all public members to their values in the database.
        /// </summary>
        protected override void SetRegisteredMembers()
        {
            this.Name = (string)base.GetColumn("Name");
            this.Type = (string)base.GetColumn("Type");
            this.Team = (string)base.GetColumn("Team");
            this.Frequency = (string)base.GetColumn("Frequency");
        }

        public static List<Goal> Get()
        {
            var result = new List<Goal>();
            var dataTable = Access.DbAccess.ExecuteSqlQuery("SELECT " + _tableName + "Id FROM " + _tableName);

            foreach (DataRow row in dataTable.Rows)
            {
                var id = Guid.Parse(row[_tableName + "Id"].ToString());
                var goal = new Goal(id);
                result.Add(goal);
            }

            return result;
        }
    }
}
