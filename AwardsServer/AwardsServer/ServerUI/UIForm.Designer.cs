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
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.dgvQueue = new System.Windows.Forms.DataGridView();
            this.Column11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.queueTimer = new System.Windows.Forms.Timer(this.components);
            this.Column13 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStudents)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCategories)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWinners)).BeginInit();
            this.tabPage4.SuspendLayout();
            this.tabPage5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvQueue)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(600, 366);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dgvStudents);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage1.Size = new System.Drawing.Size(592, 340);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Students";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // dgvStudents
            // 
            this.dgvStudents.AllowUserToAddRows = false;
            this.dgvStudents.AllowUserToDeleteRows = false;
            this.dgvStudents.AllowUserToResizeColumns = false;
            this.dgvStudents.AllowUserToResizeRows = false;
            this.dgvStudents.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvStudents.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvStudents.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column13,
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column5,
            this.Column4});
            this.dgvStudents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvStudents.Location = new System.Drawing.Point(2, 2);
            this.dgvStudents.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dgvStudents.Name = "dgvStudents";
            this.dgvStudents.RowHeadersVisible = false;
            this.dgvStudents.RowTemplate.Height = 24;
            this.dgvStudents.Size = new System.Drawing.Size(588, 336);
            this.dgvStudents.TabIndex = 0;
            this.dgvStudents.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dgvStudents_CellBeginEdit);
            this.dgvStudents.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvStudents_CellEndEdit);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dgvCategories);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage2.Size = new System.Drawing.Size(592, 340);
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
            this.dgvCategories.Location = new System.Drawing.Point(2, 2);
            this.dgvCategories.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dgvCategories.Name = "dgvCategories";
            this.dgvCategories.RowHeadersVisible = false;
            this.dgvCategories.RowTemplate.Height = 24;
            this.dgvCategories.Size = new System.Drawing.Size(588, 336);
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
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage3.Size = new System.Drawing.Size(592, 340);
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
            this.dgvWinners.Location = new System.Drawing.Point(2, 2);
            this.dgvWinners.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dgvWinners.Name = "dgvWinners";
            this.dgvWinners.ReadOnly = true;
            this.dgvWinners.RowHeadersVisible = false;
            this.dgvWinners.RowTemplate.Height = 24;
            this.dgvWinners.Size = new System.Drawing.Size(588, 336);
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
            this.Column9.HeaderText = "Male Winner(s) | Num Votes";
            this.Column9.Name = "Column9";
            this.Column9.ReadOnly = true;
            // 
            // Column10
            // 
            this.Column10.HeaderText = "Female Winner(s) | Num Votes";
            this.Column10.Name = "Column10";
            this.Column10.ReadOnly = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.btnSaveOptions);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage4.Size = new System.Drawing.Size(592, 340);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Server Options";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // btnSaveOptions
            // 
            this.btnSaveOptions.Location = new System.Drawing.Point(4, 5);
            this.btnSaveOptions.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnSaveOptions.Name = "btnSaveOptions";
            this.btnSaveOptions.Size = new System.Drawing.Size(585, 20);
            this.btnSaveOptions.TabIndex = 0;
            this.btnSaveOptions.Text = "Save";
            this.btnSaveOptions.UseVisualStyleBackColor = true;
            this.btnSaveOptions.Click += new System.EventHandler(this.btnSaveOptions_Click);
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.dgvQueue);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPage5.Size = new System.Drawing.Size(592, 340);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Current Queue";
            this.tabPage5.UseVisualStyleBackColor = true;
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
            this.dgvQueue.Location = new System.Drawing.Point(3, 3);
            this.dgvQueue.Name = "dgvQueue";
            this.dgvQueue.ReadOnly = true;
            this.dgvQueue.RowHeadersVisible = false;
            this.dgvQueue.Size = new System.Drawing.Size(586, 334);
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
            // queueTimer
            // 
            this.queueTimer.Enabled = true;
            this.queueTimer.Interval = 10000;
            this.queueTimer.Tick += new System.EventHandler(this.queueTimer_Tick);
            // 
            // Column13
            // 
            this.Column13.HeaderText = "Acc. Name";
            this.Column13.Name = "Column13";
            this.Column13.ReadOnly = true;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "Name";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "Last Name";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "Tutor Group";
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            // 
            // Column5
            // 
            this.Column5.HeaderText = "Sex";
            this.Column5.Name = "Column5";
            this.Column5.ReadOnly = true;
            // 
            // Column4
            // 
            this.Column4.HeaderText = "Voted";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            // 
            // UIForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 366);
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
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
            this.tabPage5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvQueue)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.DataGridView dgvStudents;
        private System.Windows.Forms.DataGridView dgvCategories;
        private System.Windows.Forms.DataGridView dgvWinners;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Timer queueTimer;
        private System.Windows.Forms.Button btnSaveOptions;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column6;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column7;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.DataGridView dgvQueue;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column11;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column12;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column8;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column9;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column10;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column13;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column4;
    }
}