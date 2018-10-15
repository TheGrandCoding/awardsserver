using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;

// == Code Owner
// Abdul
// == 

namespace AwardsServer
{ 
    public class DatabaseStuffs
    {
        public Dictionary<string, User> AllStudents = new Dictionary<string, User>(); // would be AccountName:User again
        public Dictionary<int, Category> AllCategories = new Dictionary<int, Category>(); // int is the Category's ID.

        public List<string> AlreadyVotedNames = new List<string>();

        public static OleDbConnection connection = new OleDbConnection();
        public void Connect()
        {
            string path = @"Provider = Microsoft.ACE.OLEDB.12.0;Data Source = DataBase.accdb; Persist Security Info = False;";
            connection.ConnectionString = path;
        }

        public void Disconnect()
        {
            connection.Close();
        }

        private void LoadCategories()
        {
            OleDbCommand command = new OleDbCommand();
            command.Connection = connection;
            command.CommandText = "select * from CategoryData";
            OleDbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Category cat = new Category(reader["Prompt"].ToString(), int.Parse(reader["ID"].ToString()));
                AllCategories.Add(cat.ID, cat);
            }
        }

        public void Load_All_Votes()
        {
            // this should read from a database
            // and read all of the user's themselves,
            // then load all of the votes etc.
            // place them into the classes above, then return it.
            // so essentially: this is returning all of the categories, with existing votes already placed into them.
            // (it should also load the AllStudents and AllCategories lists..)
            if(connection.State == System.Data.ConnectionState.Closed)
            {
                try
                {
                    connection.Open();
                }
                catch (OleDbException ex)
                {
                    if (ex.Message.Contains("Could not find file"))
                    {
                        try
                        {
                            System.IO.File.WriteAllBytes("Database.accdb", AwardsServer.Properties.Resources.EmptyDatabase);
                            try
                            {
                                connection.Open();
                                Logging.Log(Logging.LogSeverity.Severe, "Created a new empty database.\r\nYou will need to add students to it before this server will start.");
                                return; // we dont want it to do anymore here.
                            }
                            catch (Exception nextEx)
                            {
                                Logging.Log(Logging.LogSeverity.Severe, "After attempting to create an empty file, still errored: " + nextEx.ToString());
                                return;
                            }
                        }
                        catch (Exception exx)
                        {
                            Logging.Log(Logging.LogSeverity.Severe, exx.ToString());
                            return;
                        }
                    }
                    else
                    {
                        Logging.Log(Logging.LogSeverity.Severe, ex.ToString());
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logging.Log(Logging.LogSeverity.Severe, ex.ToString());
                    return;
                }
            }
            AllCategories = new Dictionary<int, Category>();
            AllStudents = new Dictionary<string, User>();
            LoadCategories();
            OleDbCommand command = new OleDbCommand();
            command.Connection = connection;
            command.CommandText = "select * from UserData";
            OleDbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                User user = new User(reader["UserName"].ToString(), reader["FirstName"].ToString(), reader["LastName"].ToString(), reader["Tutor"].ToString(), char.Parse(reader["Sex"].ToString()));
                if(user.AccountName.Length != "cheale14".Length)
                {
                    Logging.Log(Logging.LogSeverity.Warning, "User " + user.ToString("FN LN TT SX AN") + " has invalid account name");
                }
                AllStudents.Add(user.AccountName, user); 
            }
            OleDbCommand command2 = new OleDbCommand();
            command2.Connection = connection;
            foreach(var cat in AllCategories.Values) //looping through every table
            {
                command2.CommandText = $"select * from Category{cat.ID}"; //selecting the *table* (not column)
                OleDbDataReader reader2 = null;
                try
                {
                    reader2 = command2.ExecuteReader(); //see if the table exists
                }
                catch (System.Data.OleDb.OleDbException ex)
                {
                    if(ex.Message.Contains("cannot find the input table or query"))
                    {
                        // table is missing
                        Logging.Log(Logging.LogSeverity.Warning, "Database table for category " + cat.ID + " missing, attempting to create..");
                        OleDbCommand tableCommand = new OleDbCommand();
                        tableCommand.Connection = connection;
                        tableCommand.CommandText = $"create table Category{cat.ID} (UserName varchar(255), VotedFor varchar(255), TimeVoted varchar(103));";
                        tableCommand.ExecuteNonQuery();
                        reader2 = command2.ExecuteReader();
                    }
                }
                while (reader2.Read())
                {
                    User VotedBy = null;
                    User UserVotedFor = null;
                    Program.TryGetUser(reader2["UserName"].ToString(), out VotedBy);
                    Program.TryGetUser(reader2["VotedFor"].ToString(), out UserVotedFor);
                    if(VotedBy == null)
                    {
                        Logging.Log(Logging.LogSeverity.Error, $"User '{reader2["UserName"]}' changed, discarding vote for '{reader2["VotedFor"]}' in category {cat.ID}");
                        continue;
                    }
                    if (UserVotedFor == null)
                    {
                        Logging.Log(Logging.LogSeverity.Error, $"User '{reader2["VotedFor"]}' changed, discarding vote by '{reader2["UserName"]}' in category {cat.ID}");
                        continue;
                    }
                    AlreadyVotedNames.Add(VotedBy.AccountName);
                    cat.AddVote(UserVotedFor, VotedBy);
                }
                reader2.Close();
            }
        }
        private readonly object _databaseLock = new object();
        public void ExecuteCommand(string cmd)
        {
            lock(_databaseLock)
            {
                // probably not very good to be able to do this but hey..
                if (connection.State == System.Data.ConnectionState.Closed && connection.State != System.Data.ConnectionState.Connecting)
                { // this technically shouldnt really be ran, considering it doesnt close it above.
                    Connect(); // just to be safe..
                    connection.Open();
                }
                OleDbCommand command = new OleDbCommand();
                command.Connection = connection;
                command.CommandText = cmd;
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Updates database to add a vote for a person in a given category
        /// </summary>
        /// <param name="categoryID">Category's ID</param>
        /// <param name="voted">Who's name was given to be voted for</param>
        /// <param name="votedBy">Who has actually done the vote</param>
        public void AddVoteFor(int categoryID, User voted, User votedBy)
        {
            if(AllCategories.TryGetValue(categoryID, out Category category))
            {
                category.AddVote(voted, votedBy);
            } else
            {
                throw new ArgumentException("Unknown category id: " + categoryID.ToString(), "categoryID");
            }
            ExecuteCommand($"insert into Category{category.ID} (UserName , VotedFor, TimeVoted) values ('{votedBy.AccountName}','{voted.AccountName}','{DateTime.Now}')");
        }
    }
}

