﻿// Copyright (c) 2012, 2014, Oracle and/or its affiliates. All rights reserved.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation; version 2 of the
// License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
// 02110-1301  USA

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MySQL.ForExcel.Classes;
using MySQL.ForExcel.Properties;
using MySQL.Utility.Classes;
using MySQL.Utility.Forms;
using ExcelInterop = Microsoft.Office.Interop.Excel;

namespace MySQL.ForExcel.Forms
{
  /// <summary>
  /// Previews a MySQL table's data and lets users select columns and rows to import to an Excel spreadsheet.
  /// </summary>
  public partial class ImportTableViewForm : AutoStyleableBaseDialog
  {
    #region Fields

    /// <summary>
    ///  The active workbook's max row number.
    /// </summary>
    private readonly int _activeWorkbookMaxRowNumber;

    /// <summary>
    /// The current row we will attempt to import data to.
    /// </summary>
    private readonly int _atRow;

    /// <summary>
    /// The type of DB object (MySQL table or view) from which to import data to the active Excel Worksheet.
    /// </summary>
    private readonly DbView _dbTableOrView;

    /// <summary>
    /// The list of columns selected by the user to be imported to Excel.
    /// </summary>
    private List<string> _importColumns;

    /// <summary>
    /// Indicates whether the number of rows being imported could be truncated since it exceeds the number of available rows in the Worksheet.
    /// </summary>
    private bool _importIsCappedByRows;

    /// <summary>
    /// A <see cref="DataTable"/> object containing a subset of the whole data which is shown in the preview grid.
    /// </summary>
    private DataTable _previewDataTable;

    /// <summary>
    /// The total rows contained in the MySQL table or view selected for import.
    /// </summary>
    private long _totalRowsCount;

    #endregion Fields

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportTableViewForm"/> class.
    /// </summary>
    /// <param name="importDbTableOrView">MySQL table or view from which to import data to an Excel spreadsheet.</param>
    /// <param name="importToWorksheetName">Name of the Excel worksheet where the data will be imported to.</param>
    public ImportTableViewForm(DbView importDbTableOrView, string importToWorksheetName)
    {
      if (importDbTableOrView == null)
      {
        throw new ArgumentNullException("importDbTableOrView");
      }

      _dbTableOrView = importDbTableOrView;
      _importColumns = null;
      _previewDataTable = null;
      MySqlTable = null;
      _activeWorkbookMaxRowNumber = Globals.ThisAddIn.ActiveWorkbook.GetWorkbookMaxRowNumber();
      var atCell = Globals.ThisAddIn.Application.ActiveCell;
      _atRow = atCell == null ? 1 : atCell.Row;
      InitializeComponent();

      PreviewDataGridView.DataError += PreviewDataGridView_DataError;
      TableNameMainLabel.Text = importDbTableOrView is DbTable ? "Table Name" : "View Name";
      PickColumnsSubLabel.Text = string.Format(Resources.ImportTableOrViewSubText, importDbTableOrView is DbTable ? "table" : "view");
      OptionsWarningLabel.Text = Resources.ImportDataWillBeTruncatedWarning;
      Text = @"Import Data - " + importToWorksheetName;
      TableNameSubLabel.Text = importDbTableOrView.Name;
      FillPreviewGrid();
      InitializeOptions();
    }

    #region Properties

    /// <summary>
    /// Gets the <see cref="MySqlDataTable"/> object containing the data to be imported to the active Excel Worksheet.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public MySqlDataTable MySqlTable { get; private set; }

    /// <summary>
    /// Gets or sets the text associated with this control.
    /// </summary>
    public override sealed string Text
    {
      get
      {
        return base.Text;
      }

      set
      {
        base.Text = value;
      }
    }

    /// <summary>
    /// Gets a value indicating the which should be index of the first row obtained by the select query.
    /// </summary>
    private int FirstRowIndex
    {
      get
      {
        var firstRowIndex = (int)FromRowNumericUpDown.Value - 1;
        return (LimitRowsCheckBox.Checked) ? firstRowIndex : -1;
      }
    }

    /// <summary>
    /// Gets a value indicating the the number of rows to be obtained by the select query after the first row.
    /// </summary>
    private int RowsTo
    {
      get
      {
        var rowCount = (int)RowsToReturnNumericUpDown.Value;
        return LimitRowsCheckBox.Checked ? rowCount > _activeWorkbookMaxRowNumber ? _activeWorkbookMaxRowNumber : rowCount : -1;
      }
    }

    #endregion Properties

    /// <summary>
    /// Hides the Import form from the user at the same time it fakes the click over OK, silently opening an edit session.
    /// </summary>
    /// <returns>Always DialogResult.OK</returns>
    public DialogResult ImportHidingDialog()
    {
      bool success = ImportData();
      return success ? DialogResult.OK : DialogResult.Cancel;
    }

    /// <summary>
    /// Handles the CheckedChanged event of the AddSummaryFieldsCheckBox control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void AddSummaryFieldsCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      RecalculateRowsToMaximum();
    }

    /// <summary>
    /// Event delegate method fired when the <see cref="AdvancedOptionsButton"/> button is clicked.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event arguments.</param>
    private void AdvancedOptionsButton_Click(object sender, EventArgs e)
    {
      using (var optionsDialog = new ImportAdvancedOptionsDialog())
      {
        optionsDialog.ShowDialog();
        if (optionsDialog.ParentFormRequiresRefresh)
        {
          FillPreviewGrid();
        }

        AddSummaryFieldsCheckBox.Checked = Settings.Default.ImportCreateExcelTable && AddSummaryFieldsCheckBox.Checked;
        AddSummaryFieldsCheckBox.Enabled = Settings.Default.ImportCreateExcelTable;
        RecalculateRowsToMaximum();
      }
    }

    /// <summary>
    /// Event delegate method fired when the <see cref="ContextMenuForGrid"/> context menu strip is opening.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event arguments.</param>
    private void ContextMenuForGrid_Opening(object sender, CancelEventArgs e)
    {
      SelectAllToolStripMenuItem.Visible = PreviewDataGridView.SelectedColumns.Count < PreviewDataGridView.Columns.Count;
      SelectNoneToolStripMenuItem.Visible = PreviewDataGridView.SelectedColumns.Count > 0;
    }

    /// <summary>
    /// Prepares and fills the preview grid with data.
    /// </summary>
    private void FillPreviewGrid()
    {
      _importColumns = null;
      SetImportParameterValues(Settings.Default.ImportPreviewRowsQuantity);
      _previewDataTable = _dbTableOrView.GetData();
      _totalRowsCount = _dbTableOrView.GetRowsCount();
      RowsCountSubLabel.Text = _totalRowsCount.ToString(CultureInfo.InvariantCulture);
      PreviewDataGridView.DataSource = _previewDataTable;
      foreach (DataGridViewColumn gridCol in PreviewDataGridView.Columns)
      {
        gridCol.SortMode = DataGridViewColumnSortMode.NotSortable;
      }

      PreviewDataGridView.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
      FromRowNumericUpDown.Maximum = _totalRowsCount;
      _importIsCappedByRows = ExcelUtilities.CheckIfRowsExceedWorksheetLimit(_totalRowsCount);
      SetCompatibilityWarningControlsVisibility(_importIsCappedByRows);
      RowsToReturnNumericUpDown.Maximum = Math.Min(FromRowNumericUpDown.Maximum, _activeWorkbookMaxRowNumber - _atRow - FromRowNumericUpDown.Value + 1);
    }

    /// <summary>
    /// Event delegate method fired when the value of the <see cref="FromRowNumericUpDown"/> control changes.
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="e">Event arguments</param>
    private void FromRowNumericUpDown_ValueChanged(object sender, EventArgs e)
    {
      RecalculateRowsToMaximum();
    }

    /// <summary>
    /// Imports the selected MySQL table's data into the active Excel worksheet.
    /// </summary>
    /// <returns><c>true</c> if the import is successful, <c>false</c> if errors were found during the import.</returns>
    private bool ImportData()
    {
      _importColumns = null;
      var selectedColumns = new List<DataGridViewColumn>();
      if (PreviewDataGridView.SelectedColumns.Count < PreviewDataGridView.Columns.Count)
      {
        _importColumns = new List<string>(PreviewDataGridView.SelectedColumns.Count);
        selectedColumns.AddRange(PreviewDataGridView.SelectedColumns.Cast<DataGridViewColumn>());
        if (selectedColumns.Count > 1)
        {
          selectedColumns.Sort((c1, c2) => c1.Index.CompareTo(c2.Index));
        }

        _importColumns.AddRange(selectedColumns.Select(selCol => selCol.HeaderText));
      }

      Cursor = Cursors.WaitCursor;

      // If the importing data exceeds the number of available rows and no row limit was set we will force it for the Select Query.
      SetImportParameterValues(Settings.Default.ImportCreateExcelTable && !LimitRowsCheckBox.Checked && _importIsCappedByRows ? (int)RowsToReturnNumericUpDown.Maximum : RowsTo);

      // Import data into Excel
      var importTuple = _dbTableOrView.ImportData();
      MySqlTable = importTuple != null ? importTuple.Item1 : null;

      Cursor = Cursors.Default;
      return MySqlTable != null;
    }

    /// <summary>
    /// Event delegate method fired when the <see cref="ImportTableViewForm"/> is closing.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event arguments.</param>
    private void ImportTableViewForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      if (DialogResult == DialogResult.OK)
      {
        e.Cancel = !ImportData();
      }
    }

    /// <summary>
    /// Sets the initial state of controls representing user options.
    /// </summary>
    private void InitializeOptions()
    {
      IncludeHeadersCheckBox.Checked = true;
      LimitRowsCheckBox.Checked = false;
      AddSummaryFieldsCheckBox.Enabled = Settings.Default.ImportCreateExcelTable;
    }

    /// <summary>
    /// Handles the CheckedChanged event of the IncludeHeadersCheckBox control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void IncludeHeadersCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      RecalculateRowsToMaximum();
    }

    /// <summary>
    /// Event delegate method fired when the <see cref="LimitRowsCheckBox"/> checked state changes.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event arguments.</param>
    private void LimitRowsCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      bool limitRows = LimitRowsCheckBox.Checked;
      FromRowNumericUpDown.Enabled = limitRows;
      RowsToReturnNumericUpDown.Enabled = limitRows;
      if (!limitRows || !RowsToReturnNumericUpDown.CanFocus)
      {
        return;
      }

      // Give focus to the field related to the checkbox whose status changed.
      RowsToReturnNumericUpDown.Focus();
      RowsToReturnNumericUpDown.Select(0, RowsToReturnNumericUpDown.Text.Length);
      AddSummaryFieldsCheckBox.Enabled = Settings.Default.ImportCreateExcelTable;
    }

    /// <summary>
    /// Event delegate method fired when the <see cref="PreviewDataGridView"/> grid is done with its data binding operation.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event arguments.</param>
    private void PreviewDataGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
    {
      PreviewDataGridView.SelectAll();
    }

    /// <summary>
    /// Event delegate method fired when the <see cref="PreviewDataGridView"/> detects a data error in one of its cells.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event arguments.</param>
    private void PreviewDataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
    {
      if (PreviewDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].ValueType != Type.GetType("System.Byte[]"))
      {
        return;
      }

      try
      {
        var img = (byte[])(PreviewDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex]).Value;
        using (var ms = new MemoryStream(img))
        {
          Image.FromStream(ms);
        }
      }
      catch (ArgumentException argEx)
      {
        MySqlSourceTrace.WriteAppErrorToLog(argEx);
      }
      catch (Exception ex)
      {
        MiscUtilities.ShowCustomizedErrorDialog(Resources.DataLoadingError, ex.Message);
        MySqlSourceTrace.WriteAppErrorToLog(ex);
      }
    }

    /// <summary>
    /// Event delegate method fired when the selection of the <see cref="PreviewDataGridView"/> grid changes.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event arguments.</param>
    private void PreviewDataGridView_SelectionChanged(object sender, EventArgs e)
    {
      ImportButton.Enabled = PreviewDataGridView.SelectedColumns.Count > 0;
    }

    /// <summary>
    /// Recalculates maximum allowed value for the RowsToReturnNumericUpDown control based on several contidions.
    /// </summary>
    private void RecalculateRowsToMaximum()
    {
      var maxAllowedRow = _activeWorkbookMaxRowNumber;
      var maximum = Math.Min(
        Math.Min(FromRowNumericUpDown.Maximum, maxAllowedRow - _atRow),
        FromRowNumericUpDown.Maximum - FromRowNumericUpDown.Value + 1);

      if (_importIsCappedByRows && !IncludeHeadersCheckBox.Checked && !Settings.Default.ImportCreateExcelTable)
      {
        maximum++;
      }

      RowsToReturnNumericUpDown.Maximum = maximum;
    }

    /// <summary>
    /// Event delegate method fired when the <see cref="SelectAllToolStripMenuItem"/> context menu item is clicked.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event arguments.</param>
    private void SelectAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
      PreviewDataGridView.SelectAll();
    }

    /// <summary>
    /// Event delegate method fired when the <see cref="SelectNoneToolStripMenuItem"/> context menu item is clicked.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event arguments.</param>
    private void SelectNoneToolStripMenuItem_Click(object sender, EventArgs e)
    {
      PreviewDataGridView.ClearSelection();
    }

    /// <summary>
    /// Shows or hides the compatibility warning controls to let the users know if the Excel spreadsheet is running in Excel 2003 compatibility mode.
    /// </summary>
    /// <param name="show">Flag indicating if the compatibility warning controls should be shown.</param>
    private void SetCompatibilityWarningControlsVisibility(bool show)
    {
      OptionsWarningLabel.Visible = show;
      OptionsWarningPictureBox.Visible = show;
    }

    /// <summary>
    /// Sets the import parameter values into the database object.
    /// This is needed before getting any data from it.
    /// </summary>
    private void SetImportParameterValues(int rowsCount)
    {
      _dbTableOrView.ImportParameters.AddSummaryRow = AddSummaryFieldsCheckBox.Checked;
      _dbTableOrView.ImportParameters.ColumnsNamesList = _importColumns;
      _dbTableOrView.ImportParameters.CreatePivotTable = CreatePivotTableCheckBox.Checked;
      _dbTableOrView.ImportParameters.FirstRowIndex = FirstRowIndex;
      _dbTableOrView.ImportParameters.IncludeColumnNames = IncludeHeadersCheckBox.Checked;
      _dbTableOrView.ImportParameters.IntoNewWorksheet = false;
      _dbTableOrView.ImportParameters.RowsCount = rowsCount;
    }
  }
}