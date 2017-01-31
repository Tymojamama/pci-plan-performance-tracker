using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PensionConsultants.Data.Utilities;
using PlanPerformance.Business.Components;

namespace PlanPerformance.Business.Entities
{
    /// <summary>
    /// Represents an abstract object that can be used for all database records.
    /// </summary>
    public abstract class DatabaseEntity : IDatabaseEntity
    {
        /// <summary>
        /// Indicates whether the instance exists as a record in the database.
        /// </summary>
        public bool ExistingRecord { get; private set; }

        /// <summary>
        /// Represents when the record was inserted into the database.
        /// </summary>
        /// <remarks>
        /// This really shouldn't be nullable, but FundDetails has
        /// a bunch of null data and I haven't got around to
        /// fixing it. Plz fx soon.
        /// </remarks>
        public DateTime? CreatedOn { get; private set; }

        /// <summary>
        /// Represents when the record was last modified in the database.
        /// </summary>
        /// <remarks>
        /// This really shouldn't be nullable, but FundDetails has
        /// a bunch of null data and I haven't got around to
        /// fixing it. Plz fx soon.
        /// </remarks>
        public DateTime? ModifiedOn { get; private set; }

        /// <summary>
        /// Reflects the DateTime that the members were set from the database. Used to control
        /// concurrency.
        /// </summary>
        public DateTime? EntityMembersAsOf { get; private set; }

        /// <summary>
        /// Represents which UserId created the record in the database.
        /// </summary>
        /// <remarks>
        /// This really shouldn't be nullable, but FundDetails has
        /// a bunch of null data and I haven't got around to
        /// fixing it. Plz fx soon.
        /// </remarks>
        public Guid? CreatedBy { get; private set; }

        /// <summary>
        /// Represents the primary key of the record in the database.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Represents which UserId last modified the record in the database.
        /// </summary>
        /// <remarks>
        /// This really shouldn't be nullable, but FundDetails has
        /// a bunch of null data and I haven't got around to
        /// fixing it. Plz fx soon.
        /// </remarks>
        public Guid? ModifiedBy { get; private set; }

        /// <summary>
        /// Represents the state of the record. 0 is Active and 1 is Inactive.
        /// </summary>
        public int StateCode { get; set; }

        /// <summary>
        /// Represents the name of the SQL table that the object is modeling.
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>
        /// A list of members that exist in the database;
        /// </summary>
        public List<ColumnTuple> RegisteredColumns;

        /// <summary>
        /// Interacts with the database on the entity's behalf.
        /// </summary>
        private SqlCrudOperator SqlCrud;

        /// <summary>
        /// Creates an instance of a DatabaseEntity object that does not exist in the database.
        /// </summary>
        /// <param name="tableName">Used to identify with which table the instance is interacting.</param>
        public DatabaseEntity(string tableName)
        {
            ExistingRecord = false;
            Id = Guid.NewGuid();
            TableName = tableName;
            EntityMembersAsOf = null;

            SqlCrud = new SqlCrudOperator(Access.DbAccess, TableName);
            RegisteredColumns = new List<ColumnTuple>();

            RegisterMembers();
        }

        /// <summary>
        /// Creates an instance of an existing DatabaseEntity object that does exist as a database record.
        /// </summary>
        /// <param name="tableName">Used to identify with which table the instance is interacting.</param>
        /// <param name="id">Used to identify with which record the instance is interacting.</param>
        public DatabaseEntity(string tableName, Guid id)
        {
            ExistingRecord = true;
            Id = id;
            TableName = tableName;

            SqlCrud = new SqlCrudOperator(Access.DbAccess, TableName);
            RegisteredColumns = new List<ColumnTuple>();

            RegisterMembers();
            RefreshMembers();
            SetRegisteredMembers();
        }

        /// <summary>
        /// Saves the record to the database. If the record already exists, it is updated,
        /// and if the record does not exist, it is inserted.
        /// </summary>
        /// <param name="modifiedBy">Used to set the CreatedBy and ModifiedBy record values.</param>
        public void SaveRecordToDatabase(Guid modifiedBy)
        {
            if (!ExistingRecord)
            {
                this.Insert(modifiedBy);
                ExistingRecord = true;
            }
            else
            {
                CheckForConcurrencyException();
                this.Update(modifiedBy);
            }

            this.RefreshMembers();
        }

        /// <summary>
        /// Deletes the record from the database.
        /// </summary>
        public void DeleteRecordFromDatabase()
        {
            SqlCrud.Delete(this.Id);
            ExistingRecord = false;
        }

        /// <summary>
        /// Resets the values of all public members to their values in the database.
        /// </summary>
        public void RefreshMembers()
        {
            if (this.ExistingRecord == false)
            {
                return;
            }

            DataTable dataTable = this.GetDetails();
            DataRow dataRow;

            if (dataTable.Rows.Count == 0)
            {
                Console.WriteLine(this.TableName + " with id " + this.Id.ToString() + " does not exist");
                return;
            }
            else
            {
                dataRow = dataTable.Rows[0];
            }

            DateTime? createdOn;
            DateTime? modifiedOn;
            Guid? createdBy;
            Guid? modifiedBy;
            int stateCode;

            foreach (ColumnTuple columnTuple in RegisteredColumns)
            {
                StringParser.Parse(dataRow[columnTuple.Name].ToString(), columnTuple.ValueType, out columnTuple.Value);
            }

            StringParser.Parse(dataRow["CreatedOn"].ToString(), out createdOn);
            StringParser.Parse(dataRow["ModifiedOn"].ToString(), out modifiedOn);
            StringParser.Parse(dataRow["CreatedBy"].ToString(), out createdBy);
            StringParser.Parse(dataRow["ModifiedBy"].ToString(), out modifiedBy);
            StringParser.Parse(dataRow["StateCode"].ToString(), out stateCode);

            this.CreatedOn = createdOn;
            this.ModifiedOn = modifiedOn;
            this.CreatedBy = createdBy;
            this.ModifiedBy = modifiedBy;
            this.StateCode = stateCode;

            this.EntityMembersAsOf = DateTime.Now;
        }

        /// <summary>
        /// Represents the existence of a concurrency error.
        /// </summary>
        /// <returns>true if there is a concurrency error. false if there is not a concurrency error.</returns>
        /// <remarks>
        /// <para>
        /// It turns out that it's fairly easy to by-pass this if you're doing weird stuff in the UI. For example,
        /// if you create a new instance from the Id, set the values, and save, no concurrency error will raise.
        /// See below for an example of this.
        /// </para>
        /// <code>
        /// public class SomeForm
        /// {
        ///     SomeEntity initialEntity;
        ///     Guid UserId;
        ///     
        ///     public SomeForm(Guid initialEntityId, Guid userId)
        ///     {
        ///         initialEntity = new SomeEntity(initialEntityId);
        ///         UserId = userId;
        ///         
        ///         txtName.Text = "Bill";
        /// 
        ///         SaveValues();
        ///     }
        ///     
        ///     public void SaveValues()
        ///     {
        ///         SomeEntity entity = new SomeEntity(initalEntity.Id);
        ///         entity.Name = txtName.Text;
        ///         entity.SaveRecordToDatabase(this.UserId);
        ///     }
        /// }
        /// </code>
        /// <para>
        /// In order to keep concurrency relevant, you must not create a new instance
        /// of the entity before saving. See below for a correct way to do this.
        /// </para>
        /// <code>
        /// public class SomeForm
        /// {
        ///     SomeEntity initialEntity;
        ///     Guid UserId;
        ///     
        ///     public SomeForm(Guid initialEntityId, Guid userId)
        ///     {
        ///         initialEntity = new SomeEntity(initialEntityId);
        ///         UserId = userId;
        ///         
        ///         txtName.Text = "Bill";
        /// 
        ///         SaveValues();
        ///     }
        ///     
        ///     public void SaveValues()
        ///     {
        ///         initialEntity.Name = txtName.Text;
        ///         initialEntity.SaveRecordToDatabase(this.UserId);
        ///     }
        /// }
        /// </code>
        /// </remarks>
        public bool IsConcurrencyError()
        {
            if (this.ExistingRecord)
            {
                DataTable dataTable = this.GetDetails();
                DataRow dataRow = dataTable.Rows[0];

                DateTime modifiedOn;
                StringParser.Parse(dataRow["ModifiedOn"].ToString(), out modifiedOn);

                if (modifiedOn > (DateTime)this.EntityMembersAsOf)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Throws an exception if there is a concurrency error.
        /// </summary>
        private void CheckForConcurrencyException()
        {
            if (IsConcurrencyError())
            {
                string message = "This record has been modified by someone else, and some data may no longer be correct. You must re-open the entity in order to save.";
                throw new DBConcurrencyException(message);
            }
        }

        /// <summary>
        /// Creates a <see cref="DataTable"/> of all data for the record.
        /// </summary>
        /// <returns>A <see cref="DataTable"/> of all data for record.</returns>
        private DataTable GetDetails()
        {
            DataTable dataTable = new DataTable();
            SqlCrud.Read(this.Id, out dataTable);
            return dataTable;
        }

        /// <summary>
        /// Updates the record in the database with this instance's data.
        /// </summary>
        /// <param name="modifiedBy">Used to set the ModifiedBy value of the record.</param>
        private void Update(Guid modifiedBy)
        {
            SqlCrud.ClearColumns();
            RegisteredColumns.Clear();
            RegisterMembers();

            foreach (ColumnTuple columnTuple in RegisteredColumns)
            {
                SqlCrud.AddColumn(columnTuple.Name, columnTuple.Value);
            }

            SqlCrud.AddColumn("ModifiedBy", modifiedBy);
            SqlCrud.AddColumn("ModifiedOn", DateTime.Now);
            SqlCrud.AddColumn("StateCode", this.StateCode);
            SqlCrud.Update(this.Id);
        }

        /// <summary>
        /// Inserts this instance's data into the database as a new record.
        /// </summary>
        /// <param name="createdBy">Used to set the CreatedBy value of the record.</param>
        private void Insert(Guid createdBy)
        {
            SqlCrud.ClearColumns();
            RegisteredColumns.Clear();
            RegisterMembers();

            foreach (ColumnTuple columnTuple in RegisteredColumns)
            {
                SqlCrud.AddColumn(columnTuple.Name, columnTuple.Value);
            }

            SqlCrud.AddColumn("ModifiedBy", createdBy);
            SqlCrud.AddColumn("ModifiedOn", DateTime.Now);
            SqlCrud.AddColumn("CreatedBy", createdBy);
            SqlCrud.AddColumn("CreatedOn", DateTime.Now);
            SqlCrud.AddColumn("StateCode", this.StateCode);
            SqlCrud.Create(this.Id);
        }

        /// <summary>
        /// Registers the instance's members with the abstract class in order to perform database operations. Do not register members
        /// that exist within the abstract class (e.g. CreatedOn).
        /// </summary>
        protected abstract void RegisterMembers();

        /// <summary>
        /// Resets the values of all public members to their values in the database.
        /// </summary>
        protected abstract void SetRegisteredMembers();

        /// <summary>
        /// Gets the value of a column with the given Column Name.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        protected object GetColumn(string columnName)
        {
            foreach (ColumnTuple columnTuple in RegisteredColumns)
            {
                if (columnTuple.Name == columnName)
                {
                    return columnTuple.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Adds a column with the given column name and value pair.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="columnValue"></param>
        protected void AddColumn(string columnName, string columnValue)
        {
            ColumnTuple columnTuple = new ColumnTuple();
            columnTuple.Name = columnName;
            columnTuple.ValueType = typeof(string);
            columnTuple.Value = columnValue;
            RegisteredColumns.Add(columnTuple);
        }

        /// <summary>
        /// Adds a column with the given column name and value pair.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="columnValue"></param>
        protected void AddColumn(string columnName, DateTime columnValue)
        {
            ColumnTuple columnTuple = new ColumnTuple();
            columnTuple.Name = columnName;
            columnTuple.ValueType = typeof(DateTime);
            columnTuple.Value = columnValue;
            RegisteredColumns.Add(columnTuple);
        }

        /// <summary>
        /// Adds a column with the given column name and value pair.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="columnValue"></param>
        protected void AddColumn(string columnName, DateTime? columnValue)
        {
            ColumnTuple columnTuple = new ColumnTuple();
            columnTuple.Name = columnName;
            columnTuple.ValueType = typeof(DateTime?);
            columnTuple.Value = columnValue;
            RegisteredColumns.Add(columnTuple);
        }

        /// <summary>
        /// Adds a column with the given column name and value pair.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="columnValue"></param>
        protected void AddColumn(string columnName, Guid columnValue)
        {
            ColumnTuple columnTuple = new ColumnTuple();
            columnTuple.Name = columnName;
            columnTuple.ValueType = typeof(Guid);
            columnTuple.Value = columnValue;
            RegisteredColumns.Add(columnTuple);
        }

        /// <summary>
        /// Adds a column with the given column name and value pair.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="columnValue"></param>
        protected void AddColumn(string columnName, Guid? columnValue)
        {
            ColumnTuple columnTuple = new ColumnTuple();
            columnTuple.Name = columnName;
            columnTuple.ValueType = typeof(Guid?);
            columnTuple.Value = columnValue;
            RegisteredColumns.Add(columnTuple);
        }

        /// <summary>
        /// Adds a column with the given column name and value pair.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="columnValue"></param>
        protected void AddColumn(string columnName, SqlBoolean columnValue)
        {
            ColumnTuple columnTuple = new ColumnTuple();
            columnTuple.Name = columnName;
            columnTuple.ValueType = typeof(SqlBoolean);
            columnTuple.Value = columnValue;
            RegisteredColumns.Add(columnTuple);
        }

        /// <summary>
        /// Adds a column with the given column name and value pair.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="columnValue"></param>
        protected void AddColumn(string columnName, SqlBoolean? columnValue)
        {
            ColumnTuple columnTuple = new ColumnTuple();
            columnTuple.Name = columnName;
            columnTuple.ValueType = typeof(SqlBoolean?);
            columnTuple.Value = columnValue;
            RegisteredColumns.Add(columnTuple);
        }

        /// <summary>
        /// Adds a column with the given column name and value pair.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="columnValue"></param>
        protected void AddColumn(string columnName, int columnValue)
        {
            ColumnTuple columnTuple = new ColumnTuple();
            columnTuple.Name = columnName;
            columnTuple.ValueType = typeof(int);
            columnTuple.Value = columnValue;
            RegisteredColumns.Add(columnTuple);
        }

        /// <summary>
        /// Adds a column with the given column name and value pair.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="columnValue"></param>
        protected void AddColumn(string columnName, int? columnValue)
        {
            ColumnTuple columnTuple = new ColumnTuple();
            columnTuple.Name = columnName;
            columnTuple.ValueType = typeof(int?);
            columnTuple.Value = columnValue;
            RegisteredColumns.Add(columnTuple);
        }

        /// <summary>
        /// Adds a column with the given column name and value pair.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="columnValue"></param>
        protected void AddColumn(string columnName, decimal columnValue)
        {
            ColumnTuple columnTuple = new ColumnTuple();
            columnTuple.Name = columnName;
            columnTuple.ValueType = typeof(decimal);
            columnTuple.Value = columnValue;
            RegisteredColumns.Add(columnTuple);
        }

        /// <summary>
        /// Adds a column with the given column name and value pair.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="columnValue"></param>
        protected void AddColumn(string columnName, decimal? columnValue)
        {
            ColumnTuple columnTuple = new ColumnTuple();
            columnTuple.Name = columnName;
            columnTuple.ValueType = typeof(decimal?);
            columnTuple.Value = columnValue;
            RegisteredColumns.Add(columnTuple);
        }
    }
}
