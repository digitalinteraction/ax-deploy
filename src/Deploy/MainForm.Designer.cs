namespace Deploy
{
    partial class MainForm
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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Outbox", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Error", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Unexpected", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup("Charged", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup5 = new System.Windows.Forms.ListViewGroup("Recharging", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup6 = new System.Windows.Forms.ListViewGroup("Clearing", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup7 = new System.Windows.Forms.ListViewGroup("Downloading", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup8 = new System.Windows.Forms.ListViewGroup("Inbox", System.Windows.Forms.HorizontalAlignment.Left);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.toolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelAvailable = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainerLog = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.listViewDevices = new System.Windows.Forms.ListView();
            this.columnHeaderLed = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBattery = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copySelectedEntriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deviceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedResetDeviceWithErrorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedSetFullChargeLevelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedDownloadAndClearNonCompletedDevicesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedWipeUnsavedRecordingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedCancelDownloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetDevicesWithCommsErrorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedIgnoreConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.installAX3DriverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonTest = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.timerUpdate = new System.Windows.Forms.Timer(this.components);
            this.configureControl = new Deploy.ConfigureControl();
            this.toolStripContainer.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer.ContentPanel.SuspendLayout();
            this.toolStripContainer.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer.SuspendLayout();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLog)).BeginInit();
            this.splitContainerLog.Panel1.SuspendLayout();
            this.splitContainerLog.Panel2.SuspendLayout();
            this.splitContainerLog.SuspendLayout();
            this.panel1.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer
            // 
            // 
            // toolStripContainer.BottomToolStripPanel
            // 
            this.toolStripContainer.BottomToolStripPanel.Controls.Add(this.statusStrip);
            // 
            // toolStripContainer.ContentPanel
            // 
            this.toolStripContainer.ContentPanel.Controls.Add(this.splitContainerLog);
            this.toolStripContainer.ContentPanel.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.toolStripContainer.ContentPanel.Size = new System.Drawing.Size(2053, 603);
            this.toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.toolStripContainer.Name = "toolStripContainer";
            this.toolStripContainer.Size = new System.Drawing.Size(2053, 701);
            this.toolStripContainer.TabIndex = 0;
            this.toolStripContainer.Text = "toolStripContainer";
            // 
            // toolStripContainer.TopToolStripPanel
            // 
            this.toolStripContainer.TopToolStripPanel.Controls.Add(this.menuStrip);
            this.toolStripContainer.TopToolStripPanel.Controls.Add(this.toolStrip);
            this.toolStripContainer.TopToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            // 
            // statusStrip
            // 
            this.statusStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(40, 40);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel,
            this.toolStripStatusLabelAvailable});
            this.statusStrip.Location = new System.Drawing.Point(0, 0);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(2053, 46);
            this.statusStrip.TabIndex = 0;
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(2008, 41);
            this.toolStripStatusLabel.Spring = true;
            this.toolStripStatusLabel.Text = "-";
            this.toolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolStripStatusLabel.Click += new System.EventHandler(this.toolStripStatusLabel_Click);
            // 
            // toolStripStatusLabelAvailable
            // 
            this.toolStripStatusLabelAvailable.Name = "toolStripStatusLabelAvailable";
            this.toolStripStatusLabelAvailable.Size = new System.Drawing.Size(30, 41);
            this.toolStripStatusLabelAvailable.Text = "-";
            this.toolStripStatusLabelAvailable.Click += new System.EventHandler(this.toolStripStatusLabelAvailable_Click);
            // 
            // splitContainerLog
            // 
            this.splitContainerLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerLog.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainerLog.Location = new System.Drawing.Point(0, 0);
            this.splitContainerLog.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.splitContainerLog.Name = "splitContainerLog";
            this.splitContainerLog.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerLog.Panel1
            // 
            this.splitContainerLog.Panel1.Controls.Add(this.panel1);
            // 
            // splitContainerLog.Panel2
            // 
            this.splitContainerLog.Panel2.Controls.Add(this.textBoxLog);
            this.splitContainerLog.Panel2Collapsed = true;
            this.splitContainerLog.Size = new System.Drawing.Size(2053, 603);
            this.splitContainerLog.SplitterDistance = 223;
            this.splitContainerLog.SplitterWidth = 10;
            this.splitContainerLog.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.listViewDevices);
            this.panel1.Controls.Add(this.configureControl);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(2053, 603);
            this.panel1.TabIndex = 0;
            // 
            // listViewDevices
            // 
            this.listViewDevices.AllowColumnReorder = true;
            this.listViewDevices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderLed,
            this.columnHeaderId,
            this.columnHeaderBattery,
            this.columnHeaderStatus});
            this.listViewDevices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewDevices.FullRowSelect = true;
            listViewGroup1.Header = "Outbox";
            listViewGroup1.Name = "listViewGroupOutbox";
            listViewGroup2.Header = "Error";
            listViewGroup2.Name = "listViewGroupError";
            listViewGroup3.Header = "Unexpected";
            listViewGroup3.Name = "listViewGroupUnexpected";
            listViewGroup4.Header = "Charged";
            listViewGroup4.Name = "listViewGroupCharged";
            listViewGroup5.Header = "Recharging";
            listViewGroup5.Name = "listViewGroupRecharging";
            listViewGroup6.Header = "Clearing";
            listViewGroup6.Name = "listViewGroupClearing";
            listViewGroup7.Header = "Downloading";
            listViewGroup7.Name = "listViewGroupDownloading";
            listViewGroup8.Header = "Inbox";
            listViewGroup8.Name = "listViewGroupInbox";
            this.listViewDevices.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2,
            listViewGroup3,
            listViewGroup4,
            listViewGroup5,
            listViewGroup6,
            listViewGroup7,
            listViewGroup8});
            this.listViewDevices.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewDevices.LabelWrap = false;
            this.listViewDevices.Location = new System.Drawing.Point(0, 339);
            this.listViewDevices.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.listViewDevices.MinimumSize = new System.Drawing.Size(4, 333);
            this.listViewDevices.Name = "listViewDevices";
            this.listViewDevices.Size = new System.Drawing.Size(2053, 333);
            this.listViewDevices.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewDevices.TabIndex = 2;
            this.listViewDevices.UseCompatibleStateImageBehavior = false;
            this.listViewDevices.View = System.Windows.Forms.View.Details;
            this.listViewDevices.SelectedIndexChanged += new System.EventHandler(this.listViewDevices_SelectedIndexChanged);
            // 
            // columnHeaderLed
            // 
            this.columnHeaderLed.Text = "";
            this.columnHeaderLed.Width = 35;
            // 
            // columnHeaderId
            // 
            this.columnHeaderId.Text = "ID";
            this.columnHeaderId.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderId.Width = 97;
            // 
            // columnHeaderBattery
            // 
            this.columnHeaderBattery.Text = "Battery";
            this.columnHeaderBattery.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderBattery.Width = 97;
            // 
            // columnHeaderStatus
            // 
            this.columnHeaderStatus.Text = "Status";
            this.columnHeaderStatus.Width = 1490;
            // 
            // textBoxLog
            // 
            this.textBoxLog.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxLog.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxLog.HideSelection = false;
            this.textBoxLog.Location = new System.Drawing.Point(0, 0);
            this.textBoxLog.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLog.Size = new System.Drawing.Size(400, 110);
            this.textBoxLog.TabIndex = 0;
            // 
            // menuStrip
            // 
            this.menuStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(40, 40);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.deviceToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.menuStrip.Size = new System.Drawing.Size(2053, 52);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            this.menuStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip_ItemClicked);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(75, 48);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(550, 46);
            this.openToolStripMenuItem.Text = "Explore W&orking Folder";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(547, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(550, 46);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copySelectedEntriesToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.toolStripSeparator4,
            this.selectAllToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(80, 48);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // copySelectedEntriesToolStripMenuItem
            // 
            this.copySelectedEntriesToolStripMenuItem.Name = "copySelectedEntriesToolStripMenuItem";
            this.copySelectedEntriesToolStripMenuItem.Size = new System.Drawing.Size(441, 46);
            this.copySelectedEntriesToolStripMenuItem.Text = "&Copy Selected Entries";
            this.copySelectedEntriesToolStripMenuItem.Click += new System.EventHandler(this.copySelectedEntriesToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(441, 46);
            this.copyToolStripMenuItem.Text = "Copy &Screenshot";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(441, 46);
            this.pasteToolStripMenuItem.Text = "&Paste Configuration";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(438, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(441, 46);
            this.selectAllToolStripMenuItem.Text = "Select &All";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.logToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(94, 48);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // logToolStripMenuItem
            // 
            this.logToolStripMenuItem.CheckOnClick = true;
            this.logToolStripMenuItem.Name = "logToolStripMenuItem";
            this.logToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F12;
            this.logToolStripMenuItem.Size = new System.Drawing.Size(247, 46);
            this.logToolStripMenuItem.Text = "&Log";
            this.logToolStripMenuItem.Click += new System.EventHandler(this.logToolStripMenuItem_Click);
            // 
            // deviceToolStripMenuItem
            // 
            this.deviceToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.advancedResetDeviceWithErrorToolStripMenuItem,
            this.advancedSetFullChargeLevelToolStripMenuItem,
            this.advancedDownloadAndClearNonCompletedDevicesToolStripMenuItem,
            this.advancedWipeUnsavedRecordingToolStripMenuItem,
            this.advancedCancelDownloadToolStripMenuItem});
            this.deviceToolStripMenuItem.Name = "deviceToolStripMenuItem";
            this.deviceToolStripMenuItem.Size = new System.Drawing.Size(118, 48);
            this.deviceToolStripMenuItem.Text = "&Device";
            // 
            // advancedResetDeviceWithErrorToolStripMenuItem
            // 
            this.advancedResetDeviceWithErrorToolStripMenuItem.Name = "advancedResetDeviceWithErrorToolStripMenuItem";
            this.advancedResetDeviceWithErrorToolStripMenuItem.Size = new System.Drawing.Size(678, 46);
            this.advancedResetDeviceWithErrorToolStripMenuItem.Text = "(Advanced) Reset Device...";
            this.advancedResetDeviceWithErrorToolStripMenuItem.Click += new System.EventHandler(this.advancedResetDeviceWithErrorToolStripMenuItem_Click);
            // 
            // advancedSetFullChargeLevelToolStripMenuItem
            // 
            this.advancedSetFullChargeLevelToolStripMenuItem.Name = "advancedSetFullChargeLevelToolStripMenuItem";
            this.advancedSetFullChargeLevelToolStripMenuItem.Size = new System.Drawing.Size(678, 46);
            this.advancedSetFullChargeLevelToolStripMenuItem.Text = "(Advanced) Treat as Fully &Charged...";
            this.advancedSetFullChargeLevelToolStripMenuItem.Click += new System.EventHandler(this.advancedSetFullChargeLevelToolStripMenuItem_Click);
            // 
            // advancedDownloadAndClearNonCompletedDevicesToolStripMenuItem
            // 
            this.advancedDownloadAndClearNonCompletedDevicesToolStripMenuItem.Name = "advancedDownloadAndClearNonCompletedDevicesToolStripMenuItem";
            this.advancedDownloadAndClearNonCompletedDevicesToolStripMenuItem.Size = new System.Drawing.Size(678, 46);
            this.advancedDownloadAndClearNonCompletedDevicesToolStripMenuItem.Text = "(Advanced) Forced Download and Clear...";
            this.advancedDownloadAndClearNonCompletedDevicesToolStripMenuItem.Click += new System.EventHandler(this.advancedDownloadAndClearNonCompletedDevicesToolStripMenuItem_Click);
            // 
            // advancedWipeUnsavedRecordingToolStripMenuItem
            // 
            this.advancedWipeUnsavedRecordingToolStripMenuItem.Name = "advancedWipeUnsavedRecordingToolStripMenuItem";
            this.advancedWipeUnsavedRecordingToolStripMenuItem.Size = new System.Drawing.Size(678, 46);
            this.advancedWipeUnsavedRecordingToolStripMenuItem.Text = "(Advanced) Destroy Recording...";
            this.advancedWipeUnsavedRecordingToolStripMenuItem.Click += new System.EventHandler(this.advancedWipeUnsavedRecordingToolStripMenuItem_Click);
            // 
            // advancedCancelDownloadToolStripMenuItem
            // 
            this.advancedCancelDownloadToolStripMenuItem.Name = "advancedCancelDownloadToolStripMenuItem";
            this.advancedCancelDownloadToolStripMenuItem.Size = new System.Drawing.Size(678, 46);
            this.advancedCancelDownloadToolStripMenuItem.Text = "(Advanced) Cancel Download...";
            this.advancedCancelDownloadToolStripMenuItem.Visible = false;
            this.advancedCancelDownloadToolStripMenuItem.Click += new System.EventHandler(this.advancedCancelDownloadToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetDevicesWithCommsErrorsToolStripMenuItem,
            this.advancedIgnoreConfigurationToolStripMenuItem,
            this.toolStripSeparator2,
            this.optionsToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(99, 48);
            this.toolsToolStripMenuItem.Text = "&Tools";
            // 
            // resetDevicesWithCommsErrorsToolStripMenuItem
            // 
            this.resetDevicesWithCommsErrorsToolStripMenuItem.Name = "resetDevicesWithCommsErrorsToolStripMenuItem";
            this.resetDevicesWithCommsErrorsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F9;
            this.resetDevicesWithCommsErrorsToolStripMenuItem.Size = new System.Drawing.Size(626, 46);
            this.resetDevicesWithCommsErrorsToolStripMenuItem.Text = "&Reset Devices With Comms Errors";
            this.resetDevicesWithCommsErrorsToolStripMenuItem.Click += new System.EventHandler(this.resetDevicesWithCommsErrorsToolStripMenuItem_Click);
            // 
            // advancedIgnoreConfigurationToolStripMenuItem
            // 
            this.advancedIgnoreConfigurationToolStripMenuItem.Name = "advancedIgnoreConfigurationToolStripMenuItem";
            this.advancedIgnoreConfigurationToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F8;
            this.advancedIgnoreConfigurationToolStripMenuItem.Size = new System.Drawing.Size(626, 46);
            this.advancedIgnoreConfigurationToolStripMenuItem.Text = "Clear Pending Configuration...";
            this.advancedIgnoreConfigurationToolStripMenuItem.Click += new System.EventHandler(this.advancedIgnoreConfigurationToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(623, 6);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(626, 46);
            this.optionsToolStripMenuItem.Text = "&Options...";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.installAX3DriverToolStripMenuItem,
            this.toolStripSeparator8,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(92, 48);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // installAX3DriverToolStripMenuItem
            // 
            this.installAX3DriverToolStripMenuItem.Name = "installAX3DriverToolStripMenuItem";
            this.installAX3DriverToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.installAX3DriverToolStripMenuItem.Size = new System.Drawing.Size(457, 46);
            this.installAX3DriverToolStripMenuItem.Text = "&Help Documentation";
            this.installAX3DriverToolStripMenuItem.Click += new System.EventHandler(this.documentationToolStripMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(454, 6);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(457, 46);
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // toolStrip
            // 
            this.toolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonTest,
            this.toolStripSeparator7});
            this.toolStrip.Location = new System.Drawing.Point(0, 49);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip.Size = new System.Drawing.Size(99, 48);
            this.toolStrip.Stretch = true;
            this.toolStrip.TabIndex = 1;
            this.toolStrip.Visible = false;
            // 
            // toolStripButtonTest
            // 
            this.toolStripButtonTest.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonTest.Image")));
            this.toolStripButtonTest.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonTest.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonTest.Name = "toolStripButtonTest";
            this.toolStripButtonTest.Size = new System.Drawing.Size(90, 45);
            this.toolStripButtonTest.Text = "Test";
            this.toolStripButtonTest.ToolTipText = "Test";
            this.toolStripButtonTest.Click += new System.EventHandler(this.toolStripButtonTest_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 48);
            // 
            // timerUpdate
            // 
            this.timerUpdate.Enabled = true;
            this.timerUpdate.Interval = 250;
            this.timerUpdate.Tick += new System.EventHandler(this.timerUpdate_Tick);
            // 
            // configureControl
            // 
            this.configureControl.BackColor = System.Drawing.SystemColors.Control;
            this.configureControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.configureControl.ForeColor = System.Drawing.SystemColors.ControlText;
            this.configureControl.Location = new System.Drawing.Point(0, 0);
            this.configureControl.Margin = new System.Windows.Forms.Padding(21, 17, 21, 17);
            this.configureControl.MinimumSize = new System.Drawing.Size(3, 339);
            this.configureControl.Name = "configureControl";
            this.configureControl.Size = new System.Drawing.Size(2053, 339);
            this.configureControl.TabIndex = 3;
            this.configureControl.ConfigurationScanned += new Deploy.ConfigureControl.ScanEventHandler(this.configureControl_ConfigurationScanned);
            this.configureControl.DeviceScanned += new Deploy.ConfigureControl.ScanEventHandler(this.configureControl_DeviceScanned);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2053, 701);
            this.Controls.Add(this.toolStripContainer);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.Name = "MainForm";
            this.Text = "Deploy";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MainForm_KeyPress);
            this.toolStripContainer.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer.ContentPanel.ResumeLayout(false);
            this.toolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer.TopToolStripPanel.PerformLayout();
            this.toolStripContainer.ResumeLayout(false);
            this.toolStripContainer.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.splitContainerLog.Panel1.ResumeLayout(false);
            this.splitContainerLog.Panel2.ResumeLayout(false);
            this.splitContainerLog.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLog)).EndInit();
            this.splitContainerLog.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.SplitContainer splitContainerLog;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.ToolStripButton toolStripButtonTest;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem installAX3DriverToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.Timer timerUpdate;
        private System.Windows.Forms.ToolStripMenuItem copySelectedEntriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem advancedIgnoreConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem deviceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem advancedSetFullChargeLevelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem advancedDownloadAndClearNonCompletedDevicesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem advancedWipeUnsavedRecordingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem advancedCancelDownloadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem advancedResetDeviceWithErrorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetDevicesWithCommsErrorsToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelAvailable;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListView listViewDevices;
        private System.Windows.Forms.ColumnHeader columnHeaderLed;
        private System.Windows.Forms.ColumnHeader columnHeaderId;
        private System.Windows.Forms.ColumnHeader columnHeaderBattery;
        private System.Windows.Forms.ColumnHeader columnHeaderStatus;
        private ConfigureControl configureControl;
    }
}

