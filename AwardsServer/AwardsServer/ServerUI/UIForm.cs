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
                object[] row = new object[] { stud.Value.AccountName.ToString(), stud.Value.FirstName.ToString(), stud.Value.LastName.ToString(), stud.Value.Tutor.ToString(), stud.Value.HasVoted };
                dgvStudents.Rows.Add(row);
                dgvStudents.Rows[dgvStudents.Rows.Count - 1].ReadOnly = false;
            }
            dgvStudents.ReadOnly = false;
            if(Program.Options.Allow_Modifications_When_Voting)
            {
                PermittedStudentEdits(EditCapabilities.All);
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
                string firstWinner = "";
                string secondWinner = "";

                var highestWinners = cat.Value.HighestVoter(false);
                var secondHighestWinners = cat.Value.HighestVoter(true);
                foreach(var maleWin in highestWinners.Item1)
                {
                    firstWinner += $"{maleWin.FullName} {maleWin.Tutor}, ";
                }
                foreach(var femaleWin in secondHighestWinners.Item1)
                {
                    secondWinner += $"{femaleWin.FullName} {femaleWin.Tutor}, ";
                }
                if (highestWinners.Item1.Count > 0)
                {
                    firstWinner += $"({highestWinners.Item2})";
                }
                else
                {
                    firstWinner = "N/A";
                }
                if (secondHighestWinners.Item1.Count > 0)
                {
                    secondWinner += $"({secondHighestWinners.Item2})";
                }
                else
                {
                    secondWinner = "N/A";
                }

                object[] row = new object[] { cat.Value.ID.ToString("00") + ": " + cat.Value.Prompt, firstWinner, secondWinner};
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
                        object[] row = new object[] { uu.IPAddress, uu.UserName, uu.User.ToString("AN FN LN TT") };
                        dgvCurrentVoters.Rows.Add(row);
                    }
                }
            } catch
            {
            }
        }

        /// <summary>
        /// Holds information regarding an Option.
        /// </summary>
        private struct OptionHold
        {
            public string VariableName; // name of the variable
            public string AttributeValue; // name as given by the [Option] attribute above the variable

            public Control InputControl; // control the user edits
            public Label NameControl; // control that is a label

            public FieldInfo FieldInfo; // the variable itself in the Options class.

            public object Value // gets value, from input, via parsing it depending on its input/type
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
                    } else if(InputControl is ComboBox)
                    {
                        ComboBox tt = (ComboBox)InputControl;
                        var obj = tt.Text;
                        // type should only be enum, considering we wont (shouldnt?) be displaying lists.
                        if(InputType.IsEnum)
                        {
                            var enumer = Enum.Parse(InputType, obj.ToString());
                            return enumer;
                        } else
                        {
                            throw new NotImplementedException("Cannot use ComboBox and non-Enum");
                        }
                    }
                    else
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
        { // yeah, i have no idea how to comment all this lol
            foreach(var opt in options)
            {
                opt.Clear();
            }
            options = new List<OptionHold>(); // resets all the options
            // gets all the variables on the class
            var variables = typeof(Program.Options).GetFields();
            foreach(var variable in variables)
            {
                // gets option attribute from the variable itself
                Program.OptionAttribute option = variable.GetCustomAttribute<OptionAttribute>(false);
                if (option == null)
                    continue;
                // sets the value to its default, so it isnt 'null'
                variable.SetValue(null, option.DefaultValue); // null since the Options class is static
                OptionHold hold = new OptionHold()
                {
                    AttributeValue = option.Description,
                    InputType = variable.FieldType,
                    VariableName = variable.Name,
                    FieldInfo = variable
                }; // holds option (get it?) related information

                // dynamic = compiler doesnt check to see if the functions exist
                // means the type can change, so its much easier to set its value
                // this gets it from the Registry, defaulting to the DefaultValue
                dynamic savedValue = Program.GetOption(hold.VariableName, option.DefaultValue.ToString());
                if (hold.InputType == typeof(int))
                {
                    savedValue = int.Parse(savedValue);
                } else if (hold.InputType == typeof(bool))
                {
                    savedValue = bool.Parse(savedValue);
                } else if (hold.InputType.IsEnum)
                {
                    savedValue = Enum.Parse(hold.InputType, savedValue);
                }
                if (savedValue == null)
                    savedValue = option.DefaultValue;
                // now, sets the value from the one we have saved.
                variable.SetValue(null, savedValue);
                // saves it in the Registry.
                Program.SetOption(hold.VariableName, savedValue.ToString());
                // From below, is setting the UI controls and such
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
                } else if (savedValue.GetType().IsEnum)
                { // enums are complicated
                    inputCont = new ComboBox();
                    string[] names = Enum.GetNames(savedValue.GetType());
                    var saved = savedValue.ToString();
                    int index = -1;
                    foreach(var i in names)
                    {
                        index++;
                        if(i == saved)
                        {
                            break;
                        }
                    }
                    ComboBox tt = (ComboBox)inputCont;
                    tt.Items.AddRange(names);
                    tt.SelectedIndex = index;
                } 
                inputCont.Location = new Point(275, yValue);
                display.Size = new Size(270, 25);
                inputCont.Tag = hold.VariableName;
                tabPage4.Controls.Add(inputCont);
                tabPage4.Controls.Add(display);
                hold.InputControl = inputCont;
                hold.NameControl = display;
                display.Text = option.Description;
                inputCont.Enabled = !option.ReadOnly;
                options.Add(hold);
            }
        }

        public void SetLocalIP()
        {
            try
            {
                var path = Options.ServerIP_File_Path;
                var ip = GetLocalIPAddress();
                if(!string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(ip))
                    System.IO.File.WriteAllText(Options.ServerIP_File_Path, ip);
            }
            catch (Exception ex)
            {
                Logging.Log("ServerIPFile", ex);
            }
        }

        private void UIForm_Load(object sender, EventArgs e)
        {
            UpdateStudents();
            UpdateCategory();
            UpdateWinners();
            UpdateOptions();
            SetLocalIP(); // must come after options since it relies on it

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
            while(SocketHandler.CurrentClients.Count < Options.Maximum_Concurrent_Connections && SocketHandler.ClientQueue.Count > 0)
            {
                try
                {
                    var client = SocketHandler.ClientQueue.FirstOrDefault();
                    if (client == null)
                        break;
                    SocketHandler.ClientQueue.Remove(client);
                    SocketHandler.CurrentClients.Add(client);
                    client.AcceptFromQueue();
                }
                catch (Exception ex)
                {
                    Logging.Log("QueueWhenZero", ex);
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
            Database.ExecuteCommand($"UPDATE UserData SET FirstName = '{newUser.FirstName}', LastName = '{newUser.LastName}', Tutor = '{newUser.Tutor}' WHERE UserName = '{newUser.AccountName}'");
            bool curVote = bool.Parse(row.Cells[4].Value.ToString());
            if(curVote == false && startVoted == true)
            {
                if(MessageBox.Show($"Are you sure you want to remove {newUser.ToString("FN LN (TT)")}'s votes?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    foreach (var category in Database.AllCategories)
                    {
                        Database.ExecuteCommand($"DELETE FROM Category{category.Key} WHERE UserName = '{newUser.AccountName}'");
                    }
                    Logging.Log(Logging.LogSeverity.Warning, "Reloading all database information");
                    Database.Load_All_Votes();
                }
            } else
            {
                row.Cells[4].Value = startVoted;
            }
        }
        [Flags]
        public enum EditCapabilities
        {
            None =        0b000000,
            AccountName = 0b000001,
            FirstName =   0b000010,
            LastName =    0b000100,
            Tutor =       0b001000,
            Voted =       0b010000,
            All = AccountName | FirstName | LastName | Tutor | Voted,
            NoneImportant = FirstName | LastName | Tutor
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
            dgvStudents.Columns[4].ReadOnly = !possibles.HasFlag(EditCapabilities.Voted);
        }

        private User editUser = null;
        private bool startVoted = false;
        private void dgvStudents_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            var row = dgvStudents.Rows[e.RowIndex];
            editUser = userFromColumns(row.Cells);
            startVoted = bool.Parse(row.Cells[4].Value.ToString());
        }
        private User userFromColumns(DataGridViewCellCollection cells)
        {
            return new User(cells[0].Value.ToString(),
                cells[1].Value.ToString(),
                cells[2].Value.ToString(),
                cells[3].Value.ToString()
                );
        }

        private void dgvStudents_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
