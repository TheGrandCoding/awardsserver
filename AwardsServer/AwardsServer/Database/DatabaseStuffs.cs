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
            connection.Open();
            LoadCategories();
            OleDbCommand command = new OleDbCommand();
            command.Connection = connection;
            command.CommandText = "select * from UserData";
            OleDbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                User user = new User(reader["UserName"].ToString(), reader["FirstName"].ToString(), reader["LastName"].ToString(), reader["Tutor"].ToString(), char.Parse(reader["Sex"].ToString()));
                AllStudents.Add(user.AccountName, user);
            }
            OleDbCommand command2 = new OleDbCommand();
            command2.Connection = connection;
            foreach(var cat in AllCategories.Values)
            {
                command2.CommandText = $"select * from Category{cat.ID}";
                OleDbDataReader reader2 = null;
                try
                {
                    reader2 = command2.ExecuteReader();
                }
                catch (System.Data.OleDb.OleDbException ex)
                {
                    if(ex.Message.Contains("cannot find the input table or query"))
                    {
                        // table is missing
                        Logging.Log(Logging.LogSeverity.Warning, "Database table for category " + cat.ID + " missing, attempting to create..");
                        OleDbCommand tableCommand = new OleDbCommand();
                        tableCommand.Connection = connection;
                        tableCommand.CommandText = $"create table Category{cat.ID} (UserName varchar(255), VotedFor varchar(255));";
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
                        Logging.Log(Logging.LogSeverity.Error, $"User '{reader2["UserName"]}' changed, disgarding vote for '{reader2["VotedFor"]}' in category {cat.ID}");
                        continue;
                    }
                    if (UserVotedFor == null)
                    {
                        Logging.Log(Logging.LogSeverity.Error, $"User '{reader2["VotedFor"]}' changed, disgarding vote by '{reader2["UserName"]}' in category {cat.ID}");
                        continue;
                    }
                    AlreadyVotedNames.Add(VotedBy.AccountName);
                    cat.AddVote(UserVotedFor, VotedBy);
                }
                reader2.Close();
            }
            connection.Close();
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
            connection.Open();
            OleDbCommand command = new OleDbCommand();
            command.Connection = connection;
            command.CommandText = $"insert into Category{category.ID} (UserName , VotedFor) values ('{votedBy.AccountName}','{voted.AccountName}')";
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}

