using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    class CustomerData<T>
    {
        //litedb必须带id
        public int Id { get; set; }
        public string Key { get; set; }
        public T Data { get; set; }
    }

    public abstract class CustomerDataSave
    {
        private const string DATABASENAME = ".userSettings.db";

        public void SetCustomerData<T>(string key, T value)
        {
            using (var db = new LiteDatabase(DATABASENAME))
            {
                var col = db.GetCollection<CustomerData<T>>(GetCustomerTableName<T>());
                CustomerData<T> customerData;
                var query = col.Query().Where(x => x.Key.Equals(key));
                if (query.Count() > 0)
                {
                    customerData = query.First();
                    customerData.Data = value;
                    col.Update(customerData);
                }
                else
                {
                    customerData = new CustomerData<T>()
                    {
                        Key = key,
                        Data = value
                    };
                    col.Insert(customerData);
                }
            }
        }
        public T GetCustomerData<T>(string key, T defaultValue = default(T))
        {
            using (var db = new LiteDatabase(DATABASENAME))
            {
                var col = db.GetCollection<CustomerData<T>>(GetCustomerTableName<T>());
                var value = col.Query().Where(x => x.Key.Equals(key));
                if (value.Count() > 0)
                {
                    return value.First().Data;
                }

                return defaultValue;
            }
        }
        protected string GetCustomerTableName<T>()
        {
            string tableName= Regex.Replace(typeof(T).Name, @"[^a-zA-Z0-9\u4e00-\u9fa5\s]", "");
            tableName = $"CustomerData_{tableName}";
            return tableName;
        }
    }
}
