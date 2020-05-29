using console.DataBase.Schema;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace console.DataBase
{
    public partial class DBManager
    {
        /// <summary>
        /// database lock object
        /// </summary>
        private static readonly object dbLock = new object();

        /// <summary>
        /// real instance of this.
        /// </summary>
        private static DBManager instance = null;

        /// <summary>
        /// Instance Property
        /// </summary>
        public static DBManager Instance
        {
            get
            {
                lock (dbLock)
                {
                    if (instance == null)
                    {
                        instance = new DBManager();
                    }
                    return instance;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private DBManager()
        {
            this.InitProperties();
            // test for connection.
            this.OracleConnectionOpen();
            this.OracleConnectionClose();
        }

        /// <summary>
        /// Convert Field Type to Oracle DB type.
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        private OracleDbType ConvertToOracleDBType(FieldInfo fieldInfo)
        {
            Type type = fieldInfo.FieldType;
            if (type == typeof(Int32))
            {
                return OracleDbType.Int32;
            }
            else if (type == typeof(string))
            {
                return OracleDbType.NChar;
            }
            else if (type == typeof(bool))
            {
                return OracleDbType.NChar;
            }
            else if (type == typeof(DateTime))
            {
                return OracleDbType.Date;
            }
            else
            {
                throw new Exception("Oracle DB Type convert error.");
            }
        }

        /// <summary>
        /// Get Table of Attribute as Schema.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        private DBSchema GetTableSchema<T>(T table)
        {
            try
            {
                return table.GetType().GetCustomAttribute<DBSchema>();
            }
            catch (Exception e)
            {
                // Throw not nullable object about dbschema.
                throw e;
            }
        }

        /// <summary>
        /// Get value match with field name of instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private object GetFieldValue<T>(T table, string fieldName)
        {
            try
            {
                return this.GetFieldInfo(table, fieldName).GetValue(table);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Get FieldInfo math with field name of instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private FieldInfo GetFieldInfo<T>(T table, string fieldName)
        {
            try
            {
                return table.GetType().GetField(fieldName);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Convert Oracle Read data to DataTable
        /// </summary>
        /// <param name="command"></param>
        /// <returns>return first table</returns>
        private DataTable ReadToDataTable(OracleCommand command)
        {
            DataSet dataSet = new DataSet();
            OracleDataAdapter oda = new OracleDataAdapter();
            oda.SelectCommand = command;
            oda.Fill(dataSet);

            return dataSet.Tables[0];
        }

        private ObservableCollection<T> ToList<T>(OracleCommand command, T table)
        {
            // TODO 

            DataSet ds = new DataSet();
            OracleDataAdapter oda = new OracleDataAdapter();
            oda.SelectCommand = command;
            oda.Fill(ds);

            DataTable dt = ds.Tables[0];
            ObservableCollection<T> list = new ObservableCollection<T>();

            Type genericType = table.GetType();
            PropertyInfo[] properties = table.GetType().GetProperties();

            foreach (DataRow row in dt.Rows)
            {
                T temp = Activator.CreateInstance<T>();

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    //object target = temp.GetType().GetProperty(dt.Columns[i].ColumnName);
                    //MethodInfo method = temp.GetType().GetProperty("").SetMethod;
                    //method.Invoke()
                    temp.GetType().GetProperty(dt.Columns[i].ColumnName).SetValue(null, row.ItemArray[i]);
                }
                list.Add(temp);
            }

            //TODO
            return list;
        }

        /// <summary>
        /// Commit operation
        /// </summary>
        public void Commit()
        {
            throw new NotImplementedException();

            this.OracleConnectionOpen();
            OracleCommand command = this.Connection.CreateCommand();
            OracleTransaction transaction;

            transaction = Connection.BeginTransaction(IsolationLevel.ReadCommitted);
            transaction.Commit(); // Rollback.
            this.OracleConnectionClose();
        }

        /// <summary>
        /// Rollback operation
        /// </summary>
        public void Rollback()
        {
            throw new NotImplementedException();

            this.OracleConnectionOpen();
            OracleCommand command = this.Connection.CreateCommand();
            OracleTransaction transaction;

            transaction = Connection.BeginTransaction(IsolationLevel.ReadCommitted);
            transaction.Rollback();
            this.OracleConnectionClose();
        }
    }
}
