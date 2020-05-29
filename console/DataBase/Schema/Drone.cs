using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace console.DataBase.Schema
{
    [DBSchema(tableName: "DRONE",
              primaryKey: "Id",
              foreignKey: "Name",
              fieldNames: new string[] { "Id", "Name" })]
    public class Drone
    {
        public int Id;
        public string Name;
    }
}
