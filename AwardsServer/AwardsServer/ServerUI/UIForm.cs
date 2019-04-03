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
using AwardsServer.BugReport;

namespace AwardsServer.ServerUI
{
    public partial class UIForm : Form
    {
        public UIForm()
        {
            InitializeComponent();
        }

        public bool LockedUI;

        public void UpdateStudents()
        {
            dgvStudents.Rows.Clear();
            foreach (var stud in Database.AllStudents)
            {
                object[] row = new object[] { stud.Value.AccountName.ToString(), stud.Value.FirstName.ToString(), stud.Value.LastName.ToString(), stud.Value.Tutor.ToString(), stud.Value.HasVoted };
                dgvStudents.Rows.Add(row);
                dgvStudents.Rows[dgvStudents.Rows.Count - 1].ReadOnly = false;
            }
            dgvStudents.ReadOnly = false;
            if (Program.Options.Allow_Modifications_When_Voting)
            {
                PermittedStudentEdits(EditCapabilities.All);
            }
        }
        public void UpdateCategory()
        {
            dgvCategories.Rows.Clear();
            foreach (var cat in Database.AllCategories)
            {
                object[] row = new object[] { cat.Key, cat.Value.Prompt };
                dgvCategories.Rows.Add(row);
            }
        }
        public void UpdateWinners()
        {
            dgvWinners.Rows.Clear();
            foreach (var cat in Database.AllCategories)
            {
                string firstWinner = "";
                string secondWinner = "";

                var highestWinners = cat.Value.HighestAtPosition(0);
                var secondHighestWinners = cat.Value.HighestAtPosition(1);
                foreach (var maleWin in highestWinners.Item1)
                {
                    firstWinner += $"{maleWin.FullName} {maleWin.Tutor}, ";
                }
                foreach (var femaleWin in secondHighestWinners.Item1)
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
                if(LockedUI)
                {
                    firstWinner = "[Hidden]";
                    secondWinner = "[Hidden]";
                }
                object[] row = new object[] { cat.Value.ID.ToString("00") + ": " + cat.Value.Prompt, firstWinner, secondWinner };
                dgvWinners.Rows.Add(row);
            }
        }
        public void UpdateCurrentQueue()
        {
            dgvQueue.Rows.Clear();
            try
            {
                lock (SocketHandler.LockClient)
                { // prevents same-time access
                    int index = 0;
                    foreach (var que in SocketHandler.ClientQueue)
                    {
                        object[] row = new object[] { index, que.User.ToString("FN LN TT") };
                        dgvQueue.Rows.Add(row);
                        index++;
                    }
                    index = 0;
                }
            }
            catch { }
        }
        public void UpdateCurrentlyVoting()
        {
            dgvCurrentVoters.Rows.Clear();
            try
            {
                lock (SocketHandler.LockClient)
                {
                    foreach (var uu in SocketHandler.CurrentClients)
                    {
                        object[] row = new object[] { uu.IPAddress, uu.UserName, uu.User.ToString("FN LN TT"), uu.LastCategoryRequested.ToString("00") };
                        dgvCurrentVoters.Rows.Add(row);
                    }
                }
            }
            catch
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

            public OptionAttribute AttributeItself; // the attribute itself

            public object Value // gets value, from input, via parsing it depending on its input/type
            {
                get
                {
                    if (InputControl is TextBox)
                    {
                        TextBox tt = (TextBox)InputControl;
                        return tt.Text;
                    }
                    else if (InputControl is NumericUpDown)
                    {
                        NumericUpDown tt = (NumericUpDown)InputControl;
                        return (int)tt.Value;
                    }
                    else if (InputControl is CheckBox)
                    {
                        CheckBox tt = (CheckBox)InputControl;
                        return tt.Checked;
                    }
                    else if (InputControl is ComboBox)
                    {
                        ComboBox tt = (ComboBox)InputControl;
                        var obj = tt.Text;
                        // type should only be enum, considering we wont (shouldnt?) be displaying lists.
                        if (InputType.IsEnum)
                        {
                            var enumer = Enum.Parse(InputType, obj.ToString());
                            return enumer;
                        }
                        else
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
            btnSaveOptions.Enabled = !LockedUI;
            foreach (var opt in options)
            {
                opt.Clear();
            }
            options = new List<OptionHold>(); // resets all the options
            // gets all the variables on the class
            var variables = typeof(Program.Options).GetFields();
            foreach (var variable in variables)
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
                    FieldInfo = variable,
                    AttributeItself = option
                }; // holds option (get it?) related information

                // dynamic = compiler doesnt check to see if the functions exist
                // means the type can change, so its much easier to set its value
                // this gets it from the Registry, defaulting to the DefaultValue
                dynamic savedValue = Program.GetOption(hold.VariableName, option.DefaultValue.ToString());
                if (hold.InputType == typeof(int))
                {
                    savedValue = int.Parse(savedValue);
                }
                else if (hold.InputType == typeof(bool))
                {
                    savedValue = bool.Parse(savedValue);
                }
                else if (hold.InputType.IsEnum)
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
                if (savedValue.GetType() == typeof(int))
                {
                    inputCont = new NumericUpDown();
                    ((NumericUpDown)inputCont).Value = (int)savedValue;
                }
                else if (savedValue.GetType() == typeof(string))
                {
                    inputCont = new TextBox();
                    ((TextBox)inputCont).Text = (string)savedValue;
                    if(option.Sensitive && LockedUI)
                    {
                        var txt = (TextBox)inputCont;
                        txt.PasswordChar = '*';
                    }
                }
                else if (savedValue.GetType() == typeof(bool))
                {
                    inputCont = new CheckBox();
                    ((CheckBox)inputCont).Checked = (bool)savedValue;
                }
                else if (savedValue.GetType().IsEnum)
                { // enums are complicated
                    inputCont = new ComboBox();
                    string[] names = Enum.GetNames(savedValue.GetType());
                    var saved = savedValue.ToString();
                    int index = -1;
                    foreach (var i in names)
                    {
                        index++;
                        if (i == saved)
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

        public void UpdateManualVote()
        {
            if (txtNameOfManualVote.Enabled == true)
                btnSubmitManualVote.Visible = false;

            txtNameOfManualVote.Visible = !LockedUI;
            btnSubmitManualVote.Enabled = !LockedUI;
        }

        public void UpdateBugReports()
        {
            dgvBugReports.Rows.Clear();
            foreach (var report in Program.BugReports)
            {
                // ID | State | Type | Reporter | Primary | Submit / Close
                var row = new string[] { "ID", "State", "Type", "Reporter", "Primary", "Additional", "Submit" };
                row[0] = report.Id.ToString();
                Color rowColor = Color.FromKnownColor(KnownColor.Window);
                if (report.Submitted)
                {
                    if (report.Issue.IsClosed)
                    {
                        row[1] = "C";
                        rowColor = Color.Gray;
                        row[6] = "N/A";
                    }
                    else
                    {
                        row[1] = "S";
                        rowColor = Color.Green;
                        row[6] = "Close";
                    }
                }
                else
                {
                    row[1] = "P";
                    rowColor = Color.Yellow;
                }
                row[2] = report.Type.ToString();
                row[3] = report.Reporter.AccountName;
                row[4] = report.Primary;
                row[5] = report.Additional;
                var index = dgvBugReports.Rows.Add(row);
                dgvBugReports.Rows[index].DefaultCellStyle.BackColor = rowColor;
            }
        }

        public void SetLocalIP()
        {
            try
            {
                var path = Options.ServerIP_File_Path;
                var ip = GetLocalIPAddress();
                if (!string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(ip))
                    System.IO.File.WriteAllText(Options.ServerIP_File_Path, ip);
            }
            catch (Exception ex)
            {
                Logging.Log("ServerIPFile", ex);
            }
        }

        public void UpdateLockUI()
        {
            string registry = "uipassword";
            var savedItem = GetOption(registry, "");
            ServerUIForm.LockedUI = !string.IsNullOrWhiteSpace(savedItem);
            if (LockedUI)
            {
                lblInputLock.Text = "Enter input to unlock the UI";
                btnInputLock.Text = "Unlock";
            } else
            {
                lblInputLock.Text = "Enter input to lock the UI";
                btnInputLock.Text = "Lock UI";
            }
            txtInputLock.Text = ""; // clear any passwords
        }

        static bool warnedGithubBefore = false;
        private void UIForm_Load(object sender, EventArgs e)
        {
            UpdateLockUI();
            UpdateStudents();
            UpdateCategory();
            UpdateWinners();
            UpdateOptions();
            UpdateManualVote();
            SetLocalIP(); // must come after options since it relies on it

            // These may error in execution:
            UpdateCurrentQueue();
            UpdateCurrentlyVoting();

            try
            {
                WebServer = new ServerUI.WebsiteHandler();
            }
            catch { }

            if (Program.Github == null)
            {
                if (!warnedGithubBefore)
                {
                    warnedGithubBefore = true;
                    if (string.IsNullOrWhiteSpace(Options.Github_AuthToken))
                    {
                        Logging.Log(Logging.LogSeverity.Error, "Github authentication token is empty.");
                        Logging.Log(Logging.LogSeverity.Error, "Bug reporting will be disableld.");
                    }
                    else
                    {
                        Github = new GithubDLL.GithubClient(Options.Github_AuthToken, "tgc-awards");
                        Github.RequestMade += (object sendert, GithubDLL.RESTRequestEventArgs args) =>
                        {
                            bool success = (int)args.ResponseCode >= 200 && (int)args.ResponseCode <= 299;
                            Logging.LogSeverity sev = success ? Logging.LogSeverity.Debug : Logging.LogSeverity.Warning;
                            Logging.Log(sev, args.ToString(), "GithubREST");
                        };
                        Program.AwardsRepository = Github.GetRepository("thegrandcoding", "awardsserver");
                        LoadBugs();
                        UpdateBugReports();
                    }
                }
            }
            else
            {
                UpdateBugReports();
            }
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
                }
                catch (Exception ex)
                {
                    Logging.Log("QueueTimer", ex);
                }
            }
            while (SocketHandler.CurrentClients.Count < Options.Maximum_Concurrent_Connections && SocketHandler.ClientQueue.Count > 0)
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
            lock (SocketHandler.LockClient)
            {
                int index = 0;
                try
                {
                    foreach (var conn in SocketHandler.ClientQueue)
                    {
                        index++;
                        conn.Send("QUEUE:" + index.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Logging.Log("QueueTimer", ex);
                }

            }
        }

        private void btnSaveOptions_Click(object sender, EventArgs e)
        {
            foreach (var hold in options)
            {
                if ((hold.FieldInfo.GetValue(null) ?? "").Equals(hold.Value))
                {
                }
                else
                {
                    hold.FieldInfo.SetValue(null, hold.Value);
                    SetOption(hold.VariableName, hold.Value.ToString());
                    string nowValue = hold.FieldInfo.GetValue(null).ToString();
                    if(hold.AttributeItself.Sensitive)
                    { // never log sensitive things
                        nowValue = new string('*', nowValue.Length);
                    }
                    Logging.Log(Logging.LogSeverity.Warning, $"Updated {hold.VariableName}, now: {nowValue}");
                    if (hold.VariableName == nameof(Program.Options.WebSever_Enabled))
                    {
                        Logging.Log(Logging.LogSeverity.Warning, "WEB SERVER CHANGED:");
                        if (((bool)hold.FieldInfo.GetValue(null)))
                        {
                            Logging.Log(Logging.LogSeverity.Warning, "You have just enabled the web server, but you will need to restart the server for it to actually come online");
                        }
                        else
                        {
                            Logging.Log(Logging.LogSeverity.Warning, "You have just disabled the web server. It will reply with 418 'Offline' to any client requests");
                        }
                    }
                }
            }
        }
        private void dgvCategories_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        { // this function isnt actually usefull
            var row = dgvCategories.Rows[e.RowIndex];
            int idVal = (int)(row.Cells[0].Value ?? -1);
            string nameVal = (string)row.Cells[1].Value;
        }

        private void dgvCategories_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var row = dgvCategories.Rows[e.RowIndex];
            int idVal = (int)(row.Cells[0].Value ?? Program.Database.AllCategories.Count + 1);
            string nameVal = (string)row.Cells[1].Value;
            Category newC = new Category(nameVal, idVal);
            // i see SQL injection possibilities
            // maybe we should sanitise it?
            // eh.. its server only so should be ok

            // ..
            // famous last words
            if (Program.Database.AllCategories.ContainsKey(idVal))
            {
                newC.Votes = Program.Database.AllCategories[idVal].Votes;
                Program.Database.AllCategories[idVal] = newC;
                Database.ExecuteCommand($"UPDATE CategoryData SET Prompt = '{newC.Prompt}' WHERE ID={idVal}");
            }
            else
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
            if (newUser.AccountName != editUser.AccountName)
            {
                Database.ExecuteCommand($"UPDATE UserData SET UserName = '{newUser.AccountName}' WHERE UserName = '{editUser.AccountName}'");
                foreach (var t in Database.AllCategories)
                {
                    if (t.Value.Votes.ContainsKey(editUser.AccountName))
                    {
                        var things = t.Value.Votes[editUser.AccountName];
                        t.Value.Votes.Remove(editUser.AccountName);
                        t.Value.Votes.Add(newUser.AccountName, things);
                    }
                    foreach (var v in t.Value.Votes)
                    {
                        var existing = v.Value.FirstOrDefault(x => x.AccountName == editUser.AccountName);
                        if (existing != null)
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
            if (curVote == false && startVoted == true)
            {
                if (MessageBox.Show($"Are you sure you want to remove {newUser.ToString("FN LN (TT)")}'s votes?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    foreach (var category in Database.AllCategories)
                    {
                        Database.ExecuteCommand($"DELETE FROM Category{category.Key} WHERE UserName = '{newUser.AccountName}'");
                    }
                    Logging.Log(Logging.LogSeverity.Warning, "Reloading all database information");
                    Database.Load_All_Votes();
                }
            }
            else
            {
                row.Cells[4].Value = startVoted;
            }
        }
        [Flags]
        public enum EditCapabilities
        {
            None = 0b000000,
            AccountName = 0b000001,
            FirstName = 0b000010,
            LastName = 0b000100,
            Tutor = 0b001000,
            Voted = 0b010000,
            All = AccountName | FirstName | LastName | Tutor | Voted,
            NoneImportant = FirstName | LastName | Tutor
        }
        public void PermittedStudentEdits(EditCapabilities possibles)
        {
            if (this.InvokeRequired)
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

        private void btnPerformManualVote_Click(object sender, EventArgs e)
        {
            if (Program.TryGetUser(txtNameOfManualVote.Text ?? "", out var student))
            {
                txtNameOfManualVote.Text = student.AccountName;
                txtNameOfManualVote.ReadOnly = true;
                dgvManualVotes.Rows.Clear();
                foreach (var category in Database.AllCategories.Values)
                {
                    var existingVotes = category.GetVotesBy(student);
                    string[] row = new string[] { $"{category.ID.ToString("00")} {category.Prompt}", "", "" };
                    dgvManualVotes.Rows.Add(row);
                    var dgvRow = dgvManualVotes.Rows[category.ID - 1];
                    if (existingVotes.Item1 != null)
                    {
                        dgvRow.Cells[1].Tag = existingVotes.Item1.AccountName;
                        dgvRow.Cells[1].Value = existingVotes.Item1.ToString("FN LN TT");
                        dgvRow.Cells[1].Style.BackColor = Color.Aquamarine;
                    }
                    if (existingVotes.Item2 != null)
                    {
                        dgvRow.Cells[2].Tag = existingVotes.Item2.AccountName;
                        dgvRow.Cells[2].Value = existingVotes.Item2.ToString("FN LN TT");
                        dgvRow.Cells[2].Style.BackColor = Color.Aquamarine;
                    }
                }
                btnReadyManualVote.Visible = false;
                btnSubmitManualVote.Visible = true;
            }
            else
            {
                txtNameOfManualVote.BackColor = Color.Red;
                MessageBox.Show("Unknown user");
            }
        }

        private void txtNameOfManualVote_TextChanged(object sender, EventArgs e)
        {
            txtNameOfManualVote.BackColor = Color.FromKnownColor(KnownColor.Window);
        }

        static UserVoteSubmit Confirming;
        private void btnSubmitManualVote_Click(object sender, EventArgs e)
        {
            var defaultBackground = dgvManualVotes.Rows[0].Cells[0].Style.BackColor;
            var highlightBackground = Color.IndianRed;
            int numEmpty = 0;
            UserVoteSubmit vote = null;
            if (Program.TryGetUser(txtNameOfManualVote.Text, out User user))
            {
                vote = new UserVoteSubmit(user);
            }
            else
            {
                return;
            }
            foreach (DataGridViewRow row in dgvManualVotes.Rows)
            {
                var firstWinner = row.Cells[1];
                var secondWinner = row.Cells[2];
                User first = null;
                User second = null;
                if (string.IsNullOrWhiteSpace((string)firstWinner.Tag))
                    numEmpty++;
                else
                    Program.TryGetUser((string)firstWinner.Tag, out first);

                firstWinner.Style.BackColor = (string.IsNullOrWhiteSpace((string)firstWinner.Tag)) ? highlightBackground : defaultBackground;
                if (string.IsNullOrWhiteSpace((string)secondWinner.Tag))
                    numEmpty++;
                else
                    Program.TryGetUser((string)secondWinner.Tag, out second);
                secondWinner.Style.BackColor = (string.IsNullOrWhiteSpace((string)secondWinner.Tag)) ? highlightBackground : defaultBackground;

                vote.AddVote(row.Index + 1, first, second);
            }
            if (numEmpty > 0 && (vote.ConfirmString != (Confirming?.ConfirmString ?? "")))
            {
                Confirming = vote;
                MessageBox.Show($"Warning:\r\nYou have left {numEmpty} nominations empty\r\nPlease confirm the highlighted ones, and hit 'Submit' again", "Empty Nominations", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                vote.Submit();
                MessageBox.Show($"Vote has been submitted", "Submitted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void dgvManualVotes_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
                return;
            var row = dgvManualVotes.Rows[e.RowIndex];
            var cell = row.Cells[e.ColumnIndex];
            string content = (string)cell.Value;
            if (!Program.TryGetUser(content, out User user)) // try text, so they can override..
                Program.TryGetUser((string)cell.Tag, out user);  // but also check tag, in case we are resetting a prior vote
            if (user != null)
            {
                cell.Style.BackColor = Color.FromKnownColor(KnownColor.Window);
                cell.Value = user.ToString("FN LN TT");
                cell.Tag = user.AccountName;
            }
            else
            {
                cell.Tag = "";
                cell.Style.BackColor = Color.Red;
            }
        }

        private void dgvBugReports_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;
            var row = dgvBugReports.Rows[e.RowIndex];
            var firstCell = int.Parse(row.Cells[0].Value.ToString());
            var report = Program.BugReports.FirstOrDefault(x => x.Id == firstCell);
            if (e.ColumnIndex == 0)
            { // ID clicked
                if (report.Submitted)
                {
                    string url = report.Issue.HTMLURL;
                    try
                    {
                        Clipboard.SetText(url);
                    }
                    catch { }
                    System.Diagnostics.Process.Start(Options.DEFAULT_WEB_BROWSER, url);
                } else
                {
                    // clicked id of non-submitted, so we want to open the log
                    System.Diagnostics.Process.Start(Options.DEFAULT_TEXT_EDITOR, report.LogFile);
                }
            }
            else if (e.ColumnIndex == 6)
            { // button click
                if (report.Submitted)
                {
                    if (report.Issue.IsClosed)
                    { // do nothing
                    }
                    else
                    {
                        // close issue, with reason.
                        var reason = Microsoft.VisualBasic.Interaction.InputBox("Enter a reason for closing the issue", "Closure Reason");
                        if (!string.IsNullOrWhiteSpace(reason))
                        {
                            report.Issue.Comment("Closed by server, reason provided: " + reason);
                            report.Issue.ModifyAsync(x =>
                            {
                                x.state = "closed";
                            });
                        }
                        report.Solved = true;
                    }
                }
                else
                {
                    // not submitted, so we need to create an issue.
                    var issue = Program.AwardsRepository.CreateIssue(x =>
                    {
                        x.title = "Bug report by " + report.Reporter.AccountName;
                        x.body = $"**Type:** {report.Type}\r\n" +
                                 $"**Primary:** {report.Primary}\r\n" +
                                 $"**Additional:** {report.Additional}";
                        x.labels = new string[] { "bug", $"bug-{report.Type.ToString().ToLower()}" };
                    });
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(report.LogFile))
                        {
                            var content = Program.AwardsRepository.CommitFile(report.LogFile, $"bugs/{issue.Number}_{report.Reporter.AccountName}.txt", $"Uploading log file for issue: #{issue.Number}", new GithubDLL.API.ShortUserCreate($"{Environment.UserName}", "no-email@noemail.org"));
                        }
                    } catch (Exception ex)
                    {
                        Logging.Log("UploadLog", ex);
                    }
                    report.Issue = issue;
                }
                Program.SaveBugs();
            }
        }

        private void btnInputLock_Click(object sender, EventArgs e)
        {
            string registry = "uipassword";
            var savedItem = GetOption(registry, "");
            ServerUIForm.LockedUI = !string.IsNullOrWhiteSpace(savedItem);
            if(LockedUI)
            {
                // we are unlocking.
                if (txtInputLock.Text == savedItem)
                {
                    SetOption(registry, "");
                } else
                {
                    MessageBox.Show($"Password incorrect.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } else
            {
                SetOption(registry, txtInputLock.Text);
            }
            this.Close(); // UI will reopen with new settings instantly applied
        }
    }
}
