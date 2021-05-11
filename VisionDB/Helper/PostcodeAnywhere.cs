using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using VisionDB.Models;

namespace VisionDB.Helper
{
    public class PostcodeAnywhere
    {
        /// <summary>
        /// Return list of addresses that match
        /// </summary>
        /// <param name="SearchTerm">Postcode</param>
        /// <returns></returns>
        internal List<AddressLookupSummary> GetAddresses(string SearchTerm)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://services.postcodeanywhere.co.uk");

                var values = new List<KeyValuePair<string, string>>();
                values.Add(new KeyValuePair<string, string>("Key", "BN41-AA23-NU47-XA59")); //todo: move to config
                values.Add(new KeyValuePair<string, string>("SearchTerm", SearchTerm));

                var content = new FormUrlEncodedContent(values);

                var result = client.PostAsync("/CapturePlus/Interactive/Find/v2.10/xmle.ws", content).Result;

                string xml = result.Content.ReadAsStringAsync().Result; 

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(xml);

                XmlNodeList list = xmldoc.GetElementsByTagName("Row");

                List<AddressLookupSummary> addresses = new List<AddressLookupSummary>();

                foreach (XmlNode node in list)
                {
                    addresses.Add(new AddressLookupSummary { Id = node.ChildNodes.Item(0).InnerText, Text = node.ChildNodes.Item(1).InnerText });
                }

                return addresses;
            }
        }
    }
}