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
                object[] row = new object[] { stud.Value.AccountName.ToString(), stud.Value.FirstName.ToString(), stud.Value.LastName.ToString(), stud.Value.Tutor.ToString(), stud.Value.Sex.ToString(), stud.Value.HasVoted ? "Yes" : "No" };
                dgvStudents.Rows.Add(row);
                dgvStudents.Rows[dgvStudents.Rows.Count - 1].ReadOnly = false;
            }
            dgvStudents.ReadOnly = false;
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
                string winnerMale = "";
                string winnerFemale = "";

                var maleWinners = cat.Value.HighestVoter('M');
                var femaleWinners = cat.Value.HighestVoter('F');
                foreach(var maleWin in maleWinners.Item1)
                {
                    winnerMale += $"{maleWin.FullName} {maleWin.Tutor}, ";
                }
                foreach(var femaleWin in femaleWinners.Item1)
                {
                    winnerFemale += $"{femaleWin.FullName} {femaleWin.Tutor}, ";
                }
                if (maleWinners.Item1.Count > 0)
                {
                    winnerMale += $"({maleWinners.Item2})";
                }
                else
                {
                    winnerMale = "N/A";
                }
                if (femaleWinners.Item1.Count > 0)
                {
                    winnerFemale += $"({femaleWinners.Item2})";
                }
                else
                {
                    winnerFemale = "N/A";
                }

                object[] row = new object[] { cat.Value.ID.ToString() + ": " + cat.Value.Prompt, winnerMale, winnerFemale};
                dgvWinners.Rows.Add(row);
            }
        }
        public void UpdateCurrentQueue()
        {
            dgvQueue.Rows.Clear();
            try
            {
                lock(SocketHandler.LockClient)
                { // prevents same-time access
                    int index = 0;
                    foreach(var que in SocketHandler.ClientQueue)
                    {
                        object[] row = new object[] { index, que.User.ToString("FN LN TT") };
                        dgvQueue.Rows.Add(row);
                        index++;
                    }
                    index = 0;
                }
            } catch { }
        }
        public void UpdateCurrentlyVoting()
        {
            dgvCurrentVoters.Rows.Clear();
            try
            {
                lock (SocketHandler.LockClient)
                {
                    foreach(var uu in SocketHandler.CurrentClients)
                    {
                        object[] row = new object[] { uu.IPAddress, uu.UserName, uu.User.ToString("AN FN LN TT SX") };
                        dgvCurrentVoters.Rows.Add(row);
                    }
                }
            } catch
            {
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
                    VariableName = variable.Name,
                    FieldInfo = variable
                };

                dynamic savedValue = Program.GetOption(hold.VariableName, option.DefaultValue.ToString());
                if (hold.InputType == typeof(int))
                {
                    savedValue = int.Parse(savedValue);
                } else if (hold.InputType == typeof(bool))
                {
                    savedValue = bool.Parse(savedValue);

                }
                if (savedValue == null)
                    savedValue = option.DefaultValue;
                variable.SetValue(null, savedValue);
                Program.SetOption(hold.VariableName, savedValue.ToString());
                Control inputCont = null;
                Label display = new Label();
                int yValue = 30 + (options.Count * 30);
                display.Location = new Point(3, yValue);
                if(savedValue.GetType() == typeof(int))
                {
                    inputCont = new NumericUpDown();
                    ((NumericUpDown)inputCont).Value = (int)savedValue;
                } else if (savedValue.GetType() == typeof(string))
                {
                    inputCont = new TextBox();
                    ((TextBox)inputCont).Text = (string)savedValue;
                } else if (savedValue.GetType() == typeof(bool))
                {
                    inputCont = new CheckBox();
                    ((CheckBox)inputCont).Checked = (bool)savedValue;
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

            // These may error in execution:
            UpdateCurrentQueue();
            UpdateCurrentlyVoting();
        }

        private void queueTimer_Tick(object sender, EventArgs e)
        {
            queueTimer.Interval = Program.Options.Time_Between_Heartbeat * 1000; // since its in seconds
            lock (SocketHandler.LockClient)
            {
                try
                {
                    foreach (var conn in SocketHandler.CurrentClients)
                    {
                        conn.Heartbeat();
                    }
                    foreach (var conn in SocketHandler.ClientQueue)
                    {
                        conn.Heartbeat();
                    }
                } catch(Exception ex)
                {
                    Logging.Log("QueueTimer", ex);
                }
            }
            lock(SocketHandler.LockClient)
            { 
                int index = 0;
                try
                {
                    foreach (var conn in SocketHandler.ClientQueue)
                    {
                        index++;
                        conn.Send("QUEUE:" + index.ToString());
                    }
                } catch (Exception ex)
                {
                    Logging.Log("QueueTimer", ex);
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
                    SetOption(hold.VariableName, hold.Value.ToString());
                    Logging.Log(Logging.LogSeverity.Warning, $"Updated {hold.VariableName}, now: {hold.FieldInfo.GetValue(null)}");
                }
            }
        }
        private void dgvCategories_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        { // this function isnt actually usefull
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

        private void UIForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.queueTimer.Enabled = false;
            this.queueTimer = null;
        }

        private void dgvStudents_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var row = dgvStudents.Rows[e.RowIndex];
            var newUser = userFromColumns(row.Cells);
            if(newUser.AccountName != editUser.AccountName)
            {
                Database.ExecuteCommand($"UPDATE UserData SET UserName = '{newUser.AccountName}' WHERE UserName = '{editUser.AccountName}'");
                foreach(var t in Database.AllCategories)
                {
                    if(t.Value.Votes.ContainsKey(editUser.AccountName))
                    {
                        var things = t.Value.Votes[editUser.AccountName];
                        t.Value.Votes.Remove(editUser.AccountName);
                        t.Value.Votes.Add(newUser.AccountName, things);
                    }
                    foreach(var v in t.Value.Votes)
                    {
                        var existing = v.Value.FirstOrDefault(x => x.AccountName == editUser.AccountName);
                        if(existing != null)
                        {
                            v.Value.Remove(existing);
                            v.Value.Add(newUser);
                        }
                    }
                    Database.ExecuteCommand($"UPDATE Category{t.Key} SET UserName = '{newUser.AccountName}' WHERE UserName = '{editUser.AccountName}'");
                    Database.ExecuteCommand($"UPDATE Category{t.Key} SET VotedFor = '{newUser.AccountName}' WHERE VotedFor = '{editUser.AccountName}'");
                }
                Database.AllStudents.Remove(editUser.AccountName); // remove the old one..
                Database.AllStudents.Add(newUser.AccountName, newUser);
            }
            Database.AllStudents[newUser.AccountName] = newUser;
            Database.ExecuteCommand($"UPDATE UserData SET FirstName = '{newUser.FirstName}', LastName = '{newUser.LastName}', Tutor = '{newUser.Tutor}', Sex = '{newUser.Sex}' WHERE UserName = '{newUser.AccountName}'");
        }
        [Flags]
        public enum EditCapabilities
        {
            None =        0b00000,
            AccountName = 0b00001,
            FirstName =   0b00010,
            LastName =    0b00100,
            Tutor =       0b01000,
            Sex =         0b10000,
            All = AccountName | FirstName | LastName | Tutor | Sex

        }
        public void PermittedStudentEdits(EditCapabilities possibles)
        {
            if(this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    PermittedStudentEdits(possibles);
                }));
                return;
            }
            dgvStudents.Columns[0].ReadOnly = !possibles.HasFlag(EditCapabilities.AccountName);
            dgvStudents.Columns[1].ReadOnly = !possibles.HasFlag(EditCapabilities.FirstName);
            dgvStudents.Columns[2].ReadOnly = !possibles.HasFlag(EditCapabilities.LastName);
            dgvStudents.Columns[3].ReadOnly = !possibles.HasFlag(EditCapabilities.Tutor);
            dgvStudents.Columns[4].ReadOnly = !possibles.HasFlag(EditCapabilities.Sex);
            dgvStudents.Columns[5].ReadOnly = true; // voted.
        }

        private User editUser = null;
        private void dgvStudents_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            var row = dgvStudents.Rows[e.RowIndex];
            editUser = userFromColumns(row.Cells);
        }
        private User userFromColumns(DataGridViewCellCollection cells)
        {
            return new User(cells[0].Value.ToString(),
                cells[1].Value.ToString(),
                cells[2].Value.ToString(),
                cells[3].Value.ToString(),
                char.Parse(cells[4].Value.ToString())
                );
        }
    }
}
