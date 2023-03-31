/*
Henson's NationStates client
Copyright (C) 2023 NotAName320

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/


using Henson.ViewModels;
using HtmlAgilityPack;
using log4net;
using NSDotnet;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Henson.Models
{
    public class NsClient
    {
        /// <summary>
        /// A NSDotNet client that interacts with the NationStates API.
        /// </summary>
        public NSAPI APIClient { get; } = NSAPI.Instance;

        /// <summary>
        /// An HTTP Client that interacts with the NationStates site.
        /// </summary>
        public RestClient HttpClient = new("https://nationstates.net");

        /// <summary>
        /// A formatted User Agent that can be used to identify Henson to the site.
        /// </summary>
        public string UserAgent
        {
            get => APIClient.UserAgent;
            set
            {
                APIClient.UserAgent = Uri.EscapeDataString($"Henson v{GetType().Assembly.GetName().Version} developed by nation: Notanam in use by nation: {value}");
            }
        }

        /// <summary>
        /// A function that automatically gets the current unix second in string form.
        /// </summary>
        private static string UserClick => DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        /// <summary>
        /// The time to wait between multiple requests.
        /// </summary>
        private const int MultipleRequestsWaitTime = 750;

        /// <summary>
        /// The link to the NationStates API.
        /// </summary>
        private const string APILink = "https://www.nationstates.net/cgi-bin/api.cgi";

        /// <summary>
        /// The log4net logger. It will emit messages as from NSClient.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        /// Pings a nation via the API and sends info back.
        /// </summary>
        /// <param name="login">A username-password pair.</param>
        /// <returns>A <c>Nation</c> object with the nation's info or <c>null</c> if the login failed.</returns>
        public async Task<Nation?> Ping(NationLoginViewModel login)
        {
            var response = await APIClient.MakeRequest(APILink + $"?nation={login.Name}&q=ping+name+flag+region", login.Pass);

            if(response == null || !response.IsSuccessStatusCode) return null;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(await response.Content.ReadAsStringAsync());
            XmlNodeList xmlResp = doc.SelectNodes("/NATION/*")!;

            var region = char.ToUpper(FindProperty(xmlResp, "region")[0]) + FindProperty(xmlResp, "region")[1..];
            return new Nation(FindProperty(xmlResp, "name"), login.Pass, FindProperty(xmlResp, "flag"), region);
        }

        /// <summary>
        /// Runs <c>Ping</c> on a list of nation logins and sleeps between them.
        /// </summary>
        /// <param name="logins">A list of username-password pairs.</param>
        /// <returns>A list of <c>Nation</c> objects with the nations' info or <c>null</c> for each login failure.</returns>
        public async Task<List<Nation?>> PingMany(List<NationLoginViewModel> logins)
        {
            List<Nation?> loginSuccesses = new();

            foreach(var n in logins)
            {
                await Task.Delay(MultipleRequestsWaitTime);
                loginSuccesses.Add(await Ping(n));
            }

            return loginSuccesses;
        }

        /// <summary>
        /// Gets the WA nations dump and finds the WA nation from a list of nations.
        /// </summary>
        /// <param name="nations">A list of nations.</param>
        /// <returns>The name of the nation that is in the WA, or <c>null</c> if no nation was found.</returns>
        public async Task<string?> FindWA(List<NationGridViewModel> nations)
        {
            var response = await APIClient.MakeRequest(APILink + $"?wa=1&q=members");

            if(response == null || !response.IsSuccessStatusCode) return null;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(await response.Content.ReadAsStringAsync());
            XmlNodeList xmlResp = doc.SelectNodes("/WA/*")!;

            HashSet<string> members = FindProperty(xmlResp, "MEMBERS").Replace('_', ' ').Split(',').ToHashSet();

            foreach(var n in nations)
            {
                if(members.Contains(n.Name.ToLower()))
                {
                    return n.Name;
                }
            }

            return null;
        }

        /// <summary>
        /// Registers a login via the site.
        /// </summary>
        /// <param name="login">A username-password pair.</param>
        /// <returns>A tuple containg the chk and Local ID, or null if the login was unsuccessful.</returns>
        public (string chk, string localId)? Login(NationLoginViewModel login)
        {
            log.Info($"Logging in to {login.Name}");
            //non template=none region page allows us to get chk and localid in one request
            //shoutout to sweeze
            RestRequest request = new("/region=notas_region", Method.Get);
            request.AddHeader("User-Agent", UserAgent);
            request.AddParameter("nation", login.Name);
            request.AddParameter("password", login.Pass);
            request.AddParameter("logging_in", "1");
            request.AddParameter("userclick", UserClick);

            var response = HttpClient.Execute(request);
            HtmlDocument htmlDoc = new();
            htmlDoc.LoadHtml(response.Content);

            string chk, localId;
            try
            {
                chk = htmlDoc.DocumentNode.SelectSingleNode("//input[@name='chk']").Attributes["value"].Value;
                localId = htmlDoc.DocumentNode.SelectSingleNode("//input[@name='localid']").Attributes["value"].Value;
            }
            catch (Exception)
            {
                log.Error($"Logging in to {login.Name} failed!");
                return null;
            }

            return (chk, localId);
        }

        /// <summary>
        /// Send a WA email to a logged in nation with the chk, or resend the email if already sent.
        /// </summary>
        /// <param name="chk">The chk recorded from a login.</param>
        /// <returns>A boolean indicating whether or not the application was successfully sent.</returns>
        public bool ApplyWA(string chk)
        {
            RestRequest request = new("/template-overall=none/page=UN_status", Method.Post);
            request.AddHeader("User-Agent", UserAgent);
            request.AddParameter("action", "join_UN");
            request.AddParameter("chk", chk);
            request.AddParameter("resend", "1");
            request.AddParameter("userclick", UserClick);

            var response = HttpClient.Execute(request);

            bool successful = response.Content != null && response.Content.Contains("has been received!");
            if(!successful) log.Error($"Applying to WA failed!");

            return successful;
        }

        /// <summary>
        /// Moves a nation to another region.
        /// </summary>
        /// <param name="targetRegion">The name of the region to move to.</param>
        /// <param name="localID">The Local ID recorded from a login.</param>
        /// <returns>A boolean indicating whether or not the move was successful.</returns>
        public bool MoveToJP(string targetRegion, string localID)
        {
            RestRequest request = new("/template-overall=none/page=change_region", Method.Post);
            request.AddHeader("User-Agent", UserAgent);
            request.AddParameter("localid", localID);
            request.AddParameter("region_name", targetRegion);
            request.AddParameter("move_region", "1");
            request.AddParameter("userclick", UserClick);

            var response = HttpClient.Execute(request);

            bool successful = response.Content != null && response.Content.Contains("Success!");
            if(!successful) log.Error($"Moving to JP {targetRegion} failed!");

            return successful;
        }

        private static string FindProperty(XmlNodeList nodes, string name, int depth = 0)
        {
            //Imported from https://github.com/kolya5544/dotNS/blob/master/Utilities.cs, used to easily select XML nodes
            if (nodes == null) return "";
            if (depth == 5) return "";
            name = name.ToLower();
            foreach (XmlNode node in nodes)
            {
                if (node.Name.ToLower() == name) return node.InnerText;
                if (node.Attributes != null && node.Attributes[name] != null) return node.Attributes[name]!.InnerText;
                if (node.ChildNodes.Count > 0)
                {
                    string anything = FindProperty(node.ChildNodes, name, depth + 1);
                    if (anything != "") return anything;
                }
            }
            return "";
        }
    }
}
