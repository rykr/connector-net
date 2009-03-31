using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Data;
using MySql.Data.VisualStudio.Editors;
using System.Data;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;

namespace MySql.Data.VisualStudio
{
    class ViewNode : DocumentNode, IVsTextBufferProvider
	{
        private TextBufferEditor editor = null;

        public ViewNode(DataViewHierarchyAccessor hierarchyAccessor, int id) : 
			base(hierarchyAccessor, id)
		{
            NodeId = "View";
            editor = new TextBufferEditor();
        }

        public static void CreateNew(DataViewHierarchyAccessor HierarchyAccessor)
        {
            ViewNode node = new ViewNode(HierarchyAccessor, 0);
            node.Edit();
        }

        #region Properties

        public override string SchemaCollection
        {
            get { return "views"; }
        }

        public override bool Dirty
        {
            get { return (editor as TextBufferEditor).Dirty; }
            protected set { (editor as TextBufferEditor).Dirty = value; }
        }

        #endregion

        public override object GetEditor()
        {
            return editor;
        }
        
        private string GetNewViewText()
        {
            StringBuilder sb = new StringBuilder("CREATE VIEW ");
            sb.AppendFormat("{0}\r\n", Name);
            sb.Append("/*\r\n(column1, column2)\r\n*/\r\n");
            sb.Append("AS /* select statement */\r\n");
            return sb.ToString();
        }

        protected override void Load()
        {
            if (IsNew)
                editor.Text = GetNewViewText();
            else
            {
                try
                {
                    string[] restrictions = new string[3];
                    restrictions[1] = Database;
                    restrictions[2] = Name;
                    DataTable views = this.GetSchema("Views", restrictions);
                    if (views.Rows.Count != 1)
                        throw new Exception(String.Format("There is no view with the name '{0}'", Name));
                    editor.Text = String.Format("ALTER VIEW `{0}` AS \r\n{1}",
                        Name, views.Rows[0]["VIEW_DEFINITION"].ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to load view with error: " + ex.Message);
                }
            }
        }

        public override string GetSaveSql()
        {
            return editor.Text;
        }

        #region IVsTextBufferProvider Members

        private IVsTextLines buffer;
        
        int IVsTextBufferProvider.GetTextBuffer(out IVsTextLines ppTextBuffer)
        {
            if (buffer == null)
            {
                Type bufferType = typeof(IVsTextLines);
                Guid riid = bufferType.GUID;
                Guid clsid = typeof(VsTextBufferClass).GUID;
                buffer = (IVsTextLines)MySqlDataProviderPackage.Instance.CreateInstance(
                                     ref clsid, ref riid, typeof(object));
            }
            ppTextBuffer = buffer;
            return VSConstants.S_OK;
        }

        int IVsTextBufferProvider.LockTextBuffer(int fLock)
        {
            return VSConstants.S_OK;
        }

        int IVsTextBufferProvider.SetTextBuffer(IVsTextLines pTextBuffer)
        {
            return VSConstants.S_OK;
        }

        #endregion
    }
}