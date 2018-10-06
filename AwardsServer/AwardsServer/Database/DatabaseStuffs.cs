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
        public List<User> AllStudents = new List<User>(); // would be AccountName:User again
        public List<Category> AllCategories = new List<Category>(); // int is the Category's ID.
        public static OleDbConnection connection = new OleDbConnection();
        public void Connect()
        {
            string path = @"Provider = Microsoft.ACE.OLEDB.12.0;Data Source = DataBase.accdb; Persist Security Info = False;";
            connection.ConnectionString = path;
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
            OleDbCommand command = new OleDbCommand();
            command.Connection = connection;
            command.CommandText = "select * from UserData";
            OleDbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                User user = new User();
                user.AccountName = reader["UserName"].ToString();
                user.FirstName = reader["LastName"].ToString();
                user.Tutor = reader["Tutor"].ToString();
                AllStudents.Add(user);
            }
            OleDbCommand command2 = new OleDbCommand();
            command2.Connection = connection;
            for (int i = 0; i < AllCategories.Count(); i++)
            {
                command2.CommandText = $"select * from {AllCategories[i].Prompt}";
                OleDbDataReader reader2 = command2.ExecuteReader();
                while (reader2.Read())
                {
                    User VotedBy = null;
                    User UserVotedFor = null;
                    for (int n = 0; n < AllStudents.Count(); n++)
                    {
                        if (AllStudents[n].AccountName == reader2["VotedFor"].ToString())
                        {
                            UserVotedFor = AllStudents[n];
                        }
                        if (AllStudents[n].AccountName == reader2["UserName"].ToString())
                        {
                            VotedBy = AllStudents[n];
                        }
                    }
                    AllCategories[i].Votes.Add(VotedBy, UserVotedFor);
                    AllCategories[i].InverseVotes.Add(UserVotedFor, VotedBy);
                }
            }
            connection.Close();
            AddVoteFor(AllCategories[0], AllStudents[0], AllStudents[1]);
        }

        /// <summary>
        /// Updates database to add a vote for a person in a given category
        /// </summary>
        /// <param name="categoryID">Category's ID</param>
        /// <param name="voted">Who's name was given to be voted for</param>
        /// <param name="votedBy">Who has actually done the vote</param>
        public void AddVoteFor(Category category, User voted, User votedBy)
        {
            connection.Open();
            OleDbCommand command = new OleDbCommand();
            command.Connection = connection;
            command.CommandText = $"insert into {category.Prompt} (UserName , VotedFor) values ('{votedBy.AccountName}','{voted.AccountName}')";
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}

