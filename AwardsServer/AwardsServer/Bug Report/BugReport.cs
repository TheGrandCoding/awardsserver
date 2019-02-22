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
    public class BugReport
    {
        public string Primary;
        public BugReportType Type;
        public string Additional;

        public string LogFile;

        [JsonIgnore]
        public User Reporter;
        [JsonProperty]
        private string reporterName => Reporter?.AccountName ?? "";
        [JsonIgnore]
        public GithubDLL.Entities.Issue Issue;
        private int issueId => Issue?.Number ?? 0;
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
                split[3],
                reporter);
            rp.internalId = System.Threading.Interlocked.Increment(ref _internalCounter);
            return rp;
        }

        [JsonConstructor]
        private BugReport(int id, string reportername, bool solved)
        {
            if (id >= 70)
                    Issue = Program.AwardsRepository.GetIssue(id);
            else
            {
                _internalCounter = id;
                this.internalId = id;
            }
            Reporter = Program.GetUser(reportername);
        }

        public BugReport(BugReportType type, string primary, string additional, string logBase64Encoded, User reporter)
        {
            Type = type;
            Primary = primary;
            Additional = additional;
            Reporter = reporter;
            string decodedLog = Encoding.UTF8.GetString(Convert.FromBase64String(logBase64Encoded));
            decodedLog = $"------------------\r\nNew bug report:\r\nType: {type}\r\nPrimary: {primary}\r\nAdditional: {additional}\r\n------------------\r\n" + decodedLog;
            string fileName = $"{DateTime.Now.ToString("yyyyMMdd")}_{reporter.AccountName}.txt";
            string path = Program.Options.Client_Bug_Logs_Folder_Path + fileName;
            System.IO.File.AppendAllText(path, decodedLog);
            LogFile = path;
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
