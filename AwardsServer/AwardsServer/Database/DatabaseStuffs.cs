using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// == Code Owner
// Abdul
// == 

namespace AwardsServer
{ 
    public class DatabaseStuffs
    {

        public Dictionary<string, User> AllStudents = new Dictionary<string, User>(); // would be AccountName:User again
        public Dictionary<int, Category> AllCategories = new Dictionary<int, Category>(); // int is the Category's ID.

        public List<Category> Load_All_Votes()
        {
            // this should read from a database
            // and read all of the user's themselves,
            // then load all of the votes etc.
            // place them into the classes above, then return it.
            // so essentially: this is returning all of the categories, with existing votes already placed into them.
            // (it should also load the AllStudents and AllCategories lists..)
            throw new NotImplementedException("Waiting for @Abdul to implement");
        }

        /// <summary>
        /// Updates database to add a vote for a person in a given category
        /// </summary>
        /// <param name="categoryID">Category's ID</param>
        /// <param name="voted">Who's name was given to be voted for</param>
        /// <param name="votedBy">Who has actually done the vote</param>
        public void AddVoteFor(int categoryID, User voted, User votedBy)
        {
            throw new NotImplementedException("Waiting for @Abdul to implement");
        }
    }
}
