using console.DataBase.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace console.ViewModel
{
    public class DroneViewModel : CViewModel<Drone>
    {
        public int Id
        {
            get => Model.Id;
            set => SetValue<int>(ref Model.Id, value);
        }

        public string Name
        {
            get => Model.Name;
            set => SetValue<string>(ref Model.Name, value);
        }

        public override string ToString()
        {
            return $"{Id}, {Name}";
        }
    }
}
