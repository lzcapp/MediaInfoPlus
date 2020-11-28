using MediaInfoLib;
using MetadataExtractor;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MediaInfo_ {
    public partial class Form : System.Windows.Forms.Form {
        public Form() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            Text = "RainySummer/MediaInfo+";
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog {
                //InitialDirectory = "c://",
                //Filter = "Pictures|*.*|Videos|*.*|All Files|*.*",
                Filter = "Multimedia Files|*.*",
                RestoreDirectory = true,
                FilterIndex = 1
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK) {
                string fileName = openFileDialog.FileName;
                var file = new FileInfo(fileName);
                MetadateQuery(file);
            }
        }

        private void MetadateQuery(FileInfo file) {
            Text = "RainySummer/MediaInfo+" + " - " + file.FullName;
            treeView.BeginUpdate();
            treeView.Nodes.Clear();
            treeView.Nodes.Add(file.FullName);
            try {
                var mi = new MediaInfo();
                mi.Open(file.FullName);
                string strFormat;
                try {
                    strFormat = mi.Get(StreamKind.General, 0, "Format");
                } catch (Exception) {
                    mi.Close();
                    mi.Dispose();
                    return;
                }
                if (strFormat == "JPEG" || strFormat == "TIFF" || strFormat == "WebP" || strFormat == "PNG" || strFormat == "PSD") {
                    // Picture File
                    mi.Close();
                    var directories = ImageMetadataReader.ReadMetadata(file.FullName);
                    int index = 0;
                    foreach (var directory in directories) {
                        string directoryName = directory.Name;
                        Console.WriteLine(directoryName);
                        treeView.Nodes[0].Nodes.Add(directoryName);
                        for (var j = 0; j < directory.TagCount; j++) {
                            string tagName = directory.Tags[j].ToString();
                            tagName = tagName.Replace("[" + directoryName + "] ", "");
                            treeView.Nodes[0].Nodes[index].Nodes.Add(tagName);
                        }
                        index++;
                    }
                } else {
                    // Video File
                    string mediaInfo = mi.Inform();
                    string[] miParameters = mediaInfo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    int index = -1;
                    foreach (var miOutput in miParameters) {
                        if (miOutput == "General" || miOutput == "Video" || miOutput == "Audio" || miOutput == "Text") {
                            treeView.Nodes[0].Nodes.Add(miOutput);
                            index++;
                        } else {
                            treeView.Nodes[0].Nodes[index].Nodes.Add(miOutput);
                        }
                    }
                }
                mi.Dispose();
            } catch (Exception) {
                treeView.EndUpdate();
                return;
            }
            treeView.ExpandAll();
            treeView.EndUpdate();
            treeView.Nodes[0].EnsureVisible();
            copyToolStripMenuItem.Visible = true;
            closeToolStripMenuItem.Visible = true;
            closeToolStripMenuItem1.Visible = true;
        }

        private void CloseToolStripMenuItem_Click(object sender, EventArgs e) {
            Text = "RainySummer/MediaInfo+";
            treeView.BeginUpdate();
            treeView.Nodes.Clear();
            treeView.EndUpdate();
            copyToolStripMenuItem.Visible = false;
            closeToolStripMenuItem.Visible = false;
            closeToolStripMenuItem1.Visible = false;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e) {
            //System.Environment.Exit(0);
            System.Diagnostics.Process thisProcess = System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id);
            thisProcess.Kill();
        }

        private void TreeView_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                var ClickPoint = new Point(e.X, e.Y);
                var CurrentNode = treeView.GetNodeAt(ClickPoint);
                if (CurrentNode != null) {
                    CurrentNode.ContextMenuStrip = contextMenuStrip;
                    treeView.SelectedNode = CurrentNode;
                    string nodeText = treeView.SelectedNode.Text.ToString();
                    Clipboard.SetData(DataFormats.Text, (Object)nodeText);
                }
            }
        }
    }
}
