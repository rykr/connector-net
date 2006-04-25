﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;
using Microsoft.VisualStudio;

namespace MySql.VSTools
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsWindowPane interface.
    /// </summary>
    [Guid("ae5d17fe-e250-4f36-b38b-533686e0e0ca")]
    public class MyToolWindow : ToolWindowPane
    {
        // This is the user control hosted by the tool window; it is exposed to the base class 
        // using the Window property. Note that, even if this class implements IDispose, we are
        // not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
        // the object returned by the Window property.
//        private IVsTrackSelectionEx trackSel;
        private ITrackSelection trackSel;
        private SelectionContainer selectContainer;
        private List<ServerNode> serverList;
        private IVsUIHierarchyWindow hierarchyWindow;

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public MyToolWindow() :
            base(null)
        {
            this.ToolClsid = VSConstants.CLSID_VsUIHierarchyWindow;
            this.ToolBar = new CommandID(GuidList.guidMyVSToolsCmdSet, PkgCmdIDList.cmdidMyExplorerToolbar);

            // Set the window title reading it from the resources.
            this.Caption = MyVSTools.GetResourceString("ToolWindowTitle");
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            this.BitmapResourceID = 300;
            this.BitmapIndex = 1;

            serverList = new List<ServerNode>();
        }

        /// <summary>
        /// This property returns the handle to the user control that should
        /// be hosted in the Tool Window.
        /// </summary>
        override public IWin32Window Window
        {
            get
            {
                // we don't need to return anything since we are reusing the
                // internal tree implementation
                return null; 
            }
        }

        internal void RefreshServerList()
        {

        }

        internal void AddServer(string name, string connectString)
        {
            ServerNode node = new ServerNode(name, connectString);
            node.ChildNodeSelected += 
                new ChildNodeSelectedEventHandler(node_ChildNodeSelected);
            node.Populate();

            hierarchyWindow.AddUIHierarchy(node, 
                (int)__VSADDHIEROPTIONS.ADDHIEROPT_DontSelectNewHierarchy);
            serverList.Add(node);
        }

        void node_ChildNodeSelected(object sender, ExplorerNode node)
        {
            UpdateSelection(node);
        }

        private void LoadServers()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                System.IO.Path.DirectorySeparatorChar +
                "MySQL" + System.IO.Path.DirectorySeparatorChar + 
                "MyVSTools.conf";
            if (System.IO.File.Exists(path))
            {
                using (System.IO.StreamReader reader =
                    new System.IO.StreamReader(path))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split(new char[] { ':' }, 2);
                        AddServer(parts[0], parts[1]);
                    }
                }
            }
        }

        private ITrackSelection TrackSelection
        {
            get
            {
                if (trackSel != null)
                    return trackSel;
                object spObj;
                IVsWindowFrame wf = (this.Frame as IVsWindowFrame);
                wf.GetProperty((int)__VSFPROPID.VSFPROPID_SPFrame, out spObj);
                Microsoft.VisualStudio.OLE.Interop.IServiceProvider sp =
                    (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)spObj;
                trackSel = (ITrackSelection)GetMyService(
                    sp, typeof(STrackSelection), typeof(ITrackSelection));
                return trackSel;
            }
        }

/*        private IVsTrackSelectionEx TrackSelection
        {
            get
            {
                if (trackSel != null)
                    return trackSel;
                object frameServiceProvider;
                (this.Frame as IVsWindowFrame).GetProperty(
                    (int)__VSFPROPID.VSFPROPID_SPFrame,
                    out frameServiceProvider);
                IServiceProvider frameSp = (frameServiceProvider as IServiceProvider);
                trackSel = frameSp.GetService(typeof(SVsTrackSelectionEx))
                    as IVsTrackSelectionEx;
                return trackSel;
            }
        }
        */
        public void UpdateSelection(Object o)
        {
            if (selectContainer == null)
                selectContainer = new SelectionContainer();
            ArrayList selObjects = new ArrayList();
            selObjects.Add(o);
            selectContainer.SelectedObjects = selObjects;
            ITrackSelection track = TrackSelection;
            if (track != null)
                track.OnSelectChange((ISelectionContainer)selectContainer);
        }

        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();

            // retrieve the object that fills the client area
            object docView;
            (this.Frame as IVsWindowFrame).GetProperty((int)__VSFPROPID.VSFPROPID_DocView, 
                out docView);

            object tmpClientFrame;
            hierarchyWindow = (docView as IVsUIHierarchyWindow);
            int result = hierarchyWindow.Init(null,
                (int)(__UIHWINFLAGS.UIHWF_ForceSingleSelect |
                __UIHWINFLAGS.UIHWF_DoNotSortRootNodes |
                __UIHWINFLAGS.UIHWF_SupportToolWindowToolbars |
                __UIHWINFLAGS.UIHWF_LinesAtRoot |
                __UIHWINFLAGS.UIHWF_RouteCmdidDelete),
                out tmpClientFrame);
            (this.Frame as IVsWindowFrame).Show();

            // make sure our config path is created.
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                System.IO.Path.DirectorySeparatorChar +
                "MySQL";
            System.IO.Directory.CreateDirectory(path);

            LoadServers();
            SetupCommandHandlers();
            selectContainer = new SelectionContainer(true, true);
        }

        private object GetMyService(Microsoft.VisualStudio.OLE.Interop.IServiceProvider sp,
            Type service, Type iface)
        {
            IntPtr pUnk = IntPtr.Zero;
            Guid serviceGuid = service.GUID;
            Guid ifaceGuid = iface.GUID;
            int hr = sp.QueryService
                (ref serviceGuid, ref ifaceGuid, out pUnk);
            if (hr >= 0 && IntPtr.Zero != pUnk) 
            {
                try 
                {
                    return Marshal.GetObjectForIUnknown(pUnk);
                }
                finally 
                {
                    Marshal.Release(pUnk);
                }
            }
            return null;
        }

        private void AddCommand(OleMenuCommandService mcs, int cmd)
        {
            // Create the command for the menu item.
            CommandID commandId = new CommandID(GuidList.guidMyVSToolsCmdSet, cmd);
            MenuCommand menuCommand = new MenuCommand( new EventHandler(CommandCallback),
                commandId);
            mcs.AddCommand(menuCommand);
        }

        private void SetupCommandHandlers()
        {
            OleMenuCommandService mcs = (Package as MyVSTools).GetMyService(
                typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                AddCommand(mcs, PkgCmdIDList.cmdidDelete);
                AddCommand(mcs, PkgCmdIDList.cmdidModifyConnection);
                AddCommand(mcs, PkgCmdIDList.cmdidShowTableData);
                AddCommand(mcs, PkgCmdIDList.cmdidOpen);
                AddCommand(mcs, PkgCmdIDList.cmdidAddNewTable);
                AddCommand(mcs, PkgCmdIDList.cmdidAddNewProcedure);
                AddCommand(mcs, PkgCmdIDList.cmdidAddNewFunction);
                AddCommand(mcs, PkgCmdIDList.cmdidAddNewView);
                AddCommand(mcs, PkgCmdIDList.cmdidOpenTableDef);
                AddCommand(mcs, PkgCmdIDList.cmdidNewQuery);
                AddCommand(mcs, PkgCmdIDList.cmdidAddNewTrigger);
            }
        }

        private void CommandCallback(object sender, EventArgs e)
        {
            IVsUIHierarchy selectedHier;

            MenuCommand mc = (sender as MenuCommand);
            hierarchyWindow.FindCommonSelectedHierarchy(
                (uint)__VSCOMHIEROPTIONS.COMHIEROPT_RootHierarchyOnly,
                out selectedHier);

            ServerNode selectedServer = (selectedHier as ServerNode);
            if (selectedServer != null && selectedServer.ActiveNode != null)
            {
                selectedServer.ActiveNode.DoCommand(mc.CommandID.ID);
            }
        }
    }

}
