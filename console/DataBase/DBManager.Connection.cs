using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace console.DataBase
{
    public partial class DBManager
    {
        private const string configFileName = "config.ini";
        private const string section = "DATABASE";
        private const string keyHost = "host";
        private const string keyPort = "port";
        private const string keySid = "sid";
        private const string keyId = "id";
        private const string keyPassword = "password";

        private string InitFileName { get; set; }

        private string ConnString { get; set; }

        private OracleConnection Connection { get; set; }

        /// <summary>
        /// Read Database section from config.ini file.
        /// <para>
        /// set properties below
        /// </para>
        /// </summary>
        private void InitProperties()
        {
            InitFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFileName);
            INIReader reader = new INIReader(InitFileName);

            // Read id and password from initialization file. 
            string host = reader.IniReadValue(section, keyHost);
            string port = reader.IniReadValue(section, keyPort);
            string sid = reader.IniReadValue(section, keySid);
            string id = reader.IniReadValue(section, keyId);
            string password = reader.IniReadValue(section, keyPassword);
            this.ConnString = $"Data Source={host}:{port}/{sid};User Id={id};Password={password}";
        }

        /// <summary>
        /// Open Connection to Oracle Database.
        /// </summary>
        /// <returns>Oracle Connection object.</returns>
        private void OracleConnectionOpen()
        {
            try
            {
                this.Connection = new OracleConnection(this.ConnString);
                this.Connection.Open();
            }
            catch (OracleException e)
            {
                if (this.Connection != null)
                {
                    this.Connection.Close();
                }
                throw e;
            }
        }

        /// <summary>
        /// Close Connection with Oracle Database.
        /// </summary>
        private void OracleConnectionClose()
        {
            try
            {
                this.Connection.Close();
            }
            catch (OracleException e)
            {
                this.Connection = null;
            }
        }
    }
}
