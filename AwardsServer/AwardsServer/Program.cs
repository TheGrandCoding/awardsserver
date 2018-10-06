using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwardsServer
{
    public class Program
    {
        public static SocketHandler Server;
        public static DatabaseStuffs Database;
        // TODO stuff

        public static class Options
        {
            /// <summary>
            /// Maximum number of students to list in a name query response
            /// </summary>
            public static int Maximum_Query_Response = 10;

            /// <summary>
            /// Prevent a user from connecting twice.
            /// </summary>
            public static bool Disallow_Simultaneous_Sessions = true;

            /// <summary>
            /// Maximum number of simultaneous users before the next one is put in a queue.
            /// </summary>
            public static int Maximum_Concurrent_Connections = 99;
        }

        public static bool TryGetUser(string username, out User user)
        {
            user = null;
            if(Database.AllStudents.ContainsKey(username))
            {
                user = Database.AllStudents[username];
                return true;
            }
            return false;
        }

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Logging.Log(Logging.LogSeverity.Info,  "Loading existing categories...");
            Database = new DatabaseStuffs();
            Database.Load_All_Votes();
            foreach(var c in Database.AllCategories)
            {
                string msg = $"{c.Value.ID} - {c.Value.Prompt}";
                foreach(var vote in c.Value.Votes)
                {
                    msg += $"\r\n{vote.Key} - {vote.Value.Count} votes";
                }
                Logging.Log(Logging.LogSeverity.Info, msg);
            }
            Logging.Log("Starting...");
            Server = new SocketHandler();
            Logging.Log("Started. Ready to accept new connections.");
            // some minor testing things below
                

            if(TryGetUser("jakepaul", out User user))
            {
                if(TryGetUser("smith101", out User otherUser))
                {
                    try
                    {
                        Database.AddVoteFor(1, user, otherUser);
                    } catch (Exception ex)
                    {
                        Logging.Log("Testing", ex);
                    }
                }
            }
            while(Server.Listening)
            {
                Console.ReadLine();
            }
            Logging.Log(Logging.LogSeverity.Severe, "Server has exited its main listening loop");
            Logging.Log(Logging.LogSeverity.Error, "Server closed.");
            while(true)
            { // pause at end so they can read console
                Console.ReadLine();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logging.Log(new Logging.LogMessage(Logging.LogSeverity.Severe, "Unhandled", (Exception)e.ExceptionObject));
        }
    }
    // Shared stuff that will be used across multiple files.
    public class User
    {
        public readonly string AccountName; // eg 'cheale14'
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Tutor;
        public readonly char Sex;
        public User(string accountName, string firstName, string lastName, string tutor, char sex)
        {
            AccountName = accountName;
            FirstName = firstName;
            LastName = lastName;
            Tutor = tutor;
            if(!(sex == 'F' || sex == 'M'))
            {
                throw new ArgumentException("Must be either 'F' or 'M'", "sex");
            }
            Sex = sex;
        }
        public override string ToString()
        {
            return this.ToString("AN: FN LN (TT)"); // $"{AccountName}: {FirstName} {LastName} ({Tutor})";
        }
        /// <summary>
        /// AN = Account Name
        /// FN = First Name
        /// LN = Last Name
        /// TT = Tutor
        /// SX = Sex
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public string ToString(string format)
        {
            format = format.Replace("AN", "{0}");
            format = format.Replace("FN", "{1}");
            format = format.Replace("LN", "{2}");
            format = format.Replace("TT", "{3}");
            format = format.Replace("SX", "{4}");
            return string.Format(format, this.AccountName, this.FirstName, this.LastName, this.Tutor, this.Sex);
        }
    }
    public class Category
    {
        public readonly int ID; // each category should have a integer assigned (from 1 to 15 for example)
        public readonly string Prompt; // eg 'most likely to become Prime Minister'
        private static int __id = 0;
        public Category(string prompt)
        {

            ID = System.Threading.Interlocked.Increment(ref __id);
            Votes = new Dictionary<string, List<User>>();
            Prompt = prompt;
        }
        public Dictionary<string, List<User>> Votes; // key: AccountName of user, list is all the users that voted for that person.

        /// <summary>
        /// Adds the vote specified, creating a new Dictionary entry if needed
        /// </summary>
        /// <param name="voted">Who was nominated</param>
        /// <param name="votedBy">Person that was doing the voting.</param>
        public void AddVote(User voted, User votedBy)
        {
            if (voted.AccountName == votedBy.AccountName)
                throw new ArgumentException("Both users are the same object, or share the same name");
            if(Votes.ContainsKey(voted.AccountName))
            {
                Votes[voted.AccountName].Add(votedBy);
            } else
            {
                var list = new List<User>() { votedBy };
                Votes.Add(voted.AccountName, list);
            }
        }
        public override string ToString()
        {
            return $"{ID}: {Votes.Count} {Prompt}";
        }
    }
}
