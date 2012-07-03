﻿namespace MySQL.ForExcel
{
  partial class EditDataForm
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
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      this.btnCommit = new System.Windows.Forms.Button();
      this.btnRevert = new System.Windows.Forms.Button();
      this.chkAutoCommit = new System.Windows.Forms.CheckBox();
      this.chkRefreshFromDB = new System.Windows.Forms.CheckBox();
      this.grdPreview = new System.Windows.Forms.DataGridView();
      this.contentAreaPanel.SuspendLayout();
      this.commandAreaPanel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grdPreview)).BeginInit();
      this.SuspendLayout();
      // 
      // contentAreaPanel
      // 
      this.contentAreaPanel.BackColor = System.Drawing.SystemColors.Window;
      this.contentAreaPanel.Controls.Add(this.chkAutoCommit);
      this.contentAreaPanel.Controls.Add(this.chkRefreshFromDB);
      this.contentAreaPanel.Controls.Add(this.grdPreview);
      this.contentAreaPanel.Size = new System.Drawing.Size(524, 301);
      // 
      // commandAreaPanel
      // 
      this.commandAreaPanel.Controls.Add(this.btnCommit);
      this.commandAreaPanel.Controls.Add(this.btnRevert);
      this.commandAreaPanel.Location = new System.Drawing.Point(0, 302);
      this.commandAreaPanel.Size = new System.Drawing.Size(524, 45);
      // 
      // btnCommit
      // 
      this.btnCommit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCommit.Location = new System.Drawing.Point(370, 12);
      this.btnCommit.Name = "btnCommit";
      this.btnCommit.Size = new System.Drawing.Size(143, 23);
      this.btnCommit.TabIndex = 1;
      this.btnCommit.Text = "Commit Changes";
      this.btnCommit.UseVisualStyleBackColor = true;
      this.btnCommit.Click += new System.EventHandler(this.btnCommit_Click);
      // 
      // btnRevert
      // 
      this.btnRevert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnRevert.Location = new System.Drawing.Point(12, 12);
      this.btnRevert.Name = "btnRevert";
      this.btnRevert.Size = new System.Drawing.Size(110, 23);
      this.btnRevert.TabIndex = 0;
      this.btnRevert.Text = "Revert Data";
      this.btnRevert.UseVisualStyleBackColor = true;
      this.btnRevert.Click += new System.EventHandler(this.btnRevert_Click);
      // 
      // chkAutoCommit
      // 
      this.chkAutoCommit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.chkAutoCommit.AutoSize = true;
      this.chkAutoCommit.BackColor = System.Drawing.Color.Transparent;
      this.chkAutoCommit.Location = new System.Drawing.Point(382, 279);
      this.chkAutoCommit.Name = "chkAutoCommit";
      this.chkAutoCommit.Size = new System.Drawing.Size(130, 17);
      this.chkAutoCommit.TabIndex = 2;
      this.chkAutoCommit.Text = "Auto-Commit Changes";
      this.chkAutoCommit.UseVisualStyleBackColor = false;
      this.chkAutoCommit.CheckedChanged += new System.EventHandler(this.chkAutoCommit_CheckedChanged);
      // 
      // chkRefreshFromDB
      // 
      this.chkRefreshFromDB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.chkRefreshFromDB.AutoSize = true;
      this.chkRefreshFromDB.BackColor = System.Drawing.Color.Transparent;
      this.chkRefreshFromDB.Checked = true;
      this.chkRefreshFromDB.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkRefreshFromDB.Location = new System.Drawing.Point(12, 279);
      this.chkRefreshFromDB.Name = "chkRefreshFromDB";
      this.chkRefreshFromDB.Size = new System.Drawing.Size(104, 17);
      this.chkRefreshFromDB.TabIndex = 1;
      this.chkRefreshFromDB.Text = "Refresh from DB";
      this.chkRefreshFromDB.UseVisualStyleBackColor = false;
      // 
      // grdPreview
      // 
      this.grdPreview.AllowUserToAddRows = false;
      this.grdPreview.AllowUserToDeleteRows = false;
      this.grdPreview.AllowUserToOrderColumns = true;
      this.grdPreview.AllowUserToResizeColumns = false;
      this.grdPreview.AllowUserToResizeRows = false;
      this.grdPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grdPreview.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
      this.grdPreview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.Transparent;
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.grdPreview.DefaultCellStyle = dataGridViewCellStyle2;
      this.grdPreview.Location = new System.Drawing.Point(12, 11);
      this.grdPreview.MultiSelect = false;
      this.grdPreview.Name = "grdPreview";
      this.grdPreview.ReadOnly = true;
      this.grdPreview.RowHeadersVisible = false;
      this.grdPreview.Size = new System.Drawing.Size(500, 252);
      this.grdPreview.TabIndex = 0;
      // 
      // EditDataForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(524, 348);
      this.CommandAreaHeight = 45;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
      this.MinimumSize = new System.Drawing.Size(300, 300);
      this.Name = "EditDataForm";
      this.Text = "Edit Data - SheetName [Range]";
      this.Activated += new System.EventHandler(this.EditDataForm_Activated);
      this.Deactivate += new System.EventHandler(this.EditDataForm_Deactivate);
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EditDataForm_FormClosed);
      this.contentAreaPanel.ResumeLayout(false);
      this.contentAreaPanel.PerformLayout();
      this.commandAreaPanel.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grdPreview)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnCommit;
    private System.Windows.Forms.Button btnRevert;
    private System.Windows.Forms.CheckBox chkAutoCommit;
    private System.Windows.Forms.CheckBox chkRefreshFromDB;
    private System.Windows.Forms.DataGridView grdPreview;
  }
}