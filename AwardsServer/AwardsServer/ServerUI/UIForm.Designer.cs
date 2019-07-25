namespace AwardsServer.ServerUI
{
    partial class UIForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.dgvStudents = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column13 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dgvCategories = new System.Windows.Forms.DataGridView();
            this.Column6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.dgvWinners = new System.Windows.Forms.DataGridView();
            this.Column8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.btnSaveOptions = new System.Windows.Forms.Button();
            this.tabCurrentQ = new System.Windows.Forms.TabPage();
            this.dgvQueue = new System.Windows.Forms.DataGridView();
            this.Column11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabCurrentV = new System.Windows.Forms.TabPage();
            this.dgvCurrentVoters = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column14 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column24 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPage7 = new System.Windows.Forms.TabPage();
            this.btnSubmitManualVote = new System.Windows.Forms.Button();
            this.btnReadyManualVote = new System.Windows.Forms.Button();
            this.dgvManualVotes = new System.Windows.Forms.DataGridView();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column15 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column16 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.txtNameOfManualVote = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage8 = new System.Windows.Forms.TabPage();
            this.dgvBugReports = new System.Windows.Forms.DataGridView();
            this.Column17 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column18 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column19 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column20 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column21 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column23 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column22 = new System.Windows.Forms.DataGridViewButtonColumn();
            this.uiLockTab = new System.Windows.Forms.TabPage();
            this.btnInputLock = new System.Windows.Forms.Button();
            this.txtInputLock = new System.Windows.Forms.TextBox();
            this.lblInputLock = new System.Windows.Forms.Label();
            this.queueTimer = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStudents)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCategories)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWinners)).BeginInit();
            this.tabPage4.SuspendLayout();
            this.tabCurrentQ.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvQueue)).BeginInit();
            this.tabCurrentV.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCurrentVoters)).BeginInit();
            this.tabPage7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvManualVotes)).BeginInit();
            this.tabPage8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBugReports)).BeginInit();
            this.uiLockTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabCurrentQ);
            this.tabControl1.Controls.Add(this.tabCurrentV);
            this.tabControl1.Controls.Add(this.tabPage7);
            this.tabControl1.Controls.Add(this.tabPage8);
            this.tabControl1.Controls.Add(this.uiLockTab);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(800, 450);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dgvStudents);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage1.Size = new System.Drawing.Size(792, 421);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Students";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // dgvStudents
            // 
            this.dgvStudents.AllowUserToAddRows = false;
            this.dgvStudents.AllowUserToDeleteRows = false;
            this.dgvStudents.AllowUserToResizeRows = false;
            this.dgvStudents.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvStudents.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvStudents.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4,
            this.Column13});
            this.dgvStudents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvStudents.Location = new System.Drawing.Point(3, 2);
            this.dgvStudents.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgvStudents.Name = "dgvStudents";
            this.dgvStudents.RowHeadersVisible = false;
            this.dgvStudents.RowTemplate.Height = 24;
            this.dgvStudents.Size = new System.Drawing.Size(786, 417);
            this.dgvStudents.TabIndex = 2;
            this.dgvStudents.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dgvStudents_CellBeginEdit);
            this.dgvStudents.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvStudents_CellEndEdit);
            // 
            // Column1
            // 
            this.Column1.HeaderText = "Acc. Name";
            this.Column1.Name = "Column1";
            // 
            // Column2
            // 
            this.Column2.HeaderText = "First Name";
            this.Column2.Name = "Column2";
            // 
            // Column3
            // 
            this.Column3.HeaderText = "Second Name";
            this.Column3.Name = "Column3";
            // 
            // Column4
            // 
            this.Column4.HeaderText = "Tutor";
            this.Column4.Name = "Column4";
            // 
            // Column13
            // 
            this.Column13.HeaderText = "Voted";
            this.Column13.Name = "Column13";
            this.Column13.ReadOnly = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dgvCategories);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage2.Size = new System.Drawing.Size(792, 421);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Categories";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dgvCategories
            // 
            this.dgvCategories.AllowUserToDeleteRows = false;
            this.dgvCategories.AllowUserToResizeRows = false;
            this.dgvCategories.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvCategories.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCategories.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column6,
            this.Column7});
            this.dgvCategories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCategories.Location = new System.Drawing.Point(3, 2);
            this.dgvCategories.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgvCategories.Name = "dgvCategories";
            this.dgvCategories.RowHeadersVisible = false;
            this.dgvCategories.RowTemplate.Height = 24;
            this.dgvCategories.Size = new System.Drawing.Size(786, 417);
            this.dgvCategories.TabIndex = 1;
            this.dgvCategories.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dgvCategories_CellBeginEdit);
            this.dgvCategories.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCategories_CellEndEdit);
            // 
            // Column6
            // 
            this.Column6.HeaderText = "ID";
            this.Column6.Name = "Column6";
            this.Column6.ReadOnly = true;
            // 
            // Column7
            // 
            this.Column7.HeaderText = "Prompt";
            this.Column7.Name = "Column7";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.dgvWinners);
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage3.Size = new System.Drawing.Size(792, 421);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Winners";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // dgvWinners
            // 
            this.dgvWinners.AllowUserToAddRows = false;
            this.dgvWinners.AllowUserToDeleteRows = false;
            this.dgvWinners.AllowUserToResizeRows = false;
            this.dgvWinners.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvWinners.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvWinners.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column8,
            this.Column9,
            this.Column10});
            this.dgvWinners.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvWinners.Location = new System.Drawing.Point(3, 2);
            this.dgvWinners.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgvWinners.Name = "dgvWinners";
            this.dgvWinners.ReadOnly = true;
            this.dgvWinners.RowHeadersVisible = false;
            this.dgvWinners.RowTemplate.Height = 24;
            this.dgvWinners.Size = new System.Drawing.Size(786, 417);
            this.dgvWinners.TabIndex = 2;
            // 
            // Column8
            // 
            this.Column8.HeaderText = "Category";
            this.Column8.Name = "Column8";
            this.Column8.ReadOnly = true;
            // 
            // Column9
            // 
            this.Column9.HeaderText = "1st Winner(s) | Num Votes";
            this.Column9.Name = "Column9";
            this.Column9.ReadOnly = true;
            // 
            // Column10
            // 
            this.Column10.HeaderText = "2nd Winner(s) | Num Votes";
            this.Column10.Name = "Column10";
            this.Column10.ReadOnly = true;
            // 
            // tabPage4
            // 
            this.tabPage4.AutoScroll = true;
            this.tabPage4.Controls.Add(this.btnSaveOptions);
            this.tabPage4.Location = new System.Drawing.Point(4, 25);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage4.Size = new System.Drawing.Size(792, 421);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Server Options";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // btnSaveOptions
            // 
            this.btnSaveOptions.Location = new System.Drawing.Point(5, 6);
            this.btnSaveOptions.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnSaveOptions.Name = "btnSaveOptions";
            this.btnSaveOptions.Size = new System.Drawing.Size(744, 25);
            this.btnSaveOptions.TabIndex = 0;
            this.btnSaveOptions.Text = "Save";
            this.btnSaveOptions.UseVisualStyleBackColor = true;
            this.btnSaveOptions.Click += new System.EventHandler(this.btnSaveOptions_Click);
            // 
            // tabCurrentQ
            // 
            this.tabCurrentQ.Controls.Add(this.dgvQueue);
            this.tabCurrentQ.Location = new System.Drawing.Point(4, 25);
            this.tabCurrentQ.Margin = new System.Windows.Forms.Padding(4);
            this.tabCurrentQ.Name = "tabCurrentQ";
            this.tabCurrentQ.Padding = new System.Windows.Forms.Padding(4);
            this.tabCurrentQ.Size = new System.Drawing.Size(792, 421);
            this.tabCurrentQ.TabIndex = 4;
            this.tabCurrentQ.Text = "Current Queue";
            this.tabCurrentQ.UseVisualStyleBackColor = true;
            // 
            // dgvQueue
            // 
            this.dgvQueue.AllowUserToAddRows = false;
            this.dgvQueue.AllowUserToDeleteRows = false;
            this.dgvQueue.AllowUserToResizeColumns = false;
            this.dgvQueue.AllowUserToResizeRows = false;
            this.dgvQueue.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvQueue.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvQueue.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column11,
            this.Column12});
            this.dgvQueue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvQueue.Location = new System.Drawing.Point(4, 4);
            this.dgvQueue.Margin = new System.Windows.Forms.Padding(4);
            this.dgvQueue.Name = "dgvQueue";
            this.dgvQueue.ReadOnly = true;
            this.dgvQueue.RowHeadersVisible = false;
            this.dgvQueue.Size = new System.Drawing.Size(784, 413);
            this.dgvQueue.TabIndex = 0;
            // 
            // Column11
            // 
            this.Column11.HeaderText = "Position In Q";
            this.Column11.Name = "Column11";
            this.Column11.ReadOnly = true;
            // 
            // Column12
            // 
            this.Column12.HeaderText = "Name";
            this.Column12.Name = "Column12";
            this.Column12.ReadOnly = true;
            // 
            // tabCurrentV
            // 
            this.tabCurrentV.Controls.Add(this.dgvCurrentVoters);
            this.tabCurrentV.Location = new System.Drawing.Point(4, 25);
            this.tabCurrentV.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabCurrentV.Name = "tabCurrentV";
            this.tabCurrentV.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabCurrentV.Size = new System.Drawing.Size(792, 421);
            this.tabCurrentV.TabIndex = 5;
            this.tabCurrentV.Text = "Current Voters";
            this.tabCurrentV.UseVisualStyleBackColor = true;
            // 
            // dgvCurrentVoters
            // 
            this.dgvCurrentVoters.AllowUserToAddRows = false;
            this.dgvCurrentVoters.AllowUserToDeleteRows = false;
            this.dgvCurrentVoters.AllowUserToResizeRows = false;
            this.dgvCurrentVoters.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvCurrentVoters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCurrentVoters.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.Column14,
            this.dataGridViewTextBoxColumn2,
            this.Column24});
            this.dgvCurrentVoters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCurrentVoters.Location = new System.Drawing.Point(3, 2);
            this.dgvCurrentVoters.Margin = new System.Windows.Forms.Padding(4);
            this.dgvCurrentVoters.Name = "dgvCurrentVoters";
            this.dgvCurrentVoters.ReadOnly = true;
            this.dgvCurrentVoters.RowHeadersVisible = false;
            this.dgvCurrentVoters.Size = new System.Drawing.Size(786, 417);
            this.dgvCurrentVoters.TabIndex = 1;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "IP";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // Column14
            // 
            this.Column14.HeaderText = "Connection";
            this.Column14.Name = "Column14";
            this.Column14.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "Name";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            // 
            // Column24
            // 
            this.Column24.HeaderText = "Category (one ahead)";
            this.Column24.Name = "Column24";
            this.Column24.ReadOnly = true;
            // 
            // tabPage7
            // 
            this.tabPage7.Controls.Add(this.btnSubmitManualVote);
            this.tabPage7.Controls.Add(this.btnReadyManualVote);
            this.tabPage7.Controls.Add(this.dgvManualVotes);
            this.tabPage7.Controls.Add(this.txtNameOfManualVote);
            this.tabPage7.Controls.Add(this.label1);
            this.tabPage7.Location = new System.Drawing.Point(4, 25);
            this.tabPage7.Name = "tabPage7";
            this.tabPage7.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage7.Size = new System.Drawing.Size(792, 421);
            this.tabPage7.TabIndex = 6;
            this.tabPage7.Text = "Manual Vote";
            this.tabPage7.UseVisualStyleBackColor = true;
            // 
            // btnSubmitManualVote
            // 
            this.btnSubmitManualVote.Location = new System.Drawing.Point(673, 5);
            this.btnSubmitManualVote.Name = "btnSubmitManualVote";
            this.btnSubmitManualVote.Size = new System.Drawing.Size(111, 23);
            this.btnSubmitManualVote.TabIndex = 4;
            this.btnSubmitManualVote.Text = "Submit";
            this.btnSubmitManualVote.UseVisualStyleBackColor = true;
            this.btnSubmitManualVote.Click += new System.EventHandler(this.btnSubmitManualVote_Click);
            // 
            // btnReadyManualVote
            // 
            this.btnReadyManualVote.Location = new System.Drawing.Point(443, 6);
            this.btnReadyManualVote.Name = "btnReadyManualVote";
            this.btnReadyManualVote.Size = new System.Drawing.Size(111, 23);
            this.btnReadyManualVote.TabIndex = 3;
            this.btnReadyManualVote.Text = "Ready";
            this.btnReadyManualVote.UseVisualStyleBackColor = true;
            this.btnReadyManualVote.Click += new System.EventHandler(this.btnPerformManualVote_Click);
            // 
            // dgvManualVotes
            // 
            this.dgvManualVotes.AllowUserToAddRows = false;
            this.dgvManualVotes.AllowUserToDeleteRows = false;
            this.dgvManualVotes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvManualVotes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvManualVotes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column5,
            this.Column15,
            this.Column16});
            this.dgvManualVotes.Location = new System.Drawing.Point(11, 31);
            this.dgvManualVotes.Name = "dgvManualVotes";
            this.dgvManualVotes.RowHeadersVisible = false;
            this.dgvManualVotes.RowTemplate.Height = 24;
            this.dgvManualVotes.Size = new System.Drawing.Size(773, 384);
            this.dgvManualVotes.TabIndex = 2;
            this.dgvManualVotes.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvManualVotes_CellEndEdit);
            // 
            // Column5
            // 
            this.Column5.HeaderText = "Category";
            this.Column5.Name = "Column5";
            // 
            // Column15
            // 
            this.Column15.HeaderText = "First Winner";
            this.Column15.Name = "Column15";
            // 
            // Column16
            // 
            this.Column16.HeaderText = "Second Winner";
            this.Column16.Name = "Column16";
            // 
            // txtNameOfManualVote
            // 
            this.txtNameOfManualVote.Location = new System.Drawing.Point(244, 6);
            this.txtNameOfManualVote.Name = "txtNameOfManualVote";
            this.txtNameOfManualVote.Size = new System.Drawing.Size(193, 22);
            this.txtNameOfManualVote.TabIndex = 1;
            this.txtNameOfManualVote.TextChanged += new System.EventHandler(this.txtNameOfManualVote_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(232, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Add a vote on behalf of (username)";
            // 
            // tabPage8
            // 
            this.tabPage8.Controls.Add(this.dgvBugReports);
            this.tabPage8.Location = new System.Drawing.Point(4, 25);
            this.tabPage8.Name = "tabPage8";
            this.tabPage8.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage8.Size = new System.Drawing.Size(792, 421);
            this.tabPage8.TabIndex = 7;
            this.tabPage8.Text = "Bug Reports";
            this.tabPage8.UseVisualStyleBackColor = true;
            // 
            // dgvBugReports
            // 
            this.dgvBugReports.AllowUserToAddRows = false;
            this.dgvBugReports.AllowUserToDeleteRows = false;
            this.dgvBugReports.AllowUserToResizeRows = false;
            this.dgvBugReports.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvBugReports.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBugReports.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column17,
            this.Column18,
            this.Column19,
            this.Column20,
            this.Column21,
            this.Column23,
            this.Column22});
            this.dgvBugReports.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvBugReports.Location = new System.Drawing.Point(3, 3);
            this.dgvBugReports.Name = "dgvBugReports";
            this.dgvBugReports.ReadOnly = true;
            this.dgvBugReports.RowHeadersVisible = false;
            this.dgvBugReports.RowTemplate.Height = 24;
            this.dgvBugReports.Size = new System.Drawing.Size(786, 415);
            this.dgvBugReports.TabIndex = 0;
            this.dgvBugReports.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvBugReports_CellContentClick);
            // 
            // Column17
            // 
            this.Column17.FillWeight = 5F;
            this.Column17.HeaderText = "ID";
            this.Column17.Name = "Column17";
            this.Column17.ReadOnly = true;
            // 
            // Column18
            // 
            this.Column18.FillWeight = 5F;
            this.Column18.HeaderText = "State";
            this.Column18.Name = "Column18";
            this.Column18.ReadOnly = true;
            // 
            // Column19
            // 
            this.Column19.FillWeight = 10F;
            this.Column19.HeaderText = "Type";
            this.Column19.Name = "Column19";
            this.Column19.ReadOnly = true;
            // 
            // Column20
            // 
            this.Column20.FillWeight = 30F;
            this.Column20.HeaderText = "Reporter";
            this.Column20.Name = "Column20";
            this.Column20.ReadOnly = true;
            // 
            // Column21
            // 
            this.Column21.FillWeight = 35F;
            this.Column21.HeaderText = "Primary";
            this.Column21.Name = "Column21";
            this.Column21.ReadOnly = true;
            // 
            // Column23
            // 
            this.Column23.FillWeight = 35F;
            this.Column23.HeaderText = "Additional";
            this.Column23.Name = "Column23";
            this.Column23.ReadOnly = true;
            // 
            // Column22
            // 
            this.Column22.FillWeight = 10F;
            this.Column22.HeaderText = "Submit";
            this.Column22.Name = "Column22";
            this.Column22.ReadOnly = true;
            // 
            // uiLockTab
            // 
            this.uiLockTab.Controls.Add(this.btnInputLock);
            this.uiLockTab.Controls.Add(this.txtInputLock);
            this.uiLockTab.Controls.Add(this.lblInputLock);
            this.uiLockTab.Location = new System.Drawing.Point(4, 25);
            this.uiLockTab.Name = "uiLockTab";
            this.uiLockTab.Padding = new System.Windows.Forms.Padding(3);
            this.uiLockTab.Size = new System.Drawing.Size(792, 421);
            this.uiLockTab.TabIndex = 8;
            this.uiLockTab.Text = "UI Lock";
            this.uiLockTab.UseVisualStyleBackColor = true;
            // 
            // btnInputLock
            // 
            this.btnInputLock.Location = new System.Drawing.Point(66, 211);
            this.btnInputLock.Name = "btnInputLock";
            this.btnInputLock.Size = new System.Drawing.Size(229, 23);
            this.btnInputLock.TabIndex = 2;
            this.btnInputLock.Text = "Lock / Unlock";
            this.btnInputLock.UseVisualStyleBackColor = true;
            this.btnInputLock.Click += new System.EventHandler(this.btnInputLock_Click);
            // 
            // txtInputLock
            // 
            this.txtInputLock.Location = new System.Drawing.Point(66, 183);
            this.txtInputLock.MaxLength = 8;
            this.txtInputLock.Name = "txtInputLock";
            this.txtInputLock.PasswordChar = '*';
            this.txtInputLock.Size = new System.Drawing.Size(146, 22);
            this.txtInputLock.TabIndex = 1;
            // 
            // lblInputLock
            // 
            this.lblInputLock.AutoSize = true;
            this.lblInputLock.Location = new System.Drawing.Point(218, 186);
            this.lblInputLock.Name = "lblInputLock";
            this.lblInputLock.Size = new System.Drawing.Size(77, 17);
            this.lblInputLock.TabIndex = 0;
            this.lblInputLock.Text = "Enter input";
            // 
            // queueTimer
            // 
            this.queueTimer.Enabled = true;
            this.queueTimer.Interval = 10000;
            this.queueTimer.Tick += new System.EventHandler(this.queueTimer_Tick);
            // 
            // UIForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "UIForm";
            this.Text = "UIForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UIForm_FormClosing);
            this.Load += new System.EventHandler(this.UIForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvStudents)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCategories)).EndInit();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvWinners)).EndInit();
            this.tabPage4.ResumeLayout(false);
            this.tabCurrentQ.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvQueue)).EndInit();
            this.tabCurrentV.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCurrentVoters)).EndInit();
            this.tabPage7.ResumeLayout(false);
            this.tabPage7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvManualVotes)).EndInit();
            this.tabPage8.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvBugReports)).EndInit();
            this.uiLockTab.ResumeLayout(false);
            this.uiLockTab.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.DataGridView dgvCategories;
        private System.Windows.Forms.DataGridView dgvWinners;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Timer queueTimer;
        private System.Windows.Forms.Button btnSaveOptions;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column6;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column7;
        private System.Windows.Forms.TabPage tabCurrentQ;
        private System.Windows.Forms.DataGridView dgvQueue;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column11;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column12;
        private System.Windows.Forms.DataGridView dgvStudents;
        private System.Windows.Forms.TabPage tabCurrentV;
        private System.Windows.Forms.DataGridView dgvCurrentVoters;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column13;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column8;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column9;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column10;
        private System.Windows.Forms.TabPage tabPage7;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtNameOfManualVote;
        private System.Windows.Forms.DataGridView dgvManualVotes;
        private System.Windows.Forms.Button btnReadyManualVote;
        private System.Windows.Forms.Button btnSubmitManualVote;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column15;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column16;
        private System.Windows.Forms.TabPage tabPage8;
        private System.Windows.Forms.DataGridView dgvBugReports;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column17;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column18;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column19;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column20;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column21;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column23;
        private System.Windows.Forms.DataGridViewButtonColumn Column22;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column14;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column24;
        private System.Windows.Forms.TabPage uiLockTab;
        private System.Windows.Forms.TextBox txtInputLock;
        private System.Windows.Forms.Label lblInputLock;
        private System.Windows.Forms.Button btnInputLock;
    }
}