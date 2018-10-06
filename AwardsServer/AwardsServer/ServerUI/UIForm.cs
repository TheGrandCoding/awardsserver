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
using System.Reflection;

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

        private struct OptionHold
        {
            public string VariableName;
            public string AttributeValue;

            public Control InputControl;
            public Label NameControl;

            public FieldInfo FieldInfo;

            public object Value
            {
                get
                {
                    if(InputControl is TextBox)
                    {
                        TextBox tt = (TextBox)InputControl;
                        return tt.Text;
                    } else if(InputControl is NumericUpDown)
                    {
                        NumericUpDown tt = (NumericUpDown)InputControl;
                        return (int)tt.Value;
                    } else if(InputControl is CheckBox)
                    {
                        CheckBox tt = (CheckBox)InputControl;
                        return tt.Checked;
                    } else
                    {
                        return null;
                    }
                }
            }

            public Type InputType;
            public void Clear()
            {
                InputControl.Dispose();
                NameControl.Dispose();
            }
        }
        List<OptionHold> options = new List<OptionHold>();

        public void UpdateOptions()
        {
            foreach(var opt in options)
            {
                opt.Clear();
            }
            options = new List<OptionHold>();
            var variables = typeof(Program.Options).GetFields();
            foreach(var variable in variables)
            {
                Program.OptionAttribute option = variable.GetCustomAttribute<OptionAttribute>(false);
                if (option == null)
                    continue;
                variable.SetValue(null, option.DefaultValue); // null since it is static
                OptionHold hold = new OptionHold()
                {
                    AttributeValue = option.Description,
                    InputType = variable.FieldType,
                    VariableName = option.Name,
                    FieldInfo = variable
                };
                Control inputCont = null;
                Label display = new Label();
                int yValue = 30 + (options.Count * 30);
                display.Location = new Point(3, yValue);
                if(option.DefaultValue.GetType() == typeof(int))
                {
                    inputCont = new NumericUpDown();
                    ((NumericUpDown)inputCont).Value = (int)option.DefaultValue;
                } else if (option.DefaultValue.GetType() == typeof(string))
                {
                    inputCont = new TextBox();
                    ((TextBox)inputCont).Text = (string)option.DefaultValue;
                } else if (option.DefaultValue.GetType() == typeof(bool))
                {
                    inputCont = new CheckBox();
                    ((CheckBox)inputCont).Checked = (bool)option.DefaultValue;
                }
                inputCont.Location = new Point(275, yValue);
                display.Size = new Size(270, 25);
                inputCont.Tag = hold.VariableName;
                tabPage4.Controls.Add(inputCont);
                tabPage4.Controls.Add(display);
                hold.InputControl = inputCont;
                hold.NameControl = display;
                display.Text = option.Description;
                options.Add(hold);
            }
        }


        private void UIForm_Load(object sender, EventArgs e)
        {
            UpdateStudents();
            UpdateCategory();
            UpdateWinners();
            UpdateOptions();
        }

        private void queueTimer_Tick(object sender, EventArgs e)
        {
            lock(SocketHandler.LockClient)
            {
                foreach (var conn in SocketHandler.CurrentClients)
                {
                    conn.Heartbeat();
                }
                foreach (var conn in SocketHandler.ClientQueue)
                {
                    conn.Heartbeat();
                }
                int index = 0;
                foreach (var conn in SocketHandler.ClientQueue)
                {
                    index++;
                    conn.Send("QUEUE:" + index.ToString());
                }
            }
        }

        private void btnSaveOptions_Click(object sender, EventArgs e)
        {
            foreach(var hold in options)
            {
                if((hold.FieldInfo.GetValue(null) ?? "").Equals(hold.Value))
                {
                } else
                {
                    hold.FieldInfo.SetValue(null, hold.Value);
                    Logging.Log(Logging.LogSeverity.Warning, $"Updated {hold.VariableName}, now: {hold.FieldInfo.GetValue(null)}");
                }
            }
        }
        private int IdStartingEdit = 0;
        private void dgvCategories_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            var row = dgvCategories.Rows[e.RowIndex];
            int idVal = (int)(row.Cells[0].Value ?? -1) ;
            string nameVal = (string)row.Cells[1].Value;
        }

        private void dgvCategories_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var row = dgvCategories.Rows[e.RowIndex];
            int idVal = (int)(row.Cells[0].Value ?? Program.Database.AllCategories.Count+1);
            string nameVal = (string)row.Cells[1].Value;
            Category newC = new Category(nameVal, idVal);
            // i see SQL injection possibilities
            // maybe we should sanitise it?
            // eh.. its server only so should be ok

            // ..
            // famous last words
            if(Program.Database.AllCategories.ContainsKey(idVal))
            {
                newC.Votes = Program.Database.AllCategories[idVal].Votes;
                Program.Database.AllCategories[idVal] = newC;
                Database.ExecuteCommand($"UPDATE CategoryData SET Prompt = '{newC.Prompt}' WHERE ID={idVal}");
            } else
            {
                Program.Database.AllCategories.Add(idVal, newC);
                Database.ExecuteCommand($"INSERT INTO CategoryData (ID, Prompt) VALUES ({newC.ID}, '{newC.Prompt}')");
            }
        }
    }
}
