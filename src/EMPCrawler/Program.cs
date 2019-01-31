using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;

namespace EMPCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Client("", "");
            var loginTask = client.GetWishList();
            loginTask.Wait();
        }
    }
}
