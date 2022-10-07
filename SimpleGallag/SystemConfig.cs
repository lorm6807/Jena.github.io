using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGallag
{
    public class SystemConfig : ViewModelBase
    {
        protected static SystemConfig _instance;
        public static SystemConfig Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Activator.CreateInstance<SystemConfig>();

                return _instance;
            }
        }

        private static string configPath { get; set; } = string.Empty;
        public static string ConfigPath
        {
            get
            {
                if (string.IsNullOrEmpty(configPath))
                {
                    var curDir = Directory.GetCurrentDirectory();
                    configPath = $@"{Directory.GetParent(curDir)}\Config";
                }

                return configPath;
            }
            set
            {
                configPath = value;
            }
        }

        public static string ConfigName { get; set; } = string.Format("{0}.Json", typeof(SystemConfig).Name);

        public 

        public static void Save()
        {
            //string data = Json.Stringify(_instance);
            //if (!Directory.Exists(ConfigPath))
            //    Directory.CreateDirectory(ConfigPath);

            //File.WriteAllText(Path.Combine(ConfigPath, ConfigName), data);
        }
    }
}
