using console.DataBase.Schema;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace console.DataBase
{
    public partial class DBManager
    {
        /// <summary>
        /// Select table with column names.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public DataTable Select<T>(T table, params string[] columns)
        {
            try
            {
                DBSchema schema = this.GetTableSchema(table);

                StringBuilder selectQuery = new StringBuilder();
                selectQuery.Append($"select ");
                foreach (string column in columns)
                {
                    selectQuery.Append($"{column}, ");
                }
                selectQuery.Remove(selectQuery.Length - 2, 1);  // Remove last ",".
                selectQuery.Append($"from {schema.TableName}");

                this.OracleConnectionOpen();
                OracleCommand command = new OracleCommand(selectQuery.ToString(), this.Connection);

                return this.ReadToDataTable(command);
            }
            catch (OracleException e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Select * from TableName.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public DataTable Select<T>(T table)
        {
            try
            {
                DBSchema schema = this.GetTableSchema(table);

                StringBuilder selectQuery = new StringBuilder();

                selectQuery.Append($"select ");
                foreach (string field in schema.GetFields())
                {
                    selectQuery.Append($"{field}, ");
                }
                selectQuery.Remove(selectQuery.Length - 2, 1);
                selectQuery.Append($"from {schema.TableName}");

                this.OracleConnectionOpen();
                OracleCommand command = new OracleCommand(selectQuery.ToString(), this.Connection);

                return this.ReadToDataTable(command);
            }
            catch (OracleException e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Insert into values to table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public bool Insert<T>(T table, params object[] values)
        {
            try
            {
                DBSchema schema = this.GetTableSchema(table);

                // Make Query.
                StringBuilder insertQuery = new StringBuilder();
                insertQuery.Append($"insert into {schema.TableName} ");
                insertQuery.Append($"(");

                StringBuilder fieldQuery = new StringBuilder();
                StringBuilder valueQuery = new StringBuilder();
                foreach (string field in schema.GetFields())
                {
                    fieldQuery.Append($"{field}, ");
                    valueQuery.Append($":p{field}, ");
                }
                fieldQuery.Remove(fieldQuery.Length - 2, 1); // remove last ","
                valueQuery.Remove(valueQuery.Length - 2, 1); // remove last ","

                insertQuery.Append(fieldQuery.ToString());
                insertQuery.Append(") ");

                insertQuery.Append($"values (");
                insertQuery.Append(valueQuery.ToString());
                insertQuery.Append(")");

                this.OracleConnectionOpen();
                // command setting.
                OracleCommand command = new OracleCommand(insertQuery.ToString(), this.Connection);
                foreach (var iter in schema.GetFields().Zip(values, Tuple.Create))
                {
                    OracleDbType type = this.ConvertToOracleDBType(GetFieldInfo(table, iter.Item1));
                    command.Parameters.Add($"p{iter.Item1}", type, iter.Item2, ParameterDirection.Input);
                }
                command.ExecuteNonQuery();
            }
            catch (OracleException e)
            {
                throw e;
            }
            return true;
        }

        /// <summary>
        /// Insert Row with instance.
        /// </summary>
        /// <typeparam name="T">Type of T (Class)</typeparam>
        /// <param name="table">Instance of T (Class)</param>
        /// <returns></returns>
        public bool Insert<T>(T table)
        {
            try
            {
                DBSchema schema = this.GetTableSchema(table);

                // Make Query.
                StringBuilder insertQuery = new StringBuilder();
                insertQuery.Append($"insert into {schema.TableName} ");
                insertQuery.Append($"(");

                StringBuilder fieldQuery = new StringBuilder();
                StringBuilder valueQuery = new StringBuilder();
                foreach (string field in schema.GetFields())
                {
                    fieldQuery.Append($"{field}, ");
                    valueQuery.Append($":p{field}, ");
                }
                fieldQuery.Remove(fieldQuery.Length - 2, 1); // remove last ","
                valueQuery.Remove(valueQuery.Length - 2, 1); // remove last ","

                insertQuery.Append(fieldQuery.ToString());
                insertQuery.Append(") ");

                insertQuery.Append($"values (");
                insertQuery.Append(valueQuery.ToString());
                insertQuery.Append(")");

                this.OracleConnectionOpen();
                // command setting.
                OracleCommand command = new OracleCommand(insertQuery.ToString(), this.Connection);
                foreach (string field in schema.GetFields())
                {
                    OracleDbType type = this.ConvertToOracleDBType(GetFieldInfo(table, field));
                    object value = this.GetFieldValue(table, field);
                    command.Parameters.Add($"p{field}", type, value, ParameterDirection.Input);
                }
                command.ExecuteNonQuery();
            }
            catch (OracleException e)
            {
                throw e;
            }
            return true;
        }

        /// <summary>
        /// Update row mathcing with primaryKeyValue in table using values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="primaryKeyValue"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public bool Update<T>(T table, object primaryKeyValue, params object[] values)
        {
            try
            {
                DBSchema schema = this.GetTableSchema(table);

                StringBuilder updateQuery = new StringBuilder();
                updateQuery.Append($"update {schema.TableName} ");
                updateQuery.Append($"set ");

                foreach (string fieldName in schema.GetFields(isContainPK: false))
                {
                    updateQuery.Append($"{fieldName}=:p{fieldName}, ");
                }

                updateQuery.Remove(updateQuery.Length - 2, 1); // remove last ","
                updateQuery.Append($"where {schema.PrimaryKey}={primaryKeyValue}");

                this.OracleConnectionOpen();
                OracleCommand command = new OracleCommand(updateQuery.ToString(), this.Connection);
                foreach (var iter in schema.GetFields(isContainPK: false).Zip(values, Tuple.Create))
                {
                    OracleDbType type = this.ConvertToOracleDBType(GetFieldInfo(table, iter.Item1));
                    command.Parameters.Add($"p{iter.Item1}", type, iter.Item2, ParameterDirection.Input);
                }
                command.ExecuteNonQuery();
            }
            catch (OracleException e)
            {
                throw e;
            }
            return true;
        }

        /// <summary>
        /// Update Table with instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public bool Update<T>(T table)
        {
            try
            {
                DBSchema schema = this.GetTableSchema(table);

                StringBuilder updateQuery = new StringBuilder();
                updateQuery.Append($"update {schema.TableName} ");
                updateQuery.Append($"set ");

                foreach (string fieldName in schema.GetFields(isContainPK: false))
                {
                    updateQuery.Append($"{fieldName}=:p{fieldName}, ");
                }

                updateQuery.Remove(updateQuery.Length - 2, 1); // remove last ","
                updateQuery.Append($"where {schema.PrimaryKey}=:p{schema.PrimaryKey}");

                this.OracleConnectionOpen();
                OracleCommand command = new OracleCommand(updateQuery.ToString(), this.Connection);
                foreach (string field in schema.GetFields(isContainPK: false))
                {
                    OracleDbType dbType = this.ConvertToOracleDBType(GetFieldInfo(table, field));
                    object inputValue = this.GetFieldValue(table, field);
                    command.Parameters.Add($"p{field}", dbType, inputValue, ParameterDirection.Input);
                }

                OracleDbType pkType = this.ConvertToOracleDBType(GetFieldInfo(table, schema.PrimaryKey));
                object pkValue = GetFieldValue(table, schema.PrimaryKey);
                command.Parameters.Add($"p{schema.PrimaryKey}", pkType, pkValue, ParameterDirection.Input);
                command.ExecuteNonQuery();
            }
            catch (OracleException e)
            {

                throw e;
            }
            return true;
        }

        /// <summary>
        /// Join two table with foreign keys.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="table1"></param>
        /// <param name="table2"></param>
        /// <param name="fk1"></param>
        /// <param name="fk2"></param>
        /// <returns></returns>
        public DataTable Join<T1, T2>(T1 table1, T2 table2, string fk1, string fk2)
        {
            try
            {
                DBSchema schema1 = this.GetTableSchema(table1);
                DBSchema schema2 = this.GetTableSchema(table2);

                StringBuilder joinQuery = new StringBuilder();
                joinQuery.Append($"select * ");
                joinQuery.Append($"from {schema1.TableName} a ");
                joinQuery.Append($"inner join {schema2.TableName} b ");
                joinQuery.Append($"on a.{fk1}=b.{fk2}");

                OracleCommand command = new OracleCommand(joinQuery.ToString(), this.Connection);
                return this.ReadToDataTable(command);
            }
            catch (OracleException e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Join two table table1 and table2 and map to table3 schema.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="table1"></param>
        /// <param name="table2"></param>
        /// <param name="table3"></param>
        /// <returns></returns>
        public DataTable Join<T1, T2, T3>(T1 table1, T2 table2, T3 table3)
        {
            try
            {
                DBSchema schema1 = this.GetTableSchema(table1);
                DBSchema schema2 = this.GetTableSchema(table2);
                DBSchema schema3 = this.GetTableSchema(table3);

                StringBuilder joinQuery = new StringBuilder();
                joinQuery.Append($"select ");

                foreach (string field in schema3.GetFields())
                {
                    if (schema1.GetFields().Contains(field))
                    {
                        joinQuery.Append($"a.{field} {field}, ");
                    }
                    else
                    {
                        joinQuery.Append($"b.{field} {field}, ");
                    }
                }
                joinQuery.Remove(joinQuery.Length - 2, 1); // remove last ","

                joinQuery.Append($"from {schema1.TableName} a ");
                joinQuery.Append($"inner join {schema2.TableName} b ");
                joinQuery.Append($"on a.{schema1.ForeignKey}=b.{schema2.ForeignKey}");

                OracleCommand command = new OracleCommand(joinQuery.ToString(), this.Connection);
                return this.ReadToDataTable(command);
            }
            catch (OracleException e)
            {

                throw e;
            }
        }

        /// <summary>
        /// Delete row matching with primaryKeyValue in table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="primaryKeyValue"></param>
        /// <returns></returns>
        public bool Delete<T>(T table, object primaryKeyValue)
        {
            try
            {
                DBSchema schema = this.GetTableSchema(table);

                StringBuilder deleteQuery = new StringBuilder();
                deleteQuery.Append($"delete from {schema.TableName} ");
                deleteQuery.Append($"where {schema.PrimaryKey}=:pkvalue");

                this.OracleConnectionOpen();
                OracleCommand command = new OracleCommand(deleteQuery.ToString(), this.Connection);
                OracleDbType type = this.ConvertToOracleDBType(GetFieldInfo(table, schema.PrimaryKey));
                command.Parameters.Add("pkvalue", type, primaryKeyValue, ParameterDirection.Input);
                command.ExecuteNonQuery();
            }
            catch (OracleException e)
            {
                throw e;
            }
            return true;
        }

        /// <summary>
        /// Delete on Instance in table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public bool Delete<T>(T table)
        {
            try
            {
                DBSchema schema = this.GetTableSchema(table);

                StringBuilder deleteQuery = new StringBuilder();
                deleteQuery.Append($"delete from {schema.TableName} ");
                deleteQuery.Append($"where {schema.PrimaryKey}=:pkvalue");

                this.OracleConnectionOpen();

                OracleCommand command = new OracleCommand(deleteQuery.ToString(), this.Connection);

                OracleDbType pkType = this.ConvertToOracleDBType(GetFieldInfo(table, schema.PrimaryKey));
                object pkValue = this.GetFieldValue(table, schema.PrimaryKey);
                command.Parameters.Add("pkvalue", pkType, pkValue, ParameterDirection.Input);

                command.ExecuteNonQuery();
            }
            catch (OracleException e)
            {
                throw e;
            }
            return true;
        }
    }
}
