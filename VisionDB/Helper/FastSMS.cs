using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace VisionDB.Helper
{
    public class FastSMS
    {
        internal string SendText(string SMSNumber, string Message, string Sender)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://my.fastsms.co.uk");

                var values = new List<KeyValuePair<string, string>>();
                values.Add(new KeyValuePair<string, string>("Username", "FS5019")); //todo: move to config
                values.Add(new KeyValuePair<string, string>("Password", "K2Q15xL9")); //todo: move to config
                values.Add(new KeyValuePair<string, string>("Action", "Send"));
                values.Add(new KeyValuePair<string, string>("DestinationAddress", SMSNumber));
                values.Add(new KeyValuePair<string, string>("SourceAddress", Sender));
                values.Add(new KeyValuePair<string, string>("Body", Message));

                var content = new FormUrlEncodedContent(values);

                var result = client.PostAsync("/api", content).Result;

                string resultContent = result.Content.ReadAsStringAsync().Result; //message ID if successful or error code if failure

                return resultContent;
            }
        }
    }
}