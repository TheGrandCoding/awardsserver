using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Newtonsoft.Json;
using GithubDLL;
using System.Text.RegularExpressions;

namespace AwardsServer.BugReport
{
    public class GithubService
    {
        public static GithubDLL.GithubClient Client;
        public const string RepoRegex = @"(?<=repos)\/.*\/.*";
        public const string IssueFindRegex = @"\S*\/\S*#\d+";
        public static List<GithubIssue> GetIssues(string input)
        {
            List<GithubIssue> issues = new List<GithubIssue>();
            var regex = new Regex(IssueFindRegex);
            var match = regex.Matches(input);
            foreach (Match mat in match)
            {
                var text = mat.Value;
                string[] split = text.Split('/');
                var owner = split[0];
                string[] secondSplit = split[1].Split('#');
                var repo = secondSplit[0];
                var id = secondSplit[1];
                var issue = Client.GetAndParse<GithubIssue>($"repos/{owner}/{repo}/issues/{id}");
                issues.Add(issue);
            }
            return issues;
        }
        public GithubService(string auth, string agent)
        {
            Client = new GithubClient(auth, agent);
            string contents = "";
            try
            {
                contents = System.IO.File.ReadAllText("bugreports.json");
            }
            catch (System.IO.FileNotFoundException)
            {
                System.IO.File.WriteAllText("bugreports.json", "");
            }
            Program.BugReports = JsonConvert.DeserializeObject<List<BugReport>>(contents);
        }
        public void Save()
        {
            System.IO.File.WriteAllText("bugreports.json", JsonConvert.SerializeObject(Program.BugReports));
        }
    }
    public class BugReport
    {
        public string Primary;
        public BugReportType Type;
        public string Additional;
        [JsonIgnore]
        public User Reporter;
        [JsonProperty]
        private string reporterName => Reporter?.AccountName ?? "";
        [JsonIgnore]
        public GithubDLL.GithubIssue Issue;
        private int issueId => Issue?.number ?? 0;
        public bool Solved;
        public bool Submitted => issueId != 0;

        static int _internalCounter = 0;
        private int internalId;
        [JsonProperty]
        public int Id
        {
            get
            {
                return issueId == 0 ? internalId : issueId;
            }
            set
            {
                if (Submitted)
                    throw new ArgumentException("Unable to edit ID of Github issue");
                internalId = value;
            }
        }

        public static BugReport Parse(string input, User reporter)
        {
            if (input.StartsWith("REPORT:"))
                input = input.Replace("REPORT:", "");
            string[] split = input.Split(';');
            var rp = new BugReport(
                (BugReportType)Enum.Parse(typeof(BugReportType), split[0]),
                split[1],
                split[2], 
                reporter);
            rp.internalId = System.Threading.Interlocked.Increment(ref _internalCounter);
            return rp;
        }

        [JsonConstructor]
        private BugReport(int id, string reportername)
        {
            if(id >= 70) 
                Issue = GithubService.GetIssues("thegrandcoding/awardsserver#" + id.ToString()).FirstOrDefault();
            else
            {
                _internalCounter = id;
                this.internalId = id;
            }
 
            Reporter = Program.GetUser(reportername);
        }

        public BugReport(BugReportType type, string primary, string additional, User reporter)
        {
            Type = type;
            Primary = primary;
            Additional = additional;
            Reporter = reporter;
        }

        /*public GithubIssue Submit()    -- This is done in the UIForm.
        {
            var repo = GithubRepository.GetRepo(GithubService.Client, "thegrandcoding", "awardsserver");
            var issue = repo.CreateIssue(GithubService.Client, x =>
            {
                x.title = "Bug Report by " + Reporter.AccountName;
                x.body = $"**Type:** {this.Type}\r\n**Primary:** {this.Primary}\r\n**Additional text**\r\n{this.Additional}";
                x.labels = new string[] { "bug" };
            });
            return issue;
        }*/

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    public enum BugReportType
    {
        NotSubmitted=0,
        Other,
        User,
        Category
    }
}
