using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EMPCrawler.Data
{
    public static class ConfigService
    {
        public static Model.Config Instance { get; set; }

        private const string _fileName = "config.json";

        public static bool LoadConfig()
        {
            if (File.Exists(_fileName))
            {
                var rawConfig = File.ReadAllText(_fileName);
                var config = JsonConvert.DeserializeObject<Model.Config>(rawConfig);

                if (config != null)
                {
                    Instance = config;
                    return true;
                }
            }
            else
            {
                var newConfig = JsonConvert.SerializeObject(new Model.Config() { Telegram = new Model.Config.TelegramConfig() }, Formatting.Indented);
                File.WriteAllText(_fileName, newConfig);
            }
            return false;
        }
    }
}
