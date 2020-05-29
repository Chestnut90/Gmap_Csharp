using console.DataBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace console.ViewModel
{
    public class CObservableCollection<ViewModel> : ObservableCollection<ViewModel>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="load"></param>
        /// <param name="isChanged"></param>
        public CObservableCollection(bool load=true, bool isChanged=true) :base()
        {
            if (load)
            {
                foreach(ViewModel item in this.LoadFromDataBase())
                {
                    this.Add(item);
                }
            }
            if (isChanged)
            {
                this.CollectionChanged += CObservableCollection_CollectionChanged;
            }
        }

        /// <summary>
        /// Occured when collection was changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CObservableCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action.ToString())
            {
                case "Add":

                    this.OnAddAction(e);

                    break;
                case "Replace":

                    this.OnReplaceAction(e);

                    break;
                case "Remove":

                    this.OnRemoveAction(e);

                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Add 
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnAddAction(NotifyCollectionChangedEventArgs e)
        {
            ViewModel newItem = (ViewModel)e.NewItems[0];
            DBManager.Instance.Insert(this.GetModelInstance(newItem));
        }

        /// <summary>
        /// Replace
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnReplaceAction(NotifyCollectionChangedEventArgs e)
        {
            ViewModel replaceItem = (ViewModel)e.NewItems[0];
            DBManager.Instance.Update(this.GetModelInstance(replaceItem));
        }

        /// <summary>
        /// Remove
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnRemoveAction(NotifyCollectionChangedEventArgs e)
        {
            ViewModel removeItem = (ViewModel)e.OldItems[0];
            DBManager.Instance.Delete(this.GetModelInstance(removeItem));
        }

        /// <summary>
        /// Replace target ViewModel with selected index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="target"></param>
        public void Replace(int index, ViewModel target)
        {
            this.SetItem(index, target);
        }

        /// <summary>
        /// Make empty ViewModel Instance and return.
        /// </summary>
        /// <returns></returns>
        private ViewModel GetViewModelInstance()
        {
            try
            {
                return Activator.CreateInstance<ViewModel>();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Get empty Model Instance
        /// </summary>
        /// <returns></returns>
        private object GetModelInstance()
        {
            try
            {
                ViewModel viewModel = this.GetViewModelInstance();
                object model = viewModel.GetType().GetMethod("GetModel").Invoke(viewModel, null);
                return model;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Get specific Model Instance as ViewModel
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        private object GetModelInstance(ViewModel viewModel)
        {
            try
            {
                return viewModel.GetType().GetMethod("GetModel").Invoke(viewModel, null);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Make New Instance with data.
        /// </summary>
        /// <param name="itemSource"></param>
        /// <returns></returns>
        private ViewModel CreateViewModel(object[] itemSource)
        {
            try
            {
                ViewModel viewModel = this.GetViewModelInstance();
                var method = viewModel.GetType().GetMethod("SetModel");
                viewModel.GetType().GetMethod("SetModel").Invoke(viewModel, new object[] { itemSource });
                return viewModel;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Load DB datas.
        /// </summary>
        /// <returns></returns>
        private CObservableCollection<ViewModel> LoadFromDataBase()
        {
            CObservableCollection<ViewModel> collection = new CObservableCollection<ViewModel>(false, false);
            DataTable dt = DBManager.Instance.Select(this.GetModelInstance());

            foreach (DataRow row in dt.Rows)
            {
                collection.Add(this.CreateViewModel(row.ItemArray));
            }

            return collection;
        }
    }
}
