using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace console.DataBase.Schema
{
    public class DBSchema : Attribute
    {
        public DBSchema(string tableName, string primaryKey, string foreignKey, params string[] fieldNames)
        {
            this.TableName = tableName;
            this.PrimaryKey = primaryKey;
            this.ForeignKey = foreignKey;
            this.Fields = new List<string>();
            foreach (string field in fieldNames)
            {
                this.Fields.Add(field);
            }
        }

        /// <summary>
        /// Table Name
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Primary Key -> not null.
        /// </summary>
        public string PrimaryKey { get; set; }

        /// <summary>
        /// Foreign Key -> nullable.
        /// </summary>
        public string ForeignKey { get; set; }

        /// <summary>
        /// All of column names (fields)
        /// </summary>
        private List<string> Fields { get; set; }

        public List<string> GetFields(bool isContainPK = true, bool isContainFK = true)
        {
            List<string> fields = new List<string>();
            foreach (string field in this.Fields)
            {
                fields.Add(field);
            }

            if (!isContainPK & !PrimaryKey.Equals(string.Empty))
            {
                fields.Remove(this.PrimaryKey);
            }

            if (!isContainFK & !ForeignKey.Equals(string.Empty))
            {
                fields.Remove(this.ForeignKey);
            }

            return fields;
        }
    }
}
