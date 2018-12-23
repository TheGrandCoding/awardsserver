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
    // Im going to try my best to comment this, Liliana..

    /// <summary>
    /// Handles all connections made via a browser to the website.
    /// </summary>
    public class WebsiteHandler
    {
        private static bool _started = false;
        private TcpListener WebServer;
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
        /// <summary>
        /// Converts an integer code to its string counterpart (ie, 404 -> 'Page not found')
        /// </summary>
        /// <param name="code">Code to convert</param>
        public static string HttpCodeToString(int code)
        {
            return ((HttpStatusCode)code).ToString();
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
        private void RespondHTTP(TcpClient client, string body, int httpCode = 200, Dictionary<string,string> headers = null, string pageTitle = "Y11 Awards", bool forceDisableTextBuild = false)
        {
            List<string> response_text = new List<string>()
            {
                "<!DOCTYPE html>", // tells browser its a HTML page
                "<html>",
                "<head>",
                "<link rel=\"stylesheet\" href=\"/WebStyles.css\">", // in resources
                "<script src=\"/WebScripts.js\"></script>", // also in resources
                "<meta charset=\"UTF-8\">",
                $"<title>{pageTitle}</title>",
                "</head>",
                "<body>",
                body,
                "</body>",
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
                    if(response_headers.ContainsKey(keypair.Key))
                    {
                        response_headers[keypair.Key] = keypair.Value;
                    } else
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
            string response_status = httpCode.ToString();
            string response_status_text = HttpCodeToString(httpCode);
            string initialSend = $"{response_proto} {response_status} {response_status_text}";
            string toSend = initialSend + "\n" + response_headers_raw + "\n" + response_body_raw;
            WriteClient(client, toSend);
        }

        public static void Log(string message, Logging.LogSeverity severity = Logging.LogSeverity.Debug)
        {
            Logging.LogMessage mg = new Logging.LogMessage(severity, message, "Web");
            Logging.Log(mg);
        }

        /// <summary>
        /// Handles a connection's HTTP GET requests.
        /// </summary>
        private void HandleClientRequest(TcpClient client, IPEndPoint ipEnd, string request)
        {
            //RespondHTTP(client, "<label>No response content</label>", 500, pageTitle: "Error");

            string[] perLine = request.Split('\n');
            // example: 'GET / HTTP/1.1'
            string pathWanted = perLine[0];
            string authorization = "";
            foreach (string line in perLine)
            {
                if (line.StartsWith("Authorization"))
                {
                    // Gets the authorisation.
                    // Ie, username/password.
                    // This doesnt seem to work for me, but i've included it anyway
                    string[] authSplit = line.Split(' ');
                    byte[] data = Convert.FromBase64String(authSplit[2]);
                    string decodedString = Encoding.UTF8.GetString(data);
                    authorization = decodedString;
                    break;
                }
            }
            string[] pathSplit = pathWanted.Split(' '); // since URL's shouldnt contain spaces..
            pathWanted = pathSplit[1];
            pathWanted = Uri.UnescapeDataString(pathWanted); // now we decode the url, so it may contain spaces
            string pathUntilTokens = pathWanted;
            Log(ipEnd.ToString() + " requested " + pathWanted + $"{(authorization == "" ? "" : "  -Auth: " + authorization)}");
            Dictionary<string, string> tokens = new Dictionary<string, string>();
            if (pathWanted.Contains("?"))
            { // finds dictionary of any 'tokens' ie:
                // http://127.0.0.1/?name=Dave&job=unemployed
                // will return a dictionary of:
                // "name":"Dave"; "job":"unemployed"
                pathUntilTokens = pathWanted.Substring(0, pathWanted.IndexOf("?"));
                string pathWithTokens = pathWanted.Replace(pathUntilTokens, "");
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
                        tokens.Add(tokenString, "");
                        continue;
                    }
                    string name = tokenString.Substring(0, tokenString.IndexOf("="));
                    string value = tokenString.Replace(name + "=", "");
                    tokens.Add(name, value);
                }
            }
            if(tokens.TryGetValue("accn", out string accountName) && tokens.TryGetValue("lName", out string lastName) && tokens.TryGetValue("tutor", out string tutor))
            { // information is parsed, and placed into auth string
                authorization = $"{accountName} {lastName}-{tutor}";
            }
            User currentStudent = null;
            if(pathUntilTokens == "/")
            { // only runs on a page that might need authentification - Which for now is just the landing page
                if (string.IsNullOrWhiteSpace(authorization))
                { // they have not given any authorisation at all - so we respond with a nice form to do so
                    RespondHTTP(client, Properties.Resources.WebAuthentificationPage, 401);
                    client.Close();
                    return;
                } else
                { // they did give auth
                    string[] split = authorization.Split(' ');
                    accountName = split[0];
                    lastName = split[1].Split('-')[0];
                    tutor = split[1].Split('-')[1];
                    var student = (Program.Database.AllStudents.Values.FirstOrDefault(x => x.AccountName == accountName && x.LastName == lastName && tutor.ToLower() == x.Tutor.ToLower()));
                    if (student == null)
                    { // but the auth is incorrect - wrong / no student
                        RespondHTTP(client, "<label class=\"error\"Your authentification failed<br>Unknown user with given account name, last name and tutor.<br>Try again.<br>Case matters.", 401);
                        client.Close();
                        return;
                    }
                    currentStudent = student;
                }
                var curIP = ipEnd.Address.ToString();
                if(curIP == Program.GetLocalIPAddress())
                {
                    curIP = "127.0.0.1";
                }
                // in order to prevent abuse, we only allow them to see the votes IF:
                // - They have voted just now (ie, since the server started)
                // - They are requesting from the same IP as they voted
                // If the two abvoe are not met, then the request is refused, as below
                // HOWEVER: We override the above is the student has a flag indicating they are to always be permitted to see the vote.
                if (
                    (currentStudent == null || 
                    !SocketHandler.CachedKnownIPs.ContainsKey(currentStudent.AccountName) || 
                    SocketHandler.CachedKnownIPs[currentStudent.AccountName].ToString() != curIP || 
                    currentStudent.Flags.Contains(Flags.Disallow_View_Online)) 
                    && !currentStudent.Flags.Contains(Flags.View_Online))
                {
                    RespondHTTP(client, "<label class=\"error\">Authentification failed<br>Any of the following may apply:<br> - You entered incorrect name/information<br> - You have not logged in / voted today <br> - You voted from another computer <br> - You may not be permitted to see your vote", 403, pageTitle: "Forbidden");
                    client.Close();
                    return;
                }
            }
            List<string> urlPath = pathUntilTokens.Split('/').Where(x => String.IsNullOrWhiteSpace(x) == false).ToList();
            string RESPONSE_BODY = "<label>An internal error occured while attempting to process your request</label>";
            string RESPONSE_TITLE = "Y11 Awards"; // default valuess
            int RESPONSE_CODE = 500;
            var headers = new Dictionary<string, string>();
            bool forceIgnoreWebpage = false;
            try
            {
                if(pathUntilTokens == "/")
                {
                    // no specific page (ie: https://127.0.0.1/)
                    RESPONSE_TITLE = "Y11 Awards";

                    string text = "<label>Your votes for each category:</label>";
                    text += $"<table><tr><th>Category</th><th>First</th><th>Second</th></tr>";
                    int catCount = 0;
                    foreach(var category in Program.Database.AllCategories.Values)
                    { // builds information into a table, for display.
                        catCount++;
                        string clrClass = (catCount % 2 == 0) ? " class=\"tblEven\"" : "";
                        string catText = "<tr" + clrClass + "><td>{0}</td><td>{1}</td><td>{2}</td></tr>";
                        var users = category.GetVotesBy(currentStudent);
                        catText = string.Format(catText, category.Prompt, users.Item1?.FullName ?? "N/A", users.Item2?.FullName ?? "N/A");
                        text += catText;
                    }
                    text += "</table>";
                    RESPONSE_BODY = text; RESPONSE_CODE = 200;
                } else if (pathUntilTokens == "/WebStyles.css")
                { // returns web styling
                    RESPONSE_BODY = Properties.Resources.WebStyles;
                    RESPONSE_CODE = 200;
                    forceIgnoreWebpage = true; // without any HTML formatting
                    headers.Add("Content-Type", "text/css");
                } else if (pathUntilTokens == "/WebScripts.js")
                { // as with the CSS above
                    RESPONSE_BODY = Properties.Resources.WebScripts;
                    RESPONSE_CODE = 200;
                    forceIgnoreWebpage = true;
                    headers.Add("Content-Type", "text/javascript");
                } else if(pathUntilTokens == "/all")
                {
                    string cssClass = (ipEnd.Address.ToString() == Program.GetLocalIPAddress() || ipEnd.Address.ToString() == "127.0.0.1" || Program.Options.Allow_NonLocalHost_WebConnections) ? "" : "class=\"hidden\"";
                    // anyone can access the info
                    // but, we hide some data via css
                    // which isnt particularly secure.. might change in future.
                    // however: I have set the javascript to remove any information within a [REDACTED] element
                    // which means that it is in no way secure - its still being sent
                    // BUT it is impossible to get the data using just Inspect element, which will prevent access from most/all

                    RESPONSE_CODE = 200; RESPONSE_TITLE = "Y11: All Data";
                    string htmlPage = Properties.Resources.WebAllDataPage; // this is the base template, which data will be added into
                    int notVoted = Program.Database.AllStudents.Count; // start with all students added
                    int currentlyVoting = 0;
                    int alreadyVoted = 0;
                    foreach(var student in Program.Database.AllStudents.Values)
                    { // removes them as needed to increment the two above
                        if(Program.Database.AlreadyVotedNames.Contains(student.AccountName))
                        {
                            notVoted--;
                            alreadyVoted++;
                        } else if (SocketHandler.CurrentClients.FirstOrDefault(x => x.User.AccountName == student.AccountName) != null)
                        {
                            notVoted--;
                            currentlyVoting++;
                        }
                    }
                    // Displays how many total votes have been made, and how many unique people have been voted, for each category.
                    string categoryTable = "<table><tr><th>Category</th><th>Total Votes (by people)</th><th>Unique Voted (num people voted)</th></tr>";
                    foreach(var category in Program.Database.AllCategories.Values)
                    {
                        int votes = 0;
                        foreach(var list in category.Votes)
                        {
                            votes += list.Value.Count;
                        }
                        string tempClass = (category.ID % 2 == 0) ? "class=\"tblEven\"" : "";
                        string format = $"<tr {tempClass}><td>{category.Prompt}</td><td {cssClass}>{votes}</td><td {cssClass}>{category.Votes.Count}</td></tr>";
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
                    foreach(var keypair in ReplaceValues) { htmlPage = htmlPage.Replace(keypair.Key, keypair.Value); }

                    RESPONSE_BODY = htmlPage;
                }
                else
                { // unknown/not set up request
                    RESPONSE_BODY = "<label class=\"error\">404 - Unkown request.</label>"; RESPONSE_CODE = 404;
                }
            } catch (Exception ex)
            {
                Log(ex.ToString(), Logging.LogSeverity.Error);
            } finally
            { // always always always respond in atleast some way
                RespondHTTP(client, RESPONSE_BODY, RESPONSE_CODE, (headers.Count == 0 ? null : headers), RESPONSE_TITLE, forceIgnoreWebpage);
            }





        }

        private void ListenNewClients()
        {
            TcpClient clientSocket = new TcpClient();
            while(WebServer != null)
            {
                try
                {
                    clientSocket = WebServer.AcceptTcpClient();
                    Byte[] bytesFrom = new Byte[clientSocket.ReceiveBufferSize];
                    string dataFromClient;
                    NetworkStream netStream = clientSocket.GetStream();
                    try
                    {
                        netStream.Read(bytesFrom, 0, Convert.ToInt32(clientSocket.ReceiveBufferSize));
                    }
                    catch (Exception ex)
                    {
                        Log(ex.ToString(), Logging.LogSeverity.Error);
                    }
                    dataFromClient = Encoding.UTF8.GetString(bytesFrom).Trim().Replace("\0", "");
                    IPEndPoint ipEnd = clientSocket.Client.RemoteEndPoint as IPEndPoint;
                    if (string.IsNullOrWhiteSpace(dataFromClient))
                    {
                        WriteClient(clientSocket, "400 " + HttpCodeToString(400));
                        clientSocket.Close();
                        continue;
                    }
                    if(dataFromClient.StartsWith("GET"))
                    { // HTTP requests may come in a few varities: we are only interested in GET requests
                        HandleClientRequest(clientSocket, ipEnd, dataFromClient);
                    } else
                    { // so, we error on any others
                        WriteClient(clientSocket, "400 " + HttpCodeToString(400));
                    }
                } catch (Exception ex)
                {
                    Log(ex.ToString(), Logging.LogSeverity.Error);
                }
            }
        }

        public WebsiteHandler()
        {
            if (_started)
                throw new InvalidOperationException("Webserver has already been started");
            _started = true;
            WebServer = new TcpListener(IPAddress.Any, 80);
            WebServer.Start();
            Thread listen = new Thread(ListenNewClients);
            listen.Start();
        }

    }
}
