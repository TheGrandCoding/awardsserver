using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwardsServer
{
    public class Program
    {
        // TODO stuff
        static void Main(string[] args)
        {
            while(true)
            {
                Logging.Log("Started up.");
                // some minor testing things below
                var user = new User();
                user.AccountName = "davsmi14";
                user.FirstName = "Dave";
                user.LastName = "Smith";
                user.Tutor = "11BOB";
                var category = new Category();
                category.Prompt = "Most likely to become Prime Minister";
                try
                {
                    category.AddVote(user, user);
                } catch (Exception ex)
                {
                    Logging.Log("StartUp", ex);
                }

                try
                {

                } catch (Exception ex)
                {
                    Logging.Log("LoadCategory", ex);
                }

                Console.ReadLine();
            }

        }
    }
    // Shared stuff that will be used across multiple files.
    public class User
    {
        public string AccountName; // eg 'cheale14'
        public string FirstName;
        public string LastName;
        public string Tutor;
    }
    public class Category
    {
        public int ID; // each category should have a integer assigned (from 1 to 15 for example)
        public string Prompt; // eg 'most likely to become Prime Minister'
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
    }
}
