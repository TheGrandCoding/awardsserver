using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace AwardsServer.ServerUI
{
    public class HTTPResponseContext
    {
        private static string HTML_AuthOrViewPage;
        public static string HTML_AuthPage => HTML_AuthOrViewPage.Replace("[[AUTH_OR_VIEW]]", "auth");
        public static string HTML_ViewPage => HTML_AuthOrViewPage.Replace("[[AUTH_OR_VIEW]]", "view");


        public static string ServerUrl => $"http://{Program.GetLocalIPAddress()}/";
        public TcpClient Client;
        public string ClientIP
        { get
            {
                var ip = ((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString();
                //if (ip == Program.GetLocalIPAddress())
                //    return "127.0.0.1";
                return ip;
            } }
        public string NameOrIdentity => ((AuthenticatedAs?.FullName) ?? ClientIP);
        public User AuthenticatedAs;
        public User Viewing;
        public string FullHTTPRequest;
        public string URLUntilTokens;
        public Dictionary<string, string> Tokens = new Dictionary<string, string>();
        public Dictionary<string, string> Cookies = new Dictionary<string, string>();
        public Dictionary<string, string> Headers = new Dictionary<string, string>();
        public string Body = "Server encrountered an error before it was able to process your request";
        public string Title = "Y11 - 500 error";
        public string AdditionalHeadText = "";
        public HttpStatusCode Code = HttpStatusCode.InternalServerError;
        public bool IgnoreHTMLFormatting = false;

        public HTTPResponseContext(TcpClient client, string request)
        {
            if (HTML_AuthOrViewPage == null)
                HTML_AuthOrViewPage = Properties.Resources.WebAuthentificationPage;
            Client = client;
            FullHTTPRequest = request;
            string[] perLine = FullHTTPRequest.Replace("\r", "").Split('\n');
            string pathWanted = perLine[0].Split(' ')[1];
            pathWanted = Uri.UnescapeDataString(pathWanted);
            URLUntilTokens = pathWanted;
            if (pathWanted.Contains("?"))
            { // finds dictionary of any 'tokens' ie:
                // http://127.0.0.1/?name=Dave&job=unemployed
                // will return a dictionary of:
                // "name":"Dave"; "job":"unemployed"
                URLUntilTokens = pathWanted.Substring(0, pathWanted.IndexOf("?"));
                string pathWithTokens = pathWanted.Replace(URLUntilTokens, "");
                pathWithTokens = pathWithTokens.Replace("?", "&");
                string[] tokenInfo = pathWithTokens.Split('&');
                foreach (string tokenString in tokenInfo)
                {
                    if (tokenString == "")
                    {
                        continue;
                    }
                    if (tokenString.Contains("=") == false)
                    {
                        Tokens.Add(tokenString, "");
                        continue;
                    }
                    string name = tokenString.Substring(0, tokenString.IndexOf("="));
                    string value = tokenString.Replace(name + "=", "");
                    Tokens.Add(name, value);
                }
            }

            foreach(var line in perLine)
            {
                if(line.StartsWith("Cookie: "))
                {
                    var nowLine = line.Replace("Cookie: ", "");
                    foreach(var splited in nowLine.Split(';'))
                    {
                        var cookie = splited.Trim();
                        var namevalue = cookie.Split('=');
                        Cookies.Add(Uri.UnescapeDataString(namevalue[0]), Uri.UnescapeDataString(namevalue[1]));
                    }
                }
            }
        }


        public void Execute/* Order 66 */()
        {
            try
            {
                GetResponse();
            } catch (Exception ex)
            {
                Logging.Log(Logging.LogSeverity.Error, ex.ToString(), "Web:" + NameOrIdentity);
                Body = "<label class=\"error\">An error occured while processing your request.<br>The error has been logged.</label>";
                Code = HttpStatusCode.InternalServerError;
                Title = "Awards - Error";
            } finally
            {
                RespondHTTP(Client, Body, Code, Headers, Title, IgnoreHTMLFormatting, AdditionalHeadText);
            }
        }

        private static string Link(string url, string tooltip = "")
        {
            if (string.IsNullOrWhiteSpace(tooltip))
                tooltip = url;
            return $"<a href=\"{url}\">{tooltip}</a>";
        }

        /// <summary>
        /// Compiles all the passed information into a proper up-to-regulation HTTP response
        /// </summary>
        /// <param name="client">Connetion to response to</param>
        /// <param name="body">HTML body text to send</param>
        /// <param name="httpCode">HTTP response status code</param>
        /// <param name="headers">Any headers to add or overwrite</param>
        /// <param name="pageTitle">Titled of the page, displayed in the tab thingy</param>
        /// <param name="forceDisableTextBuild">"Lets just ignore HTML, send it raw!" - Disregards any HTML formatting, and only sends as per HTTP</param>
        /// <param name="additionalHead">Any additional text that is added just before the head tag is closed</param>
        private void RespondHTTP(TcpClient client, string body, HttpStatusCode httpCode = HttpStatusCode.InternalServerError, Dictionary<string, string> headers = null, string pageTitle = "Y11 Awards", bool forceDisableTextBuild = false, string additionalHead = "")
        {
            List<string> FooterArr = new List<string>();
            if(AuthenticatedAs != null)
            {
                if(AuthenticatedAs.Flags.Contains(Flags.Coundon_Staff))
                {
                    FooterArr.Add(Link(ServerUrl + "student", "See a student's votes"));
                } else if(!AuthenticatedAs.Flags.Contains(Flags.Disallow_View_Online))
                {
                    FooterArr.Add(Link(ServerUrl, "See your votes"));
                } 
            }
            FooterArr.Add(Link(ServerUrl + "all", "See all votes"));

            List<string> response_text = new List<string>()
            {
                "<!DOCTYPE html>", // tells browser its a HTML page
                "<html>",
                "<head>",
                "<link rel=\"stylesheet\" href=\"/WebStyles.css\">", // in resources
                "<script src=\"/WebScripts.js\"></script>", // also in resources
                "<meta charset=\"UTF-8\">",
                $"<title>{pageTitle}</title>",
                additionalHead,
                "</head>",
                "<body>",
                body,
                "</body>",
                $"<br><hr><footer>{string.Join(" - ", FooterArr)}</footer>",
                "</html>"
            };
            string response_body_raw = string.Join("", response_text);
            if (forceDisableTextBuild) // forces us to not set the above, maybe we want to send a raw text file
                response_body_raw = body;
            Dictionary<string, string> response_headers = new Dictionary<string, string>()
            {
                { "Content-Type", "text/html; encoding=utf8" },
                { "Content-Length", response_body_raw.Length.ToString() },
                { "Connection", "Keep-Alive" }
            };
            if (headers != null)
            { // overrides, or adds new, headers
                foreach (KeyValuePair<string, string> keypair in headers)
                {
                    if (response_headers.ContainsKey(keypair.Key))
                    {
                        response_headers[keypair.Key] = keypair.Value;
                    }
                    else
                    {
                        response_headers.Add(keypair.Key, keypair.Value);
                    }
                }
            }
            // The below compiles all the above into proper format, then sends it.
            string response_headers_raw = "";
            foreach (KeyValuePair<string, string> keypair in response_headers)
            {
                response_headers_raw += keypair.Key + ": " + keypair.Value + "\n";
            }
            string response_proto = "HTTP/1.1";
            string response_status = ((int)httpCode).ToString();
            string response_status_text = httpCode.ToString();
            string initialSend = $"{response_proto} {response_status} {response_status_text}";
            string toSend = initialSend + "\n" + response_headers_raw + "\n" + response_body_raw;
            WriteClient(client, toSend);
        }

        /// <summary>
        /// Sends the message to the client
        /// Static.. so its helpful
        /// </summary>
        public static void WriteClient(TcpClient client, string message)
        {
            message = $"%{message}`";
            NetworkStream stream = client.GetStream();
            stream.Flush();
            Byte[] broadcastBytes = Encoding.UTF8.GetBytes(message);
            stream.Write(broadcastBytes, 0, broadcastBytes.Length);
            stream.Flush();
        }

        internal static bool IsIgnoredFile(string url)
        {
            return url.EndsWith(".js") || url.EndsWith(".css") || url.EndsWith(".ico");
        }

        internal virtual bool CheckCookiesAndSetVariables()
        {
            if (!Program.Options.WebSever_Enabled)
            {
                Body = "<p>Website of the Awards server has been disabled</p>";
                Title = "Offline";
                Code = (HttpStatusCode)418;
                return false;
            }
            if (!IsIgnoredFile(URLUntilTokens))

            {
                if (Cookies.TryGetValue("Auth", out string authorisation))
                {
                    string[] splited = new string[] { "", "", "" };
                    if(authorisation.Contains(" "))
                    {
                        authorisation = authorisation.Replace(";", "-");
                        var spl1 = authorisation.Split(' ');
                        var spl2 = spl1[1].Split('-');
                        splited = new string[]
                        {
                            spl1[0],
                            spl2[0],
                            spl2[1]
                        };
                    } else
                    {
                        splited = authorisation.Split(';');
                    }
                    var name = splited[0];
                    var lastName = splited[1];
                    var tutor = splited[2];
                    AuthenticatedAs = Program.Database.AllStudents.Values.FirstOrDefault(x =>
                    x.AccountName.ToLower() == name.ToLower() &&
                    x.LastName.ToLower() == lastName.ToLower() &&
                    x.Tutor.ToLower() == tutor.ToLower());
                    if (AuthenticatedAs == null || (
                        (
                            !SocketHandler.CachedKnownIPs.ContainsKey(AuthenticatedAs.AccountName) ||
                            SocketHandler.CachedKnownIPs[AuthenticatedAs.AccountName].ToString() != ClientIP ||
                            AuthenticatedAs.Flags.Contains(Flags.Disallow_View_Online)
                        )
                        && !AuthenticatedAs.Flags.Contains(Flags.View_Online)))
                    {
                        Body = "<label class=\"error\">Authentification failed<br>Any of the following may apply:<br> - You entered incorrect name/information<br> - You have not logged in / voted today <br> - You voted from another computer <br> - You may not be permitted to see your vote</label>";
                        Body += "<br><br><br><hr>" + HTML_AuthPage;
                        Code = HttpStatusCode.Forbidden;
                        Title = "Forbidden";
                        AuthenticatedAs = null; // since they're not authenticated
                        return false;
                    }
                }
                if (Cookies.TryGetValue("View", out string wanted))
                {
                    if (Program.TryGetUser(wanted, out Viewing))
                    {
                        // nothing
                    }
                }
            }
            return true;
        }

        internal virtual bool CheckAuth()
        {
            // checks if auth is needed, if not returns ERRORED
            if(!(URLUntilTokens.EndsWith(".js") || URLUntilTokens.EndsWith(".css")))
            { // Allow them to see the js/css files
                if (AuthenticatedAs == null)
                { // No auth was provided (or was accepted)
                    if (Cookies.ContainsKey("Auth"))
                    {
                        Body = "<label class=\"error\">Authentification failed, you may need to connect a client to the server, then try again.</label><br><hr><br>";
                        Body += HTML_AuthPage;
                        Title = "Auth Rejected";
                        Code = HttpStatusCode.Unauthorized;
                    }
                    else
                    {
                        Body = HTML_AuthPage;
                        Title = "Auth Failed";
                        Code = HttpStatusCode.Forbidden;
                    }
                    return false;
                }
                else
                {
                    /*if (
                    (AuthenticatedAs == null ||
                    !SocketHandler.CachedKnownIPs.ContainsKey(AuthenticatedAs.AccountName) ||
                    SocketHandler.CachedKnownIPs[AuthenticatedAs.AccountName].ToString() != ClientIP ||
                    AuthenticatedAs.Flags.Contains(Flags.Disallow_View_Online))
                    && !AuthenticatedAs.Flags.Contains(Flags.View_Online))
                    {
                        Body = "<label class=\"error\">Authentification failed<br>Any of the following may apply:<br> - You entered incorrect name/information<br> - You have not logged in / voted today <br> - You voted from another computer <br> - You may not be permitted to see your vote</label>";
                        Code = HttpStatusCode.Forbidden;
                        Title = "Forbidden";
                        return false;
                    }*/
                    // duplicate , shsould already be checked in the CheckAuthsAndSetCookies() func               
                }
            } else if (URLUntilTokens == "/student")
            {
                if(Viewing == null)
                {
                    if(Cookies.ContainsKey("View"))
                    {
                        Body = $"<label class=\"error\">Either the info provided was incorrect, or you are unable to view that</label>";
                        Title = "Cannot View Student";
                        Code = HttpStatusCode.Forbidden;
                    }
                    else
                    {
                        Body = $"<label class=\"error\">You need to provide the student's information first:</label><br>"; 
                        Body += HTML_ViewPage;
                        Title = "Provide Info";
                        Code = HttpStatusCode.BadRequest;
                    }
                    return false;
                } else
                {
                    if(Viewing.Flags.Contains(Flags.Disallow_View_Online) ||
                        (
                        Viewing.AccountName == AuthenticatedAs.AccountName && 
                        AuthenticatedAs.Flags.Contains(Flags.Coundon_Staff) && 
                        AuthenticatedAs.Flags.Contains(Flags.Disallow_Vote_Staff)
                        )
                    ) {
                        Body = $"<label class=\"error\">Either the info provided was incorrect, or you are unable to view that</label>";
                        Title = "Cannot View Student";
                        Code = HttpStatusCode.Forbidden;
                        return false;
                    }
                }
            }
            return true;
        }

        internal virtual bool CheckRedirect()
        {
            if(URLUntilTokens == "/")
            {
                if(AuthenticatedAs.Flags.Contains(Flags.Coundon_Staff) && AuthenticatedAs.Flags.Contains(Flags.Disallow_Vote_Staff))
                {
                    Body = "Redirecting you to: " + Link(ServerUrl + "student");
                    Code = HttpStatusCode.TemporaryRedirect;
                    Title = "Redirect...";
                    Headers.Add("Location", ServerUrl + "student");
                    return false;
                } else if (AuthenticatedAs.Flags.Contains(Flags.Disallow_Vote_Staff))
                {
                    Body = "Redirecting you to: " + Link(ServerUrl + "all");
                    Code = HttpStatusCode.TemporaryRedirect;
                    Title = "Redirect...";
                    Headers.Add("Location", ServerUrl + "all");
                    return false;
                }
            }
            return true;
        }

        internal virtual void GetResponse()
        {
            throw new InvalidOperationException("Class should be implemented, and this function overriden");
        }
    }

    public class HTTPGetResponse : HTTPResponseContext
    {
        public HTTPGetResponse(TcpClient client, string request) : base(client, request)
        {

        }

        internal override void GetResponse()
        {
            bool getCookies = CheckCookiesAndSetVariables(); // needs to be called before below log msg, as the AuthenticatedAs is assigned in this func
            if(!IsIgnoredFile(URLUntilTokens))
                Logging.Log(Logging.LogSeverity.Info, NameOrIdentity + " requested " + URLUntilTokens + $"{(AuthenticatedAs == null ? "" : "  -Auth: " + AuthenticatedAs.ToString("AN"))}" + $"{(Viewing == null ? "" : "   -View: " + Viewing.ToString("AN"))}", (AuthenticatedAs == null ? "" :  AuthenticatedAs.AccountName + "/") + "Web" );
            if(getCookies && CheckAuth())
            { // Ensure they are permitted to view that page
                if (CheckRedirect())
                { // Check if we should redirect fire
                    if (URLUntilTokens == "/")
                    {
                        Title = "Y11 Awards";

                        string text = "<label>Your votes for each category:</label>";
                        text += $"<table><tr><th>Category</th><th>First</th><th>Second</th></tr>";
                        int catCount = 0;
                        foreach (var category in Program.Database.AllCategories.Values)
                        { // builds information into a table, for display.
                            catCount++;
                            string clrClass = (catCount % 2 == 0) ? " class=\"tblEven\"" : "";
                            string catText = "<tr" + clrClass + "><td>{0}</td><td>{1}</td><td>{2}</td></tr>";
                            var users = category.GetVotesBy(AuthenticatedAs);
                            catText = string.Format(catText, category.Prompt, users.Item1?.FullName ?? "N/A", users.Item2?.FullName ?? "N/A");
                            text += catText;
                        }
                        text += "</table>";
                        Body = text; Code = HttpStatusCode.OK;
                    }
                    else if (URLUntilTokens == "/WebStyles.css")
                    { // returns web styling
                        Body = Properties.Resources.WebStyles;
                        Code = HttpStatusCode.OK;
                        IgnoreHTMLFormatting = true; // without any HTML formatting
                        Headers.Add("Content-Type", "text/css");
                    }
                    else if (URLUntilTokens == "/WebScripts.js")
                    { // as with the CSS above
                        Body = Properties.Resources.WebScripts;
                        Code = HttpStatusCode.OK;
                        IgnoreHTMLFormatting = true;
                        Headers.Add("Content-Type", "text/javascript");
                    }
                    else if (URLUntilTokens == "/all")
                    {
                        if (AuthenticatedAs == null)
                        {
                            // we should redirect them to get authenticated.
                            AdditionalHeadText = $"<meta http-equiv=\"refresh\" content=\"5; URL = http://{Program.GetLocalIPAddress()}/\" />";
                            Body = $"<label class=\"error\">You need to be authenticated.<br>You will be redirected shortly.<br><br>If not, head to http://{Program.GetLocalIPAddress()}/</label>";
                            Code = HttpStatusCode.Unauthorized;
                            return;
                        }
                        string cssClass = ((ClientIP == Program.GetLocalIPAddress() || ClientIP == "127.0.0.1" || Program.Options.Allow_NonLocalHost_WebConnections || AuthenticatedAs.Flags.Contains(Flags.Coundon_Staff))) ? "" : "class=\"hidden\"";
                        // anyone can access the info
                        // but, we hide some data via css
                        // which isnt particularly secure.. might change in future.
                        // however: I have set the javascript to remove any information within a [REDACTED] element
                        // which means that it is in no way secure - its still being sent
                        // BUT it is impossible to get the data using just Inspect element, which will prevent access from most/all
                        Code = HttpStatusCode.OK; Title = "Y11: All Data";
                        string htmlPage = Properties.Resources.WebAllDataPage; // this is the base template, which data will be added into
                        int notVoted = Program.Database.AllStudents.Count; // start with all students added
                        int currentlyVoting = 0;
                        int alreadyVoted = 0;
                        foreach (var student in Program.Database.AllStudents.Values)
                        { // removes them as needed to increment the two above
                            if (Program.Database.AlreadyVotedNames.Contains(student.AccountName))
                            {
                                notVoted--;
                                alreadyVoted++;
                            }
                            else if (SocketHandler.CurrentClients.FirstOrDefault(x => x.User.AccountName == student.AccountName) != null)
                            {
                                notVoted--;
                                currentlyVoting++;
                            }
                        }
                        // Displays how many total votes have been made, and how many unique people have been voted, for each category.
                        string categoryTable = "<table><tr><th>Category</th><th>Total Votes (by people)</th><th>Unique Voted (num people voted)</th></tr>";
                        foreach (var category in Program.Database.AllCategories.Values)
                        {
                            int votes = 0;
                            foreach (var list in category.Votes)
                            {
                                votes += list.Value.Count;
                            }
                            string tempClass = (category.ID % 2 == 0) ? "class=\"tblEven\"" : "";
                            string format = $"<tr {tempClass}><td>{category.Prompt}</td><td>{votes}</td><td>{category.Votes.Count}</td></tr>";
                            categoryTable += format;
                        }
                        categoryTable += "</table>";

                        string winnersTable = "<table><tr><th>Category</th><th>First Winner</th><th>Second Winner</th></tr>";
                        foreach (var category in Program.Database.AllCategories.Values)
                        {
                            var highestVote = category.HighestVoter(false);
                            var highestWinners = highestVote.Item1;
                            var secondHighestVote = category.HighestVoter(true);
                            var secondHighestWinners = secondHighestVote.Item1;
                            string first = $"{string.Join(", ", highestWinners.Select(x => x.ToString("FN LN (TT)")))}";
                            if (string.IsNullOrWhiteSpace(first)) first = "N/A";
                            string second = $"{string.Join(", ", secondHighestWinners.Select(x => x.ToString("FN LN (TT)")))}";
                            if (string.IsNullOrWhiteSpace(second)) second = "N/A";
                            string format = "<tr><td>{1}</td><td {0}>{4}: {2}</td><td {0}>{5}: {3}</td></tr>";
                            format = string.Format(format, cssClass, category.Prompt, first, second, $"<strong>{highestVote.Item2}</strong>", $"<strong>{secondHighestVote.Item2}</strong>");
                            winnersTable += format;
                        }
                        winnersTable += "</table>";


                        // replaces data from the templace, see the WebAllDataPage.txt
                        Dictionary<string, string> ReplaceValues = new Dictionary<string, string>()
                    {
                        { "[[NUM_NOT_VOTED]]", notVoted.ToString() },
                        { "[[NUM_VOTED]]", alreadyVoted.ToString() },
                        { "[[NUM_VOTING]]", currentlyVoting.ToString() },
                        { "[[HIDENOT]]", cssClass },
                        { "[[CATEGORY_TABLE]]", categoryTable },
                        { "[[WINNER_TABLE]]", winnersTable }

                    };
                        foreach (var keypair in ReplaceValues) { htmlPage = htmlPage.Replace(keypair.Key, keypair.Value); }

                        Body = htmlPage;
                        // This is commented out because it works.
                        // I've added this in because i'm aware that technically speaking
                        // another <DOCTYPE html> and <head> and <body> elements are being put into the preexisting <body> location
                        // but google chrome seems to work it out.. and it works. So there's that.
                        // forceIgnoreWebpage = true;
                    }
                    else if (URLUntilTokens == "/student")
                    {
                        if (!AuthenticatedAs.Flags.Contains(Flags.Coundon_Staff))
                        {
                            Body = "<label class=\"error\">You do not have access to view other people's votes.</label>";
                            Code = HttpStatusCode.Forbidden;
                            Title = "Forbidden";
                            return;
                        }
                        if (Viewing == null)
                        {
                            Body = "<label class=\"error\">You will need to enter the information of the desired student:</label><br>" + HTML_ViewPage;
                            Code = HttpStatusCode.BadRequest;
                            Title = "Invalid Info";
                            return;
                        }
                        else
                        {
                            string text = $"<label>{Viewing.ToString("FN LN")}'s votes for each category:</label>";
                            text += $"<table><tr><th>Category</th><th>First</th><th>Second</th></tr>";
                            int catCount = 0;
                            foreach (var category in Program.Database.AllCategories.Values)
                            { // builds information into a table, for display.
                                catCount++;
                                string clrClass = (catCount % 2 == 0) ? " class=\"tblEven\"" : "";
                                string catText = "<tr" + clrClass + "><td>{0}</td><td>{1}</td><td>{2}</td></tr>";
                                var users = category.GetVotesBy(Viewing);
                                catText = string.Format(catText, category.Prompt, users.Item1?.FullName ?? "N/A", users.Item2?.FullName ?? "N/A");
                                text += catText;
                            }
                            text += "</table>";
                            Body = text; Code = HttpStatusCode.OK;
                        }
                    }
                    else
                    { // unknown/not set up request
                        Body = "<label class=\"error\">404 - Unkown request.</label>"; Code = HttpStatusCode.NotFound; Title = "404 - Not Found";
                    }
                }
            }
        }
    }
}
