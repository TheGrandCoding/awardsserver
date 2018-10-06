using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AwardsServer.Program;

namespace AwardsServer.ServerUI
{
    public partial class UIForm : Form
    {
        public UIForm()
        {
            InitializeComponent();
        }


        public void UpdateStudents()
        {
            dgvStudents.Rows.Clear();
            foreach(var stud in Database.AllStudents)
            {
                object[] row = new object[] { stud.Value.FirstName, stud.Value.LastName, stud.Value.Tutor, stud.Value.Sex, stud.Value.HasVoted };
                dgvStudents.Rows.Add(row);
            }
        }
        public void UpdateCategory()
        {
            dgvCategories.Rows.Clear();
            foreach(var cat in Database.AllCategories)
            {
                object[] row = new object[] { cat.Key, cat.Value.Prompt };
                dgvCategories.Rows.Add(row);
            }
        }
        public void UpdateWinners()
        {
            dgvWinners.Rows.Clear();
            foreach(var cat in Database.AllCategories)
            {
                var sortedMale = cat.Value.OrderVotes('M');
                var sortedFemale = cat.Value.OrderVotes('F');

                string winnerMale = "N/A";
                string winnerFemale = "N/A";

                if(sortedMale.Count > 0)
                {
                    Program.TryGetUser(sortedMale[0], out User firstMale);
                    winnerMale = firstMale.FullName + " " + firstMale.Tutor + " (" + cat.Value.Votes[firstMale.AccountName].Count.ToString() + ")";
                }

                if (sortedFemale.Count > 0)
                {
                    Program.TryGetUser(sortedFemale[0], out User firstFemale);
                    winnerFemale = firstFemale.FullName + " " + firstFemale.Tutor + " (" + cat.Value.Votes[firstFemale.AccountName].Count.ToString() + ")";
                }

                object[] row = new object[] { cat.Value.Prompt, winnerMale, winnerFemale};
                dgvWinners.Rows.Add(row);
            }
        }

        private void UIForm_Load(object sender, EventArgs e)
        {
            UpdateStudents();
            UpdateCategory();
            UpdateWinners();
        }
    }
}
