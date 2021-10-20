using ImGuiNET;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace HoneyBee.Diff.Gui
{
    class CustomerData<T>
    {
        public string Key { get; set; }
        public T Data { get; set; }
    }

    [Export(typeof(IUserSettingsModel))]
    public class UserSettingsModel : IUserSettingsModel
    {
        private const string DATABASENAME = ".userSettings.db";

        private int _styleColors = -1;
        public int StyleColors 
        {
            get
            {
                if (_styleColors == -1)
                {
                    _styleColors = GetCustomerData<int>("StyleColors", 0);
                }
                return _styleColors;
            }
            set
            {
                _styleColors = value;
                SetCustomerData<int>("StyleColors", value);
            }
        }

        public uint MarkBgColor
        {
            get
            {
                uint markBgColor = 0;
                //lightģʽ
                if (_styleColors == 0)
                {
                    markBgColor = ImGui.GetColorU32(new Vector4(1, 0.8f, 0.8f, 1));
                }
                else
                {
                    markBgColor = ImGui.GetColorU32(new Vector4(0.8f, 0.2f, 0.2f, 1));
                }
                return markBgColor;
            }
        }
        public Vector4 MarkRedColor
        {
            get
            {
                //lightģʽ
                if (_styleColors == 0)
                {
                    return new Vector4(0.8f, 0.2f, 0.2f, 1);
                }
                else
                {
                     return new Vector4(1, 0.8f, 0.8f, 1);
                }
            }
        }

        public Vector4 MarkGreenColor
        {
            get
            {
                //lightģʽ
                if (_styleColors == 0)
                {
                    return new Vector4(0.2f, 0.8f, 0.2f, 1);
                }
                else
                {
                    return new Vector4(0.8f, 1.0f, 0.8f, 1);
                }
            }
        }

   
        public void SetCustomerData<T>(string key, T value)
        {
            using (var db = new LiteDatabase(DATABASENAME))
            {
                string tableName = $"CustomerData_{typeof(T).Name}";
                var col = db.GetCollection<CustomerData<T>>(tableName);
                CustomerData<T> customerData = new CustomerData<T>()
                {
                    Key = key,
                    Data = value
                };
                col.Insert(customerData);
            }
        }

        public T GetCustomerData<T>(string key,T defaultValue = default(T))
        {
            using (var db = new LiteDatabase(DATABASENAME))
            {
                var col = db.GetCollection<CustomerData<T>>(typeof(CustomerData<T>).FullName);
                var value = col.Query().Where(x => x.Key.Equals(key));
                if (value.Count() > 0)
                {
                    return value.First().Data;
                }
                return defaultValue;
            }
        }


    }
}
