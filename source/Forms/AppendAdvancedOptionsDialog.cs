﻿// Copyright (c) 2012-2013, Oracle and/or its affiliates. All rights reserved.
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
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using MySQL.ForExcel.Classes;
using MySQL.ForExcel.Properties;
using MySQL.Utility.Forms;

namespace MySQL.ForExcel.Forms
{
  /// <summary>
  /// Advanced options dialog for the operations performed by the <see cref="AppendDataForm"/>.
  /// </summary>
  public partial class AppendAdvancedOptionsDialog : AutoStyleableBaseDialog
  {
    /// <summary>
    /// List of column mappings for the current user.
    /// </summary>
    private readonly MySqlColumnMappingList _mappings;

    /// <summary>
    /// Specific column mapping currently selected by the user.
    /// </summary>
    private MySqlColumnMapping _selectedMapping;

    /// <summary>
    /// Gets or sets a value indicating whether the data in the parent form needs to be reloaded on the grids.
    /// </summary>
    /// <value>
    ///   <c>true</c> if requires refreshing; otherwise, <c>false</c>.
    /// </value>
    public bool ParentFormRequiresRefresh { get; private set; }

    /// <summary>
    /// Creates a new instance of the <see cref="AppendAdvancedOptionsDialog"/> class.
    /// </summary>
    public AppendAdvancedOptionsDialog()
    {
      ParentFormRequiresRefresh = false;
      InitializeComponent();

      DoNotPerformAutoMapCheckBox.Checked = Settings.Default.AppendPerformAutoMap;
      AutoStoreColumnMappingCheckBox.Checked = Settings.Default.AppendAutoStoreColumnMapping;
      ReloadColumnMappingCheckBox.Checked = Settings.Default.AppendReloadColumnMapping;
      UseFormattedValuesCheckBox.Checked = Settings.Default.AppendUseFormattedValues;
      PreviewRowsQuantityNumericUpDown.Value = Math.Min(PreviewRowsQuantityNumericUpDown.Maximum, Settings.Default.AppendLimitPreviewRowsQuantity);
      _mappings = new MySqlColumnMappingList();
      RefreshMappingList();
    }

    /// <summary>
    /// Event delegate method fired when the <see cref="ImportAdvancedOptionsDialog"/> is being closed.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event arguments.</param>
    private void AppendAdvancedOptionsDialog_FormClosing(object sender, FormClosingEventArgs e)
    {
      if (DialogResult == DialogResult.Cancel)
      {
        return;
      }

      var previewRowsQuantity = (int)PreviewRowsQuantityNumericUpDown.Value;
      ParentFormRequiresRefresh = Settings.Default.AppendUseFormattedValues != UseFormattedValuesCheckBox.Checked ||
                                  Settings.Default.AppendLimitPreviewRowsQuantity != previewRowsQuantity;

      Settings.Default.AppendPerformAutoMap = DoNotPerformAutoMapCheckBox.Checked;
      Settings.Default.AppendAutoStoreColumnMapping = AutoStoreColumnMappingCheckBox.Checked;
      Settings.Default.AppendReloadColumnMapping = ReloadColumnMappingCheckBox.Checked;
      Settings.Default.AppendUseFormattedValues = UseFormattedValuesCheckBox.Checked;
      Settings.Default.AppendLimitPreviewRowsQuantity = previewRowsQuantity;
      MiscUtilities.SaveSettings();
    }

    /// <summary>
    /// Event delegate method fired when the <see cref="DeleteMappingButton"/> button is clicked.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event arguments.</param>
    private void DeleteMappingButton_Click(object sender, EventArgs e)
    {
      if (_selectedMapping == null)
      {
        return;
      }

      _mappings.Remove(_selectedMapping);
      RefreshMappingList();
    }

    /// <summary>
    /// Event delegate method fired when an item within the <see cref="MappingsListView"/> list view is selected.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event arguments.</param>
    private void MappingsListView_SelectedIndexChanged(object sender, EventArgs e)
    {
      DeleteMappingButton.Enabled = MappingsListView.SelectedItems.Count > 0;
      _selectedMapping = MappingsListView.SelectedItems.Count > 0 ? MappingsListView.SelectedItems[0].Tag as MySqlColumnMapping : null;
      RenameMappingButton.Enabled = _selectedMapping != null;
    }

    /// <summary>
    /// Refreshes the list of column mappings shown in the mappings list view.
    /// </summary>
    private void RefreshMappingList()
    {
      MappingsListView.Items.Clear();

      foreach (var item in _mappings.UserColumnMappingsList)
      {
        ListViewItem itemList = new ListViewItem
        {
          Text = string.Format("{0} ({1}.{2})", item.Name, item.SchemaName, item.TableName)
        };
        itemList.SubItems.Add(string.Empty);
        itemList.Tag = item;
        MappingsListView.Items.Add(itemList);
      }

      if (MappingsListView.Items.Count > 0)
      {
        MappingsListView.Items[0].Selected = true;
      }
      else
      {
        DeleteMappingButton.Enabled = false;
        RenameMappingButton.Enabled = false;
      }
    }

    /// <summary>
    /// Event delegate method fired when the <see cref="RenameMappingButton"/> button is clicked.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event arguments.</param>
    private void RenameMappingButton_Click(object sender, EventArgs e)
    {
      if (_selectedMapping == null)
      {
        return;
      }

      int indexForName = 1;
      string proposedMappingName;
      do
      {
        proposedMappingName = _selectedMapping.TableName + "Mapping" + (indexForName > 1 ? indexForName.ToString(CultureInfo.InvariantCulture) : string.Empty);
        indexForName++;
      }
      while (_mappings.UserColumnMappingsList.Any(mapping => mapping.Name == proposedMappingName));

      string newName;
      using (var newColumnMappingDialog = new AppendNewColumnMappingDialog(proposedMappingName))
      {
        DialogResult dr = newColumnMappingDialog.ShowDialog();
        if (dr == DialogResult.Cancel)
        {
          return;
        }

        newName = newColumnMappingDialog.ColumnMappingName;
      }

      // Show error if name already exists
      if (_mappings.UserColumnMappingsList.Count(t => t.Name.Equals(newName)) > 0)
      {
        MiscUtilities.ShowCustomizedErrorDialog(Resources.MappingNameAlreadyExistsTitle, Resources.MappingNameAlreadyExistsDetail);
        return;
      }

      _mappings.Rename(_selectedMapping, newName);
      RefreshMappingList();
      ListViewItem item = MappingsListView.FindItemWithText(string.Format("{0} ({1}.{2})", newName, _selectedMapping.SchemaName, _selectedMapping.TableName));
      if (item != null)
      {
        MappingsListView.Items[item.Index].Selected = true;
      }

      MappingsListView.Focus();
    }
  }
}