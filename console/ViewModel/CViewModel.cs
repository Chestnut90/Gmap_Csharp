using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace console.ViewModel
{
    public class CViewModel<T> : ViewModelBase
    {
        public CViewModel()
        {
            this.Model = Activator.CreateInstance<T>();

        }

        /// <summary>
        /// Mapping data for model on DB.
        /// </summary>
        protected T Model { get; set; }

        /// <summary>
        /// return Model
        /// </summary>
        /// <returns></returns>
        public T GetModel()
        {
            return Model;
        }

        /// <summary>
        /// Set Raw data with items params.
        /// </summary>
        /// <param name="items"></param>
        public void SetModel(object[] items)
        {
            try
            {
                FieldInfo[] fields = Model.GetType().GetFields();
                object[] values = (object[])items;

                foreach (var zip in fields.Zip(values, Tuple.Create))
                {
                    Type valueType = zip.Item1.FieldType;
                    object con = Convert.ChangeType(zip.Item2, valueType);

                    zip.Item1.SetValue(Model, con);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
