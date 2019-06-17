using System;
using System.Collections.Generic;
using System.Text;

namespace EMPCrawler.Model
{
    public class Config
    {
        public string Mailaddress { get; set; }
        public string Password { get; set; }

        public TelegramConfig Telegram { get; set; }
        public class TelegramConfig
        {
            public string Apikey { get; set; }
            public string ChatId { get; set; }
        }
    }
}
