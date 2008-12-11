using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using MySql.Data.VisualStudio.Editors;
using System.Drawing;
using System.Collections;
using MySql.Data.VisualStudio.Properties;
using System.ComponentModel;
using Microsoft.VisualStudio.OLE.Interop;
using MySql.Data.VisualStudio.DbObjects;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;

namespace MySql.Data.VisualStudio
{
	class TableEditor : UserControl
    {
        private TableNode tableNode;
        private System.ComponentModel.IContainer components;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewComboBoxColumn dataGridViewComboBoxColumn1;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private VS2005PropertyGrid columnProperties;
        private Panel panel1;
        private DataGridView columnGrid;
        private TableEditorPane pane;
        private MySplitter splitter1;
        private DataGridViewTextBoxColumn NameColumn;
        private DataGridViewComboBoxColumn TypeColumn;
        private DataGridViewCheckBoxColumn AllowNullColumn;
        private BindingSource columnBindingSource;
        private string[] dataTypes;

		public TableEditor(TableEditorPane pane, TableNode node)
		{
            this.pane = pane;
			tableNode = node;
			InitializeComponent();

            dataTypes = Metadata.GetDataTypes(true);
            TypeColumn.Items.AddRange((object[])dataTypes);

            tableNode.DataLoaded += new EventHandler(OnDataLoaded);
            columnGrid.RowTemplate.HeaderCell = new MyDataGridViewRowHeaderCell();
            SetupCommands();
		}

#region Properties

        List<Index> Indexes 
        {
            get { return tableNode.Table.Indexes; }
        }

        List<Column> Columns 
        {
            get { return tableNode.Table.Columns; }
        }

#endregion 

		void OnDataLoaded(object sender, EventArgs e)
		{
            columnBindingSource.DataSource = tableNode.Table.Columns;
            columnBindingSource.AddingNew += new AddingNewEventHandler(columnBindingSource_AddingNew);
            pane.SelectObject(tableNode.Table);
		}

        void columnBindingSource_AddingNew(object sender, AddingNewEventArgs e)
        {
            e.NewObject = new ColumnWithTypeDescriptor();
        }

		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.columnProperties = new MySql.Data.VisualStudio.Editors.VS2005PropertyGrid();
            this.panel1 = new System.Windows.Forms.Panel();
            this.columnGrid = new System.Windows.Forms.DataGridView();
            this.NameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TypeColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.AllowNullColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.columnBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn1 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.splitter1 = new MySql.Data.VisualStudio.Editors.MySplitter();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.columnGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.columnBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(6, 10);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Padding = new System.Drawing.Point(6, 6);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(612, 308);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.columnProperties);
            this.tabPage1.Location = new System.Drawing.Point(4, 30);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(604, 274);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Column Properties";
            // 
            // columnProperties
            // 
            this.columnProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.columnProperties.Location = new System.Drawing.Point(3, 3);
            this.columnProperties.Name = "columnProperties";
            this.columnProperties.Size = new System.Drawing.Size(598, 268);
            this.columnProperties.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.tabControl1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 103);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(6, 10, 6, 6);
            this.panel1.Size = new System.Drawing.Size(624, 324);
            this.panel1.TabIndex = 9;
            // 
            // columnGrid
            // 
            this.columnGrid.AllowUserToResizeRows = false;
            this.columnGrid.AutoGenerateColumns = false;
            this.columnGrid.BackgroundColor = System.Drawing.SystemColors.Window;
            this.columnGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.columnGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.columnGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.columnGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.columnGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NameColumn,
            this.TypeColumn,
            this.AllowNullColumn});
            this.columnGrid.DataSource = this.columnBindingSource;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.columnGrid.DefaultCellStyle = dataGridViewCellStyle2;
            this.columnGrid.Dock = System.Windows.Forms.DockStyle.Top;
            this.columnGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.columnGrid.GridColor = System.Drawing.SystemColors.ControlLight;
            this.columnGrid.Location = new System.Drawing.Point(0, 0);
            this.columnGrid.Name = "columnGrid";
            this.columnGrid.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.columnGrid.RowHeadersWidth = 25;
            this.columnGrid.ShowCellErrors = false;
            this.columnGrid.ShowEditingIcon = false;
            this.columnGrid.ShowRowErrors = false;
            this.columnGrid.Size = new System.Drawing.Size(624, 103);
            this.columnGrid.TabIndex = 2;
            this.columnGrid.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.columnGrid_CellValidating);
            this.columnGrid.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.columnGrid_EditingControlShowing);
            // 
            // NameColumn
            // 
            this.NameColumn.DataPropertyName = "ColumnName";
            this.NameColumn.HeaderText = "Column Name";
            this.NameColumn.MinimumWidth = 200;
            this.NameColumn.Name = "NameColumn";
            this.NameColumn.Width = 200;
            // 
            // TypeColumn
            // 
            this.TypeColumn.DataPropertyName = "DataType";
            this.TypeColumn.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            this.TypeColumn.DisplayStyleForCurrentCellOnly = true;
            this.TypeColumn.HeaderText = "Data Type";
            this.TypeColumn.Name = "TypeColumn";
            // 
            // AllowNullColumn
            // 
            this.AllowNullColumn.DataPropertyName = "AllowNull";
            this.AllowNullColumn.HeaderText = "Allow Nulls";
            this.AllowNullColumn.Name = "AllowNullColumn";
            // 
            // columnBindingSource
            // 
            this.columnBindingSource.DataSource = typeof(Column);
            this.columnBindingSource.CurrentChanged += new System.EventHandler(this.columnBindingSource_CurrentChanged);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "ColumnName";
            this.dataGridViewTextBoxColumn1.HeaderText = "Column Name";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.Width = 150;
            // 
            // dataGridViewComboBoxColumn1
            // 
            this.dataGridViewComboBoxColumn1.DataPropertyName = "DataType";
            this.dataGridViewComboBoxColumn1.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            this.dataGridViewComboBoxColumn1.DisplayStyleForCurrentCellOnly = true;
            this.dataGridViewComboBoxColumn1.HeaderText = "Data Type";
            this.dataGridViewComboBoxColumn1.Name = "dataGridViewComboBoxColumn1";
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 103);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(624, 6);
            this.splitter1.TabIndex = 10;
            this.splitter1.TabStop = false;
            // 
            // TableEditor
            // 
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.columnGrid);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "TableEditor";
            this.Size = new System.Drawing.Size(624, 427);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.columnGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.columnBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #region Command Handling

        private void SetupCommands()
        {
            pane.AddCommand(GuidList.DavinciCommandSet, SharedCommands.cmdidIndexesAndKeys,
                new EventHandler(OnIndexesAndKeys), null);
            pane.AddCommand(GuidList.DavinciCommandSet, SharedCommands.cmdidForeignKeys,
                new EventHandler(OnForeignKeys), new EventHandler(OnQueryForeignKeys));
            pane.AddCommand(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.PrimaryKey,
                new EventHandler(OnPrimaryKey), new EventHandler(OnQueryPrimaryKey));
            pane.AddCommand(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.GenerateChangeScript,
                new EventHandler(OnGenerateChangeScript), null);
        }

        private void OnQueryPrimaryKey(object sender, EventArgs e)
        {
            bool allKeys = columnGrid.SelectedRows.Count == 0 ? false : true;

            foreach (DataGridViewRow row in columnGrid.SelectedRows)
                if (!tableNode.Table.Columns[row.Index].PrimaryKey)
                {
                    allKeys = false;
                    break;
                }

            OleMenuCommand primaryKey = sender as OleMenuCommand;
            primaryKey.Checked = allKeys;
        }

        private void OnPrimaryKey(object sender, EventArgs e)
        {
            OleMenuCommand primaryKey = sender as OleMenuCommand;

            foreach (Column c in Columns)
                c.PrimaryKey = false;
            tableNode.Table.DeleteKey(null);

            // if not checked then we are setting the key columns
            if (!primaryKey.Checked)
            {
                Index index = tableNode.Table.CreateIndexWithUniqueName(true);

                List<int> rows = new List<int>();

                foreach (DataGridViewRow row in columnGrid.SelectedRows)
                    rows.Add(row.Index);
                if (columnGrid.SelectedRows.Count == 0)
                    rows.Add(columnGrid.CurrentCell.RowIndex);
                foreach (int row in rows)
                {
                    Columns[row].PrimaryKey = true;
                    IndexColumn ic = new IndexColumn();
                    ic.OwningIndex = index;
                    ic.ColumnName = Columns[row].ColumnName;
                    ic.SortOrder = IndexSortOrder.Ascending;
                    index.Columns.Add(ic);
                }
                if (index.Columns.Count > 0)
                    Indexes.Add(index);
            }
            columnGrid.Refresh();
        }

        private void OnIndexesAndKeys(object sender, EventArgs e)
        {
            TableIndexDialog dlg = new TableIndexDialog(tableNode);
            dlg.ShowDialog();
        }

        private void OnQueryForeignKeys(object sender, EventArgs e)
        {
            OleMenuCommand foreignKey = sender as OleMenuCommand;
            foreignKey.Enabled = tableNode.Table.SupportsFK;
        }

        private void OnForeignKeys(object sender, EventArgs e)
        {
            ForeignKeyDialog dlg = new ForeignKeyDialog(tableNode);
            dlg.ShowDialog();
        }

        private void OnGenerateChangeScript(object sender, EventArgs e)
        {
            GenerateChangeScriptDialog dlg = new GenerateChangeScriptDialog(tableNode);
            dlg.ShowDialog();
        }

        #endregion

        private void columnGrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            Type t = e.Control.GetType();
            if (t == typeof(DataGridViewComboBoxEditingControl))
            {
                DataGridViewComboBoxEditingControl ec = e.Control as DataGridViewComboBoxEditingControl;
                ec.DropDownStyle = ComboBoxStyle.DropDown;
                columnGrid.NotifyCurrentCellDirty(true);
                ec.Items.Clear();
                foreach (string s in dataTypes)
                    ec.Items.Add(s);
                if (tableNode.Table.Columns.Count > columnGrid.CurrentRow.Index)
                {
                    Column c = tableNode.Table.Columns[columnGrid.CurrentRow.Index];
                    AdjustComboBox(ec, c.DataType);
                }
            }
            else if (t == typeof(DataGridViewTextBoxEditingControl))
            {
                DataGridViewTextBoxEditingControl tec = e.Control as DataGridViewTextBoxEditingControl;
                tec.Multiline = true;
                tec.Dock = DockStyle.Fill;
                tec.BorderStyle = BorderStyle.Fixed3D;
            }
        }

        private void columnBindingSource_CurrentChanged(object sender, EventArgs e)
        {
            Column currentObject = columnBindingSource.Current as Column;
            columnProperties.SelectedObject = currentObject;
        }


        private void AdjustComboBox(ComboBox cb, string type)
        {
            if (String.IsNullOrEmpty(type)) return;
            int index = type.IndexOf("(");
            if (index == -1)
                cb.Items.Add(type);
            else
            {
                string baseType = type.Substring(0, index);
                for (int i = 0; i < cb.Items.Count; i++)
                {
                    string item = cb.Items[i] as string;
                    if (item.StartsWith(baseType))
                    {
                        cb.Items[i] = type;
                        break;
                    }
                }
            }
        }

        private bool InsertType(string type)
        {
            int index = type.IndexOf("(");
            if (index == -1) return false;

            string baseType = type.Substring(0, index);
            for (int i = 0; i < TypeColumn.Items.Count; i++)
            {
                string s = TypeColumn.Items[i] as string;
                if (s.StartsWith(baseType))
                {
                    TypeColumn.Items[i] = type;
                    break;
                }
            }
            return true;
        }

        private void columnGrid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex != 1) return;
            string type = e.FormattedValue as string;
            if (String.IsNullOrEmpty(type)) return;
            if (TypeColumn.Items.Contains(type)) return;

            if (!InsertType(type))
                TypeColumn.Items.Add(type);
            columnGrid.CurrentCell.Value = e.FormattedValue;
        }
	}
}
