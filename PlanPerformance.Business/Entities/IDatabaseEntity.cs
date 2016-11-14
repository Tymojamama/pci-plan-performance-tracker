using System;

namespace PlanPerformance.Business.Entities
{
    /// <summary>
    /// Represents a public contract for interacting with <see cref="DatabaseEntity"/> objects.
    /// </summary>
    public interface IDatabaseEntity
    {
        /// <summary>
        /// Saves the record to the database. If the record already exists, it is updated,
        /// and if the record does not exist, it is updated.
        /// </summary>
        /// <param name="modifiedBy">Used to set the CreatedBy and ModifiedBy record values.</param>
        void SaveRecordToDatabase(Guid modifiedBy);

        /// <summary>
        /// Deletes the record from the database.
        /// </summary>
        void DeleteRecordFromDatabase();

        /// <summary>
        /// Resets the values of all public members to their values in the database.
        /// </summary>
        void RefreshMembers();
    }
}
