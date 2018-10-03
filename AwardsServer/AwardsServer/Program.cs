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
        public string AccountName; // eg 'cheale14'
        public string FirstName;
        public string LastName;
        public string Tutor;
        public override string ToString()
        {
            return $"{AccountName}: {FirstName} {LastName} ({Tutor})";
        }
    }
    public class Category
    {
        public readonly int ID; // each category should have a integer assigned (from 1 to 15 for example)
        public string Prompt; // eg 'most likely to become Prime Minister'
        private static int __id = 0;
        public Category()
        {
            ID = System.Threading.Interlocked.Increment(ref __id);
            Votes = new Dictionary<string, List<User>>();
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
