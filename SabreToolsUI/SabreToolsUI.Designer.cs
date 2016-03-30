namespace SabreTools
{
	partial class SabreToolsUI
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
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.generateDatLabel = new System.Windows.Forms.Label();
			this.systemsLabel = new System.Windows.Forms.Label();
			this.systemsCheckedListBox = new System.Windows.Forms.CheckedListBox();
			this.sourcesCheckedListBox = new System.Windows.Forms.CheckedListBox();
			this.sourcesLabel = new System.Windows.Forms.Label();
			this.generateButton = new System.Windows.Forms.Button();
			this.generateAllButton = new System.Windows.Forms.Button();
			this.oldCheckBox = new System.Windows.Forms.CheckBox();
			this.renameCheckBox = new System.Windows.Forms.CheckBox();
			this.importDatLabel = new System.Windows.Forms.Label();
			this.importTextBox = new System.Windows.Forms.TextBox();
			this.fileButton = new System.Windows.Forms.Button();
			this.importButton = new System.Windows.Forms.Button();
			this.folderButton = new System.Windows.Forms.Button();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.helpToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(681, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.quitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// quitToolStripMenuItem
			// 
			this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
			this.quitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.X)));
			this.quitToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
			this.quitToolStripMenuItem.Text = "Exit";
			this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
			// 
			// editToolStripMenuItem
			// 
			this.editToolStripMenuItem.Name = "editToolStripMenuItem";
			this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
			this.editToolStripMenuItem.Text = "Edit";
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.helpToolStripMenuItem.Text = "Help";
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
			this.aboutToolStripMenuItem.Text = "About";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
			// 
			// generateDatLabel
			// 
			this.generateDatLabel.AutoSize = true;
			this.generateDatLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.generateDatLabel.Location = new System.Drawing.Point(12, 34);
			this.generateDatLabel.Name = "generateDatLabel";
			this.generateDatLabel.Size = new System.Drawing.Size(112, 17);
			this.generateDatLabel.TabIndex = 1;
			this.generateDatLabel.Text = "Generate DAT";
			// 
			// systemsLabel
			// 
			this.systemsLabel.AutoSize = true;
			this.systemsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.systemsLabel.Location = new System.Drawing.Point(15, 65);
			this.systemsLabel.Name = "systemsLabel";
			this.systemsLabel.Size = new System.Drawing.Size(56, 15);
			this.systemsLabel.TabIndex = 3;
			this.systemsLabel.Text = "Systems:";
			// 
			// systemsCheckedListBox
			// 
			this.systemsCheckedListBox.FormattingEnabled = true;
			this.systemsCheckedListBox.Items.AddRange(Helper.GetAllSystems());
			this.systemsCheckedListBox.Location = new System.Drawing.Point(77, 65);
			this.systemsCheckedListBox.Name = "systemsCheckedListBox";
			this.systemsCheckedListBox.Size = new System.Drawing.Size(260, 34);
			this.systemsCheckedListBox.TabIndex = 4;
			// 
			// sourcesCheckedListBox
			// 
			this.sourcesCheckedListBox.FormattingEnabled = true;
			this.sourcesCheckedListBox.Items.AddRange(Helper.GetAllSources());
			this.sourcesCheckedListBox.Location = new System.Drawing.Point(415, 65);
			this.sourcesCheckedListBox.Name = "sourcesCheckedListBox";
			this.sourcesCheckedListBox.Size = new System.Drawing.Size(244, 34);
			this.sourcesCheckedListBox.TabIndex = 7;
			// 
			// sourcesLabel
			// 
			this.sourcesLabel.AutoSize = true;
			this.sourcesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.sourcesLabel.Location = new System.Drawing.Point(353, 65);
			this.sourcesLabel.Name = "sourcesLabel";
			this.sourcesLabel.Size = new System.Drawing.Size(55, 15);
			this.sourcesLabel.TabIndex = 6;
			this.sourcesLabel.Text = "Sources:";
			// 
			// generateButton
			// 
			this.generateButton.Location = new System.Drawing.Point(18, 128);
			this.generateButton.Name = "generateButton";
			this.generateButton.Size = new System.Drawing.Size(75, 23);
			this.generateButton.TabIndex = 8;
			this.generateButton.Text = "Generate";
			this.generateButton.UseVisualStyleBackColor = true;
			this.generateButton.Click += new System.EventHandler(this.generateButton_Click);
			// 
			// generateAllButton
			// 
			this.generateAllButton.Location = new System.Drawing.Point(100, 128);
			this.generateAllButton.Name = "generateAllButton";
			this.generateAllButton.Size = new System.Drawing.Size(97, 23);
			this.generateAllButton.TabIndex = 9;
			this.generateAllButton.Text = "Generate All";
			this.generateAllButton.UseVisualStyleBackColor = true;
			this.generateAllButton.Click += new System.EventHandler(this.generateAllButton_Click);
			// 
			// oldCheckBox
			// 
			this.oldCheckBox.AutoSize = true;
			this.oldCheckBox.Location = new System.Drawing.Point(18, 105);
			this.oldCheckBox.Name = "oldCheckBox";
			this.oldCheckBox.Size = new System.Drawing.Size(129, 17);
			this.oldCheckBox.TabIndex = 10;
			this.oldCheckBox.Text = "Use RomVault Format";
			this.oldCheckBox.UseVisualStyleBackColor = true;
			// 
			// renameCheckBox
			// 
			this.renameCheckBox.AutoSize = true;
			this.renameCheckBox.Checked = true;
			this.renameCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.renameCheckBox.Location = new System.Drawing.Point(154, 105);
			this.renameCheckBox.Name = "renameCheckBox";
			this.renameCheckBox.Size = new System.Drawing.Size(102, 17);
			this.renameCheckBox.TabIndex = 11;
			this.renameCheckBox.Text = "Rename Games";
			this.renameCheckBox.UseVisualStyleBackColor = true;
			// 
			// importDatLabel
			// 
			this.importDatLabel.AutoSize = true;
			this.importDatLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.importDatLabel.Location = new System.Drawing.Point(12, 168);
			this.importDatLabel.Name = "importDatLabel";
			this.importDatLabel.Size = new System.Drawing.Size(89, 17);
			this.importDatLabel.TabIndex = 12;
			this.importDatLabel.Text = "Import DAT";
			// 
			// importTextBox
			// 
			this.importTextBox.Location = new System.Drawing.Point(100, 201);
			this.importTextBox.Name = "importTextBox";
			this.importTextBox.Size = new System.Drawing.Size(405, 20);
			this.importTextBox.TabIndex = 14;
			// 
			// fileButton
			// 
			this.fileButton.Location = new System.Drawing.Point(511, 199);
			this.fileButton.Name = "fileButton";
			this.fileButton.Size = new System.Drawing.Size(33, 23);
			this.fileButton.TabIndex = 15;
			this.fileButton.Text = "File";
			this.fileButton.UseVisualStyleBackColor = true;
			this.fileButton.Click += new System.EventHandler(this.fileButton_Click);
			// 
			// importButton
			// 
			this.importButton.Location = new System.Drawing.Point(15, 199);
			this.importButton.Name = "importButton";
			this.importButton.Size = new System.Drawing.Size(75, 23);
			this.importButton.TabIndex = 16;
			this.importButton.Text = "Import";
			this.importButton.UseVisualStyleBackColor = true;
			this.importButton.Click += new System.EventHandler(this.importButton_Click);
			// 
			// folderButton
			// 
			this.folderButton.Location = new System.Drawing.Point(550, 199);
			this.folderButton.Name = "folderButton";
			this.folderButton.Size = new System.Drawing.Size(46, 23);
			this.folderButton.TabIndex = 17;
			this.folderButton.Text = "Folder";
			this.folderButton.UseVisualStyleBackColor = true;
			this.folderButton.Click += new System.EventHandler(this.folderButton_Click);
			// 
			// SabreToolsUI
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(681, 257);
			this.Controls.Add(this.folderButton);
			this.Controls.Add(this.importButton);
			this.Controls.Add(this.fileButton);
			this.Controls.Add(this.importTextBox);
			this.Controls.Add(this.importDatLabel);
			this.Controls.Add(this.renameCheckBox);
			this.Controls.Add(this.oldCheckBox);
			this.Controls.Add(this.generateAllButton);
			this.Controls.Add(this.generateButton);
			this.Controls.Add(this.sourcesCheckedListBox);
			this.Controls.Add(this.sourcesLabel);
			this.Controls.Add(this.systemsCheckedListBox);
			this.Controls.Add(this.systemsLabel);
			this.Controls.Add(this.generateDatLabel);
			this.Controls.Add(this.menuStrip1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "SabreToolsUI";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "SabreTools UI 0.2.4.0";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.Label generateDatLabel;
		private System.Windows.Forms.Label systemsLabel;
		private System.Windows.Forms.CheckedListBox systemsCheckedListBox;
		private System.Windows.Forms.CheckedListBox sourcesCheckedListBox;
		private System.Windows.Forms.Label sourcesLabel;
		private System.Windows.Forms.Button generateButton;
		private System.Windows.Forms.Button generateAllButton;
		private System.Windows.Forms.CheckBox oldCheckBox;
		private System.Windows.Forms.CheckBox renameCheckBox;
		private System.Windows.Forms.Label importDatLabel;
		private System.Windows.Forms.TextBox importTextBox;
		private System.Windows.Forms.Button fileButton;
		private System.Windows.Forms.Button importButton;
		private System.Windows.Forms.Button folderButton;
	}
}

