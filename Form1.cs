using MediaInfoLib;
using MetadataExtractor;
using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Diagnostics;

namespace MediaInfo_ {
    public partial class Form : System.Windows.Forms.Form {
        private readonly string argFileName = "";

        public Form() {
            InitializeComponent();
        }

        public Form(string[] args) {
            InitializeComponent();
            if (args.Length >= 1) {
                argFileName = args[0];
            }
        }

        private void Form1_Load(object sender, EventArgs e) {
            Text = "RainySummer/MediaInfo+";
            if (argFileName != "") {
                var file = new FileInfo(argFileName);
                MetadateQuery(file);
            }
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
                int fileType = 0; // Default is Other File Type
                string miFile = mi.Inform();
                string[] miFiles = miFile.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var miLine in miFiles) {
                    if (miLine == "Image") {
                        fileType = 1;
                        break;
                    } else if (miLine == "Video" || miLine == "Audio") {
                        fileType = 2;
                        break;
                    }
                }
                if (fileType == 1) {
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
                            tagName = tagName.Replace(" - ", ": ");
                            treeView.Nodes[0].Nodes[index].Nodes.Add(tagName);
                        }
                        index++;
                    }
                } else if (fileType == 2) {
                    // Video or Audio File
                    string mediaInfo = mi.Inform();
                    string[] miParameters = mediaInfo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    int index = -1;
                    foreach (var miOutput in miParameters) {
                        if (miOutput == "General" || miOutput == "Video" || miOutput == "Audio" || miOutput == "Text") {
                            treeView.Nodes[0].Nodes.Add(miOutput);
                            index++;
                        } else {
                            string tagName = Regex.Replace(miOutput, "\\s{2,}:", ":");
                            treeView.Nodes[0].Nodes[index].Nodes.Add(tagName);
                        }
                    }
                } else {
                    //
                    string mediaInfo = mi.Inform();
                    string[] miParameters = mediaInfo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    int index = -1;
                    foreach (var miOutput in miParameters) {
                        if (miOutput == "General" || miOutput == "Text") {
                            treeView.Nodes[0].Nodes.Add(miOutput);
                            index++;
                        } else {
                            string tagName = Regex.Replace(miOutput, "\\s{2,}:", ":");
                            treeView.Nodes[0].Nodes[index].Nodes.Add(tagName);
                        }
                    }
                    treeView.Nodes[0].Nodes.Add("System");
                    index++;
                    treeView.Nodes[0].Nodes[index].Nodes.Add("Extension: " + file.Extension);
                    treeView.Nodes[0].Nodes[index].Nodes.Add("File Length: " + file.Length + " bytes");
                    string isReadOnly = file.IsReadOnly ? "Yes" : "No";
                    treeView.Nodes[0].Nodes[index].Nodes.Add("Read Only: " + isReadOnly);
                    treeView.Nodes[0].Nodes[index].Nodes.Add("Creation Time: " + file.CreationTime);
                    treeView.Nodes[0].Nodes[index].Nodes.Add("Creation Time Utc: " + file.CreationTimeUtc);
                    treeView.Nodes[0].Nodes[index].Nodes.Add("Last Access Time: " + file.LastAccessTime);
                    treeView.Nodes[0].Nodes[index].Nodes.Add("Last Access Time Utc: " + file.LastAccessTimeUtc);
                    treeView.Nodes[0].Nodes[index].Nodes.Add("Last Write Time: " + file.LastWriteTime);
                    treeView.Nodes[0].Nodes[index].Nodes.Add("Last Write TimeUtc: " + file.LastWriteTimeUtc);
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
            Process thisProcess = Process.GetProcessById(Process.GetCurrentProcess().Id);
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

        private void TreeView_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                e.Effect = DragDropEffects.Link;
            } else {
                e.Effect = DragDropEffects.None;
            }
        }

        private void TreeView_DragDrop(object sender, DragEventArgs e) {
            string fileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            var file = new FileInfo(fileName);
            MetadateQuery(file);
        }
    }
}
