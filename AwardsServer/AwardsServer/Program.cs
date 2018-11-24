using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.InteropServices;

//get ready for some seemingly obvious questions
//ctrl-f "??" to find what might be confusing
namespace AwardsServer
{
    public class Program
    {
        public static ServerUI.UIForm ServerUIForm;
        public static SocketHandler Server; // Handles the connection and essentially interfaces with the TCP-side of things
        public static DatabaseStuffs Database; // database related things

        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)] //??
        public class OptionAttribute : Attribute //what is this for?? does it 'Constructs the information for an Option.'?
        {
            public readonly string Name;
            public readonly string Description;
            public readonly object DefaultValue;
            /// <summary>
            /// Constructs the information for an Option.
            /// </summary>
            /// <param name="description">Displayed on UI Form: what this option does</param>
            /// <param name="name">Internal/short name for this option</param>
            /// <param name="defaultValue">Default value for the option</param>
            public OptionAttribute(string description, string name, object defaultValue)
            {
                Name = name;
                Description = description;
                DefaultValue = defaultValue;
            }
        }
        public static class Options
        {
            [Option("Maximum number of students to list in a name query response", "Max students for query", 10)]
            public static int Maximum_Query_Response;

            [Option("Is the same username permitted to be connected at the same time", "Allow identical usernames", false)]
            public static bool Simultaneous_Session_Allowed;

            [Option("Allow student data to be modified even after someone joins", "Allow data modify", false)]
            public static bool Allow_Modifications_When_Voting;

            [Option("Maximum before queue begins.", "Queue threshhold", 15)]
            public static int Maximum_Concurrent_Connections;

            [Option("Time (in seconds) between each heartbeat message is sent", "Time (s) between heartbeat", 5)]
            public static int Time_Between_Heartbeat;

            [Option("Whether it should display when a message is recieved", "Whether console shows message recieved", true)]
            public static bool Display_Recieve_Client;

            [Option("Whether it should display when a message is sent", "Whether console shows sent messages", true)]
            public static bool Display_Send_Client;

            [Option("Any severity below this is not shown in the UI", "Lowest severity displayed", Logging.LogSeverity.Debug)]
            public static Logging.LogSeverity Only_Show_Above_Severity;
        }

        private const string MainRegistry = "HKEY_CURRENT_USER\\AwardsProgram\\Server";
        public static void SetOption(string key, string value) //??
        {
            Microsoft.Win32.Registry.SetValue(MainRegistry, key, value); //??
        }
        public static T Convert<T>(string input) //converts one object type to another ??
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    // Cast ConvertFromString(string text) : object to (T)
                    return (T)converter.ConvertFromString(input);
                }
                return default(T);
            }
            catch (NotSupportedException)
            {
                return default(T);
            }
        }
        /*public static T GetOption<T>(string key, T defaultValue)
        {
            var item = Microsoft.Win32.Registry.GetValue(MainRegistry, key, defaultValue);
            if (item == null)
                item = defaultValue;
            return Convert<T>(item.ToString());
        }*/
        public static string GetOption(string key, string defaultValue) //returns... option as a string??
        {
            var item = Microsoft.Win32.Registry.GetValue(MainRegistry, key, defaultValue);
            if (item == null)
                return defaultValue;
            return (string)item;
        }

        public static bool TryGetUser(string username, out User user) //checks if the user exits (+assigns them to 'user' if true)??
        {
            user = null;
            if(Database.AllStudents.ContainsKey(username))
            {
                user = Database.AllStudents[username];
                return true;
            }
            return false;
        }
        public static User GetUser(string username) //the above checks if the user exists, this returns the user
        {
            TryGetUser(username, out User user);
            return user;
        }


        // Console window closing things:
        private delegate bool ConsoleEventDelegate(int eventType); //?? i mean this is a new level of ??
        [DllImport("kernel32.dll", SetLastError = true)] //??
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add); //??
        static ConsoleEventDelegate handler; //handles... anything??
        static bool ConsoleEventCallback(int eventType) //why is this used ??
        {
            if (eventType == 2) //what's event type 2?? -it's the window closing right
            {
                // code to run here
                Logging.Log(new Logging.LogMessage(Logging.LogSeverity.Severe, "Console window closing..")); 
                try
                {
                    Database.Disconnect();
                } catch (Exception ex)
                {
                    Logging.Log("CloseConn", ex);
                }
            }
            return false;
        }

        static void Main(string[] args)
        {
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true); // this line & above handle the console window closing
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException; //??
            Logging.Log(Logging.LogSeverity.Info,  "Loading existing categories...");
            Database = new DatabaseStuffs();
            Database.Connect();
            Database.Load_All_Votes();
            if(Database.AllStudents.Count == 0)
            {
                Logging.Log(Logging.LogSeverity.Error, "No students have been loaded. Assuming that this is an error.");
                Console.ReadLine();
                Logging.Log(Logging.LogSeverity.Error, "This error will continue to occur until atleast one student is added to the 'Database.accdb' file");
                Console.ReadLine();
                return; // closes
            }
#if DEBUG
            var st = new User(Environment.UserName.ToLower(), "Local", "Host", "1010", 'M');
            if (!Database.AllStudents.ContainsKey(st.AccountName)) //if the user is not in the database
                Database.AllStudents.Add(st.AccountName, st); //add the user
#endif

            Logging.Log($"Loaded {Database.AllStudents.Count} students and {Database.AllCategories.Count} categories.");

            Logging.Log("Starting socket listener...");
            Server = new SocketHandler();
            Logging.Log("Started. Ready to accept new connections.");
            

            // Open UI form..
            System.Threading.Thread uiThread = new System.Threading.Thread(runUI);
            uiThread.Start();

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
        { //logs any unhandled exceptions
            Logging.Log(new Logging.LogMessage(Logging.LogSeverity.Severe, "Unhandled", (Exception)e.ExceptionObject));
        }
        private static void runUI()
        {
            bool first = false; //??why
            while (Server.Listening)
            {
                if(ServerUIForm != null) //if the form is open??
                {
                    ServerUIForm.Dispose(); // close it??
                }
                ServerUIForm = new ServerUI.UIForm();
                if(!first) //does this switch between allowing the user to edit and not ??
                {
                    first = true;
                    ServerUIForm.PermittedStudentEdits(ServerUI.UIForm.EditCapabilities.All);
                } else
                {
                    ServerUIForm.PermittedStudentEdits(ServerUI.UIForm.EditCapabilities.None);
                }
                ServerUIForm.ShowDialog();
                Logging.Log(Logging.LogSeverity.Debug, "UI Form closed; reopening to regenerate data. If you want to close the server, close the Console");
            }
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
        public bool HasVoted => Program.Database.AlreadyVotedNames.Contains(AccountName);
        public string FullName => FirstName + " " + LastName;

        public User(string accountName, string firstName, string lastName, string tutor, char sex) 
        {//creating a new user
            AccountName = accountName;
            FirstName = firstName;
            LastName = lastName;
            Tutor = tutor;
            if(!(sex == 'F' || sex == 'M'))
            {
                throw new ArgumentException("Must be either 'F' or 'M'", "sex"); //its 2018 lol jk
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
        public Category(string prompt, int id = -1)
        {
            ID = System.Threading.Interlocked.Increment(ref __id);
            if (id > -1)
            { // allows you to manually set the ID (eg, from database)
                ID = id;
            }
            Votes = new Dictionary<string, List<User>>();
            Prompt = prompt;
        }
        public Dictionary<string, List<User>> Votes; // key: AccountName of user, list is all the users that voted for that person.

        /// <summary>
        /// Returns the keys of the Votes dict from highest to lowest.
        /// </summary>
        /// <returns></returns>
        public List<string> SortVotes(char sex) //in ascending order
        {
            var sortedDict = from entry in Votes where Program.GetUser(entry.Key).Sex == sex orderby entry.Value.Count ascending select entry.Key;
            // yay for linq.
            return sortedDict.ToList();
        }

        /// <summary>
        /// Returns the person with the highest vote, or the list of people tied to the highest vote
        /// </summary>
        /// <param name="sex">'M' or 'F'</param>
        public Tuple<List<User>, int> HighestVoter(char sex) //returns the most voted for person
        {
            List<User> tied = new List<User>();
            int highest = 0;
            var sorted = this.SortVotes(sex);
            foreach(var u in sorted) //necessary in case there's a tie
            { // could you make a descending list, and stop looping after the num of votes is lower than the first user's (less loops)?
                if(this.Votes[u].Count > highest)
                {
                    highest = this.Votes[u].Count;
                    Program.TryGetUser(u, out User highestU);
                    tied = new List<User>(); // need to reset
                    tied.Add(highestU);
                } else if (this.Votes[u].Count == highest)
                {
                    Program.TryGetUser(u, out User hig);
                    tied.Add(hig);
                }
            }
            Tuple<List<User>, int> returns = new Tuple<List<User>, int>(tied, highest);
            return returns;
        }

        /// <summary>
        /// Adds the vote specified, creating a new Dictionary entry if needed
        /// </summary>
        /// <param name="voted">Who was nominated</param>
        /// <param name="votedBy">Person that was doing the voting.</param>
        public void AddVote(User voted, User votedBy) //add a vote to 'voted'
        {
            if (voted.AccountName == votedBy.AccountName)
            {
                throw new ArgumentException("Both users are the same object, or share the same name");
            }
            if(Votes.ContainsKey(voted.AccountName)) 
            {
                Votes[voted.AccountName].Add(votedBy); //add a vote to an existing voter
            } else

            {
                Votes.Add(voted.AccountName, new List<User>() { votedBy }); //create a new voter + add their vote
            }
        }
        public override string ToString()
        {
            return $"{ID}: {Votes.Count} {Prompt}";
        }
    }
}
