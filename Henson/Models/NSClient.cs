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
using Newtonsoft.Json.Linq;
using NSDotnet;
using NSDotnet.Enums;
using ReactiveUI;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using DynamicData;

namespace Henson.Models
{
    public class NsClient : ReactiveObject
    {
        /// <summary>
        /// A NSDotNet client that interacts with the NationStates API.
        /// </summary>
        public NSAPI APIClient { get; } = NSAPI.Instance;

        /// <summary>
        /// An HTTP Client that interacts with the NationStates site.
        /// </summary>
        public RestClient HttpClient = new("https://www.nationstates.net");

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
        /// The current index when running the same function multiple times with RunMany.
        /// </summary>
        public int ManyCount
        {
            get => manyCount;
            set
            {
                this.RaiseAndSetIfChanged(ref manyCount, value);
            }
        }
        private int manyCount = 0;

        /// <summary>
        /// The current index when running the same function multiple times with RunMany.
        /// </summary>
        public int ManyTotal
        {
            get => manyTotal;
            set
            {
                this.RaiseAndSetIfChanged(ref manyTotal, value);
            }
        }
        private int manyTotal = 0;

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
            APIClient.Auth = new NSAuth(AuthType.Password, login.Pass);
            var response = await APIClient.MakeRequest(APILink + $"?nation={login.Name}&q=ping+name+flag+region");

            if(response == null || !response.IsSuccessStatusCode)
            {
                log.Error($"Failed to ping {login.Name}");
                return null;
            }

            XmlDocument doc = new();
            doc.LoadXml(await response.Content.ReadAsStringAsync());
            XmlNodeList xmlResp = doc.SelectNodes("/NATION/*")!;

            var region = char.ToUpper(FindProperty(xmlResp, "region")[0]) + FindProperty(xmlResp, "region")[1..];
            return new Nation(FindProperty(xmlResp, "name"), login.Pass, FindProperty(xmlResp, "flag"), region);
        }

        /// <summary>
        /// Runs the given function on a list of nation logins and sleeps between them.
        /// </summary>
        /// <param name="inputs">A list of data to insert.</param>
        /// <param name="function">The function to run.</param>
        /// <returns>A list of objects corresponding to what each login returned or <c>null</c> for each login failure.</returns>
        public async Task<List<TTwo?>> RunMany<TOne, TTwo>(List<TOne> inputs, Func<TOne, Task<TTwo?>> function)
        {
            ManyCount = 0;
            ManyTotal = inputs.Count;
            List<TTwo?> results = new();

            foreach(var n in inputs)
            {
                await Task.Delay(MultipleRequestsWaitTime);
                results.Add(await function(n));
                ManyCount++;
            }

            return results;
        }

        /// <summary>
        /// Gets the WA nations dump and finds the WA nation from a list of nations.
        /// </summary>
        /// <param name="nations">A list of nations.</param>
        /// <returns>The name of the nation that is in the WA, or <c>null</c> if no nation was found.</returns>
        public async Task<string?> FindWA(List<NationGridViewModel> nations)
        {
            var response = await APIClient.MakeRequest(APILink + "?wa=1&q=members");

            if(response == null || !response.IsSuccessStatusCode) return null;

            XmlDocument doc = new();
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
        /// Checks if the nation has sufficient RO perms to tag a region.
        /// </summary>
        /// <param name="nation">The nation to check.</param>
        /// <returns>The nation right back, or <c>null</c> if the nation didn't have perms.</returns>
        public async Task<NationGridViewModel?> IsROWithTagPerms(NationGridViewModel nation)
        {
            var response = await APIClient.MakeRequest(APILink + $"?region={nation.Region}&q=officers");

            if(response == null || !response.IsSuccessStatusCode) return null;

            XmlDocument doc = new();
            doc.LoadXml(await response.Content.ReadAsStringAsync());
            XmlNode root = doc.DocumentElement!;
            XmlNodeList nodeList = root.SelectNodes($"descendant::OFFICER[NATION='{nation.Name.ToLower().Replace(' ', '_')}']")!;

            string authority = FindProperty(nodeList, "AUTHORITY");

            return authority.Contains('A') && authority.Contains('C') && authority.Contains('E') ? nation : null;
            
        }

        /// <summary>
        /// Registers a login via the site.
        /// </summary>
        /// <param name="login">A username-password pair.</param>
        /// <returns>A tuple containg the chk, Local ID, and pin, or null if the login was unsuccessful.</returns>
        public async Task<(string chk, string localId, string pin, string region)?> Login(NationLoginViewModel login)
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

            var response = await HttpClient.ExecuteAsync(request);

            if(response.Content == null) return null;

            HtmlDocument htmlDoc = new();
            htmlDoc.LoadHtml(response.Content);

            string chk, localId, pin, region;
            try
            {
                //idk how to explain this but it works
                chk = htmlDoc.DocumentNode.SelectSingleNode("//input[@name='chk']").Attributes["value"].Value;
                localId = htmlDoc.DocumentNode.SelectSingleNode("//input[@name='localid']").Attributes["value"].Value;
                pin = response!.Headers!.ToList().Find(x => x.Name == "Set-Cookie")!.Value!.ToString()!.Split("; ")[0].Split('=')[1];
                region = htmlDoc.DocumentNode.SelectSingleNode("//li[@id='panelregionbar']").FirstChild.Attributes["href"].Value
                                .Replace("region=", "").Replace('_', ' ');
                region = char.ToUpper(region[0]) + region[1..];
            }
            catch (Exception)
            {
                log.Error($"Logging in to {login.Name} failed!");
                return null;
            }

            return (chk, localId, pin, region);
        }

        /// <summary>
        /// Send a WA email to a logged in nation with the chk, or resend the email if already sent.
        /// </summary>
        /// <param name="chk">The chk recorded from a login.</param>
        /// <param name="pin">The PIN recorded from a login.</param>
        /// <returns>A boolean indicating whether or not the application was successfully sent.</returns>
        public async Task<bool> ApplyWA(string chk, string pin)
        {
            RestRequest request = new("/template-overall=none/page=UN_status", Method.Post);
            request.AddHeader("User-Agent", UserAgent);
            request.AddParameter("action", "join_UN");
            request.AddParameter("chk", chk);
            request.AddParameter("resend", "1");
            request.AddParameter("userclick", UserClick);
            request.AddCookie("pin", pin, "/", ".nationstates.net");

            var response = await HttpClient.ExecuteAsync(request);

            bool successful = response.Content != null && response.Content.Contains("has been received!");
            if(!successful) log.Error($"Applying to WA failed!");

            return successful;
        }

        /// <summary>
        /// Moves a nation to another region.
        /// </summary>
        /// <param name="targetRegion">The name of the region to move to.</param>
        /// <param name="localID">The Local ID recorded from a login.</param>
        /// <param name="pin">The PIN recorded from a login.</param>
        /// <returns>A boolean indicating whether or not the move was successful.</returns>
        public async Task<bool> MoveToJP(string targetRegion, string localID, string pin)
        {
            RestRequest request = new("/template-overall=none/page=change_region", Method.Post);
            request.AddHeader("User-Agent", UserAgent);
            request.AddParameter("localid", localID);
            request.AddParameter("region_name", targetRegion);
            request.AddParameter("move_region", "1");
            request.AddParameter("userclick", UserClick);
            request.AddCookie("pin", pin, "/", ".nationstates.net");

            var response = await HttpClient.ExecuteAsync(request);

            bool successful = response.Content != null && response.Content.Contains("Success!");
            if(!successful) log.Error($"Moving to JP {targetRegion} failed!");

            return successful;
        }

        /// <summary>
        /// Change the WFE of a region that a nation has permissions for.
        /// </summary>
        /// <param name="targetRegion">The name of the region to change the WFE of.</param>
        /// <param name="chk">The chk recorded from a login.</param>
        /// <param name="pin">The PIN recorded from a login.</param>
        /// <param name="wfe">The WFE.</param>
        /// <returns></returns>
        public async Task<bool> SetWFE(string targetRegion, string chk, string pin, string wfe)
        {
            RestRequest request = new("/template-overall=none/page=region_control", Method.Post);
            request.AddHeader("User-Agent", UserAgent);
            request.AddParameter("page", "region_control");
            request.AddParameter("chk", chk);
            request.AddParameter("region", targetRegion);
            request.AddParameter("setwfebutton", "1");
            request.AddParameter("userclick", UserClick);
            request.AddCookie("pin", pin, "/", ".nationstates.net");

            // HTML Escape the wfe to preserve unicode
            /// <remarks>Because HttpUtility.HtmlEncode does not cover the whole of
            /// the ASCII space, where some emoji lie, some extra work has to go in
            /// to ensure that all characters are properly escaped</remarks>
            string Escaped = String
                .Join("",HttpUtility.HtmlEncode(wfe).ToArray()
                .Select(c => (int)c > 127 ? $"&#{(int)c};" : ""+c));
            // Because we're converting to ISO, some things need to be un-escaped
            var AsciiEscape = new [] { "&amp;", "&lt;", "&gt;", "&quot;", "&#39;" };
            foreach(var seq in AsciiEscape)
            {
                Escaped = Escaped.Replace(seq, HttpUtility.HtmlDecode(seq));
            }
            //Convert to encoding
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            request.AddParameter("message", iso.GetString(Encoding.Convert(Encoding.UTF8, iso, Encoding.UTF8.GetBytes(Escaped))));

            var response = await HttpClient.ExecuteAsync(request);

            bool successful = response.Content != null && response.Content.Contains("World Factbook Entry updated!");
            if(!successful) log.Error($"Changing WFE of {targetRegion} failed!");

            return successful;
        }

        /// <summary>
        /// Uploads a banner to the NationStates site.
        /// </summary>
        /// <param name="targetRegion">The region whose banner is being changed.</param>
        /// <param name="chk">The chk recorded from a login.</param>
        /// <param name="pin">The PIN recorded from a login.</param>
        /// <param name="file">The path to the file being uploaded.</param>
        /// <returns>The ID of the banner uploaded.</returns>
        public async Task<string?> UploadBanner(string targetRegion, string chk, string pin, string file)
        {
            RestRequest request = new("/cgi-bin/upload.cgi", Method.Post);
            request.AddHeader("User-Agent", UserAgent);
            request.AddParameter("page", "region_control");
            request.AddParameter("uploadtype", "rbanner");
            request.AddParameter("expect", "json");
            request.AddParameter("chk", chk);
            request.AddParameter("region", targetRegion);
            request.AddParameter("userclick", UserClick);
            request.AddCookie("pin", pin, "/", ".nationstates.net");

            request.AddFile("file_upload_rbanner", file);

            var response = await HttpClient.ExecuteAsync(request);

            if(!response.IsSuccessStatusCode)
            {
                log.Error($"Uploading banner to {targetRegion} failed!");
                return null;
            }

            return JObject.Parse(response.Content!)["id"]!.ToString();
        }

        /// <summary>
        /// Uploads a flag to the NationStates site.
        /// </summary>
        /// <param name="targetRegion">The region whose banner is being changed.</param>
        /// <param name="chk">The chk recorded from a login.</param>
        /// <param name="pin">The PIN recorded from a login.</param>
        /// <param name="file">The path to the file being uploaded.</param>
        /// <returns>The ID of the banner uploaded.</returns>
        public async Task<string?> UploadFlag(string targetRegion, string chk, string pin, string file)
        {
            RestRequest request = new("/cgi-bin/upload.cgi", Method.Post);
            request.AddHeader("User-Agent", UserAgent);
            request.AddParameter("page", "region_control");
            request.AddParameter("uploadtype", "rflag");
            request.AddParameter("expect", "json");
            request.AddParameter("chk", chk);
            request.AddParameter("region", targetRegion);
            request.AddParameter("userclick", UserClick);
            request.AddCookie("pin", pin, "/", ".nationstates.net");

            request.AddFile("file_upload_rflag", file);

            var response = await HttpClient.ExecuteAsync(request);

            if(!response.IsSuccessStatusCode)
            {
                log.Error($"Uploading flag to {targetRegion} failed!");
                return null;
            }

            return JObject.Parse(response.Content!)["id"]!.ToString();
        }

        /// <summary>
        /// Sets the banner and flag on the region.
        /// </summary>
        /// <param name="targetRegion">The region whose banner and flag is being changed.</param>
        /// <param name="chk">The chk recorded from a login.</param>
        /// <param name="pin">The PIN recorded from a login.</param>
        /// <param name="bannerID">The banner ID from an earlier upload.</param>
        /// <param name="flagID">The flag ID from an earlier upload.</param>
        /// <returns></returns>
        public async Task<bool> SetBannerFlag(string targetRegion, string chk, string pin, string bannerID, string flagID)
        {
            RestRequest request = new("/template-overall=none/page=region_control", Method.Post);
            request.AddHeader("User-Agent", UserAgent);
            request.AddParameter("page", "region_control");
            request.AddParameter("chk", chk);
            request.AddParameter("region", targetRegion);
            request.AddParameter("newbanner", bannerID);
            request.AddParameter("newflag", flagID);
            request.AddParameter("saveflagandbannerchanges", "1");
            request.AddParameter("flagmode", "flag");
            request.AddParameter("newflagmode", "flag");
            request.AddParameter("userclick", UserClick);
            request.AddCookie("pin", pin, "/", ".nationstates.net");

            var response = await HttpClient.ExecuteAsync(request);

            bool successful = response.Content != null && response.Content.Contains("banner/flag updated!");
            if(!successful) log.Error($"Setting banner and flag of {targetRegion} failed!");

            return successful;
        }

        /// <summary>
        /// Gets a list of embassies from the region.
        /// </summary>
        /// <param name="targetRegion">The region to get embassies from.</param>
        /// <returns>A list of lists of strings representing the embassy relations has, or null if something went wrong.</returns>
        public async Task<List<(string name, int type)>?> GetEmbassies(string targetRegion)
        {
            var response = await APIClient.MakeRequest(APILink + $"?region={targetRegion}&q=embassies");
            
            if(response == null || !response.IsSuccessStatusCode) return null;
            
            XmlDocument doc = new();
            doc.LoadXml(await response.Content.ReadAsStringAsync());
            XmlNode root = doc.DocumentElement!;

            List<(string name, int type)>? retVal = new();
            foreach(var node in root.SelectNodes(".//EMBASSY")!.Cast<XmlNode>())
            {
                int embCode;                
                if(node.Attributes?["type"] == null)
                {
                    embCode = 0;
                }
                else
                {
                    embCode = new[] { "invited", "pending", "requested" }.IndexOf(node.Attributes["type"]!.Value) + 1; //lol
                    if(embCode == 0) continue; //double lol
                }
                retVal.Add(new(node.InnerText, embCode));
            }
            
            return retVal;
        }

        /// <summary>
        /// Closes/Rejects an offer/withdraws an offer for an embassy from a region.
        /// </summary>
        /// <param name="targetRegion">The region that the nation is currently in.</param>
        /// <param name="chk">The chk recorded from a login.</param>
        /// <param name="pin">The PIN recorded from a login.</param>
        /// <param name="regionToClose">The region to close embassies with.</param>
        /// <param name="closeType">The type of embassy relationship that exists.</param>
        /// <returns>Whether the closures were successful.</returns>
        public async Task<bool> CloseEmbassy(string targetRegion, string chk, string pin, string regionToClose, int closeType)
        {
            RestRequest request = new("/template-overall=none/page=region_control", Method.Post);
            request.AddHeader("User-Agent", UserAgent);
            request.AddParameter("page", "region_control");
            request.AddParameter("chk", chk);
            request.AddParameter("region", targetRegion);

            if(closeType == 0) request.AddParameter("cancelembassyregion", regionToClose);
            else if(closeType == 1) request.AddParameter("rejectembassyregion", regionToClose);
            else if(closeType == 2) request.AddParameter("abortembassyregion", regionToClose);
            else request.AddParameter("withdrawembassyregion", regionToClose);
            
            request.AddCookie("pin", pin, "/", ".nationstates.net");

            var response = await HttpClient.ExecuteAsync(request);
            
            bool successful = response.Content != null &&
                              (response.Content.Contains(" rejected.") ||
                               response.Content.Contains(" demolition.") ||
                               response.Content.Contains(" withdrawn.") ||
                               response.Content.Contains(" aborted."));
            if(!successful) log.Error($"Rejecting embassy of {regionToClose} from {targetRegion} failed!");

            return successful;
        }

        public async Task<bool> RequestEmbassy(string targetRegion, string chk, string pin, string regionToRequest)
        {
            RestRequest request = new("/template-overall=none/page=region_control", Method.Post);
            request.AddHeader("User-Agent", UserAgent);
            request.AddParameter("page", "region_control");
            request.AddParameter("chk", chk);
            request.AddParameter("region", targetRegion);
            request.AddParameter("requestembassyregion", regionToRequest);
            request.AddParameter("requestembassy", "1");
            request.AddCookie("pin", pin, "/", ".nationstates.net");

            var response = await HttpClient.ExecuteAsync(request);

            bool successful = response.Content != null && response.Content.Contains(" has been sent.");
            if(!successful) log.Error($"Requesting embassy of {regionToRequest} from {targetRegion} failed!");

            return successful;
        }

        public async Task<bool> AddTag(string targetRegion, string chk, string pin, string tag)
        {
            RestRequest request = new("/template-overall=none/page=region_control", Method.Post);
            request.AddHeader("User-Agent", UserAgent);
            request.AddParameter("page", "region_control");
            request.AddParameter("chk", chk);
            request.AddParameter("region", targetRegion);
            request.AddParameter("add_tag", tag);
            request.AddParameter("updatetagsbutton", "1");
            request.AddCookie("pin", pin, "/", ".nationstates.net");

            var response = await HttpClient.ExecuteAsync(request);
            
            bool successful = response.Content != null && response.Content.Contains(" updated!");
            if(!successful) log.Error($"Adding tag {tag} to {targetRegion} failed!");

            return successful;
        }
        
        public async Task<bool> RemoveTag(string targetRegion, string chk, string pin, string tag)
        {
            RestRequest request = new("/template-overall=none/page=region_control", Method.Post);
            request.AddHeader("User-Agent", UserAgent);
            request.AddParameter("page", "region_control");
            request.AddParameter("chk", chk);
            request.AddParameter("region", targetRegion);
            request.AddParameter("remove_tag", tag);
            request.AddParameter("updatetagsbutton", "1");
            request.AddCookie("pin", pin, "/", ".nationstates.net");

            var response = await HttpClient.ExecuteAsync(request);
            
            bool successful = response.Content != null && response.Content.Contains(" updated!");
            if(!successful) log.Error($"Adding tag {tag} to {targetRegion} failed!");

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
