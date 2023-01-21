using dotNS;
using dotNS.Classes;
using Henson.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Henson.Models
{
    public class NSClient
    {
        public NSClient(string userAgent)
        {
            Client.UserAgent = Uri.EscapeDataString($"Henson v{GetType().Assembly.GetName().Version} developed by nation: Notanam in use by nation: {userAgent}");
        }

        public DotNS Client { get; } = new();

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
                Thread.Sleep(1200); //avoid ratelimits
            }
            return (retVal, didAuthFail);
        }
    }
}
