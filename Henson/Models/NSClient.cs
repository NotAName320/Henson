using dotNS;
using dotNS.Classes;
using Henson.ViewModels;
using HtmlAgilityPack;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Henson.Models
{
    public class NSClient
    {
        public NSClient(string userAgent)
        {
            APIClient.UserAgent = Uri.EscapeDataString($"Henson v{GetType().Assembly.GetName().Version} developed by nation: Notanam in use by nation: {userAgent}");
        }

        public DotNS APIClient { get; } = new();
        public RestClient HttpClient = new("https://www.nationstates.net");

        private const int MultipleRequestsWaitTime = 600;

        private static string UserClick => DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

        public (List<Nation> nations, bool authFailedOnSome) AuthenticateAndReturnInfo(List<NationLoginViewModel> logins)
        {
            List<Nation> retVal = new();
            bool didAuthFail = false;
            foreach(NationLoginViewModel login in logins)
            {
                try
                {
                    if(APIClient.UpdatePin(login.Name, login.Pass))
                    {
                        string[] nationInfo = APIClient.PublicShard(login.Name, new Shards.PublicShard[] { Shards.PublicShard.Name, Shards.PublicShard.Flag, Shards.PublicShard.Region });
                        nationInfo[2] = char.ToUpper(nationInfo[2][0]) + nationInfo[2][1..]; //API doesn't capitalize region names
                        retVal.Add(new Nation(nationInfo[0], login.Pass, nationInfo[1], nationInfo[2]));
                        Thread.Sleep(MultipleRequestsWaitTime);
                    }
                    else
                    {
                        didAuthFail = true;
                    }
                }
                catch (Exception)
                {
                    didAuthFail = true;
                }
                Thread.Sleep(MultipleRequestsWaitTime); //avoid ratelimits
            }

            return (retVal, didAuthFail);
        }

        public bool Ping(NationLoginViewModel login)
        {
            NameValueCollection nvc = new()
            {
                { "nation", login.Name },
                { "q", "ping" }
            };

            return Utilities.API(nvc, login.Pass, 0, APIClient.UserAgent).IsSuccessStatusCode;
        }

        public List<bool> PingMany(List<NationLoginViewModel> logins)
        {
            List<bool> loginSuccesses = new();
            foreach(var n in logins)
            {
                loginSuccesses.Add(Ping(n));
                Thread.Sleep(MultipleRequestsWaitTime);
            }

            return loginSuccesses;
        }

        public string? FindWA(List<NationGridViewModel> nations)
        {
            NameValueCollection nvc = new()
            {
                { "wa", "1" },
                { "q", "members" }
            };

            var response = Utilities.API(nvc, null, 0, APIClient.UserAgent);

            XmlNodeList nodelist = Utilities.Parse(Utilities.StrResp(response), "*");

            HashSet<string> members = nodelist.FindProperty("MEMBERS").Replace('_', ' ').Split(',').ToHashSet();

            foreach(var n in nations)
            {
                if(members.Contains(n.Name.ToLower()))
                {
                    return n.Name;
                }
            }

            return null;
        }

        public (string pin, string chk)? Login(NationLoginViewModel login)
        {
            var request = new RestRequest("/template-overall=none/page=un");
            request.AddHeader("User-Agent", APIClient.UserAgent);
            request.AddParameter("nation", login.Name);
            request.AddParameter("password", login.Pass);
            request.AddParameter("logging_in", "1");
            request.AddParameter("userclick", UserClick);

            var response = HttpClient.Get(request);

            if(response.IsSuccessStatusCode)
            {
                var pin = response.Headers!.Where(x => x.Name == "Set-Cookie").ElementAt(0).Value
                                 !.ToString()!.Split("; ")[0][4..]; //string fuckery basically copied 1:1 from swarm, shoutout sweeze

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(response.Content);

                var chk = htmlDoc.DocumentNode.SelectSingleNode("//input[@name='chk']").Attributes["value"].Value;

                return (pin, chk);
            }

            return null;
        }

        public bool ApplyWA(string pin, string chk)
        {
            var request = new RestRequest("/template-overall=none/page=UN_status");
            request.AddHeader("User-Agent", APIClient.UserAgent);
            request.AddHeader("Cookie", $"pin={pin}");
            request.AddParameter("userclick", UserClick);

            NameValueCollection nvc = new()
            {
                { "action", "join_UN" },
                { "chk", chk },
                { "submit", "1"}
            };
            request.AddJsonBody(nvc);

            var response = HttpClient.Post(request);

            Console.WriteLine(response.Content);

            return response.IsSuccessStatusCode;
        }

        public string? GetLocalID(string pin)
        {
            return "";
        }

        public void MoveToJP(string targetRegion, string pin, string localID)
        {
            return;
        }
    }
}
