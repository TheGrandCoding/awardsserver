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
        public static List<Category> AllCategories = new List<Category>(); // int is the Category's ID.
        // TODO stuff
        static void Main(string[] args)
        {
            AddCatergories();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            while(true)
            {
                Logging.Log("Loading existing categories...");
                Database = new DatabaseStuffs();
                Database.AllCategories = AllCategories;
                Database.Connect();
                Database.Load_All_Votes();
                Logging.Log("Starting...");
                Server = new SocketHandler();
                Logging.Log("Started. Ready to accept new connections.");
                // some minor testing things below
                //var user = new User();
                //user.AccountName = "davsmi14";
                //user.FirstName = "Dave";
                //user.LastName = "Smith";
                //user.Tutor = "11BOB";
                //var category = new Category();
                //category.Prompt = "Most likely to become Prime Minister";
                //try
                //{
                //    category.AddVote(user, user);
                //}
                //catch (Exception ex)
                //{
                //    Logging.Log("StartUp", ex);
                //}
                try
                {

                }
                catch (Exception ex)
                {
                    Logging.Log("LoadCategory", ex);
                }

                Console.ReadLine();
            }

        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logging.Log(new Logging.LogMessage(Logging.LogSeverity.Severe, "Unhandled", (Exception)e.ExceptionObject));
        }
        private static  void AddCatergories()
        {
            Category category = new Category();
            category.ID = 1;
            category.Prompt = "Catergory1";
            AllCategories.Add(category);
        }
    }
    // Shared stuff that will be used across multiple files.
    public class User
    {
        public string AccountName; // eg 'cheale14'
        public string FirstName;
        public string LastName;
        public string Tutor;
        public string Sex;
        public Dictionary<Category,User> VotedFor = new Dictionary<Category,User>();
    }
    public class Category
    {
        public int ID; // each category should have a integer assigned (from 1 to 15 for example)
        public string Prompt; // eg 'most likely to become Prime Minister'
        public Dictionary<User, User> Votes= new Dictionary<User, User>(); // key:user that voted, Value : the person the user voted for.
        public Dictionary<User, User> InverseVotes = new Dictionary<User, User>(); // key=value and value = key
        /// <summary>
        /// Adds the vote specified, creating a new Dictionary entry if needed
        /// </summary>
        /// <param name="voted">Who was nominated</param>
        /// <param name="votedBy">Person that was doing the voting.</param>
        public void AddVote(User voted, User votedBy)
        {
            if (voted.AccountName == votedBy.AccountName)
            {
                throw new ArgumentException("Both users are the same object, or share the same name");
            }
            Votes.Add(votedBy, voted);
        }
    }
}
