using dotNS;
using dotNS.Classes;
using Henson.ViewModels;
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
            Client.UserAgent = Uri.EscapeDataString($"Henson v{GetType().Assembly.GetName().Version} developed by nation: Notanam in use by nation: {userAgent}");
        }

        public DotNS Client { get; } = new();

        private const int MultipleRequestsWaitTime = 600;

        public (List<Nation> nations, bool authFailedOnSome) AuthenticateAndReturnInfo(List<NationLoginViewModel> logins)
        {
            List<Nation> retVal = new();
            bool didAuthFail = false;
            foreach(NationLoginViewModel login in logins)
            {
                try
                {
                    if(Client.UpdatePin(login.Name, login.Pass))
                    {
                        string[] nationInfo = Client.PublicShard(login.Name, new Shards.PublicShard[] { Shards.PublicShard.Name, Shards.PublicShard.Flag, Shards.PublicShard.Region });
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

            return Utilities.API(nvc, login.Pass, 0, Client.UserAgent).IsSuccessStatusCode;
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

            var response = Utilities.API(nvc, null, 0, Client.UserAgent);

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
    }
}
