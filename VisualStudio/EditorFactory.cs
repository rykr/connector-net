﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace MySql.Data.VisualStudio
{
    /// <summary>
    /// Factory for creating our editors
    /// </summary>
    [Guid("ef2efd06-12ed-4ef3-ab46-1abc6ab7cfc9")]
    public class EditorFactory : IVsEditorFactory
    {
        private MyPackage myPackage;
        private ServiceProvider vsServiceProvider;
        private Microsoft.VisualStudio.Data.DataConnection connection;

        public EditorFactory(MyPackage PackageEditor)
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering {0} constructor", this.ToString()));

            myPackage = PackageEditor;
        }
        
        internal Microsoft.VisualStudio.Data.DataConnection Connection 
        {
            set { connection = value; }
        }

        #region IVsEditorFactory Members

        public int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider psp)
        {
            vsServiceProvider = new ServiceProvider(psp);
            return VSConstants.S_OK;
        }

        public object GetService(Type serviceType)
        {
            return vsServiceProvider.GetService(serviceType);
        }

        // This method is called by the Environment (inside IVsUIShellOpenDocument::
        // OpenStandardEditor and OpenSpecificEditor) to map a LOGICAL view to a 
        // PHYSICAL view. A LOGICAL view identifies the purpose of the view that is
        // desired (e.g. a view appropriate for Debugging [LOGVIEWID_Debugging], or a 
        // view appropriate for text view manipulation as by navigating to a find
        // result [LOGVIEWID_TextView]). A PHYSICAL view identifies an actual type 
        // of view implementation that an IVsEditorFactory can create. 
        //
        // NOTE: Physical views are identified by a string of your choice with the 
        // one constraint that the default/primary physical view for an editor  
        // *MUST* use a NULL string as its physical view name (*pbstrPhysicalView = NULL).
        //
        // NOTE: It is essential that the implementation of MapLogicalView properly
        // validates that the LogicalView desired is actually supported by the editor.
        // If an unsupported LogicalView is requested then E_NOTIMPL must be returned.
        //
        // NOTE: The special Logical Views supported by an Editor Factory must also 
        // be registered in the local registry hive. LOGVIEWID_Primary is implicitly 
        // supported by all editor types and does not need to be registered.
        // For example, an editor that supports a ViewCode/ViewDesigner scenario
        // might register something like the following:
        //      HKLM\Software\Microsoft\VisualStudio\8.0\Editors\
        //          {...guidEditor...}\
        //              LogicalViews\
        //                  {...LOGVIEWID_TextView...} = s ''
        //                  {...LOGVIEWID_Code...} = s ''
        //                  {...LOGVIEWID_Debugging...} = s ''
        //                  {...LOGVIEWID_Designer...} = s 'Form'
        //
        public int MapLogicalView(ref Guid rguidLogicalView, out string pbstrPhysicalView)
        {
            pbstrPhysicalView = null;   // initialize out parameter

            // we support only a single physical view
            if (VSConstants.LOGVIEWID_Primary == rguidLogicalView)
                return VSConstants.S_OK;        // primary view uses NULL as pbstrPhysicalView
            else
                return VSConstants.E_NOTIMPL;   // you must return E_NOTIMPL for any unrecognized rguidLogicalView values
        }

        public int Close()
        {
            return VSConstants.S_OK;
        }

        public int CreateEditorInstance(
                        uint grfCreateDoc,
                        string pszMkDocument,
                        string pszPhysicalView,
                        IVsHierarchy pvHier,
                        uint itemid,
                        System.IntPtr punkDocDataExisting,
                        out System.IntPtr ppunkDocView,
                        out System.IntPtr ppunkDocData,
                        out string pbstrEditorCaption,
                        out Guid pguidCmdUI,
                        out int pgrfCDW)
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering {0} CreateEditorInstance()", this.ToString()));

            // Initialize to null
            ppunkDocView = IntPtr.Zero;
            ppunkDocData = IntPtr.Zero;
            pguidCmdUI = GuidList.guidEditorFactory;
            pgrfCDW = 0;
            pbstrEditorCaption = null;

            // Validate inputs
            if ((grfCreateDoc & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0)
            {
                Debug.Assert(false, "Only Open or Silent is valid");
                return VSConstants.E_INVALIDARG;
            }
            if (punkDocDataExisting != IntPtr.Zero)
            {
                return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
            }

            // Create the Document (editor)
            BaseEditor be = GetEditor(ref pszMkDocument, myPackage);
            be.Connection = connection;
            ppunkDocView = Marshal.GetIUnknownForObject(be);
            ppunkDocData = Marshal.GetIUnknownForObject(be);
            pbstrEditorCaption = String.Format("{0} [{1}]",
                pszMkDocument, be.EditorType);

            return VSConstants.S_OK;
        }

        private BaseEditor GetEditor(ref string name, MyPackage myPackage)
        {
            int index = name.IndexOf(':');
            string type = name.Substring(0, index);
            name = name.Substring(index + 1);
            switch (type)
            {
                case "TABLE": return new TableEditor(myPackage);
                case "PROCEDURE": return new StoredRoutineEditor(myPackage);
                default:
                    return null;
            }
        }

        #endregion
    }
}
