using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PensionConsultants.Data.Access;
using PensionConsultants.Data.Utilities;

namespace PlanPerformance.Business.Components
{
    public class Access
    {
        /// <summary>
        /// Represents a database connection to the production database.
        /// </summary>
        public static DataAccessComponent DbAccess = new DataAccessComponent(DataAccessComponent.Connections.PCIDB_PlanPerformance);

        public static bool ConnectionSucceeded()
        {
            return DbAccess.ConnectionSucceeded();
        }
    }
}
