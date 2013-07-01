using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TilesToJPG
{
    public partial class MainForm : Form
    {
        private Thread m_convertThread;
        private ConvertTiles m_convertThreadData;

        public MainForm()
        {
            InitializeComponent();
        }

        private void buttonBrowseInput_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Select the file \"tilemapresource.xml\" from the root of the tiles folder.";
            dlg.Filter = "tilemapresource.xml|tilemapresource.xml|All Files (*.*)|*.*";
            dlg.FileName = textBoxInput.Text + "tilemapresource.xml";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBoxInput.Text = Path.GetDirectoryName(dlg.FileName) + "\\";
                if (textBoxOutput.Text.Length < 1)
                {
                    textBoxOutput.Text = Path.GetDirectoryName(dlg.FileName) + "_JPGs\\";
                }
            }
        }

        private void buttonOutput_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Select any file from the output folder.";
            dlg.Filter = "All Files (*.*)|*.*";
            dlg.FileName = textBoxOutput.Text + "any_file";
            dlg.CheckFileExists = false;
            dlg.CheckPathExists = false;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBoxOutput.Text = Path.GetDirectoryName(dlg.FileName) + "\\";
            }
        }

        private void textBoxQuality_Leave(object sender, EventArgs e)
        {
            int quality = 85;
            try
            {
                quality = int.Parse(textBoxQuality.Text);
            }
            catch { }
            textBoxQuality.Text = quality.ToString();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (m_convertThreadData == null)
            {
                textBoxLog.Clear();
                mainProgressBar.Value = 0;
                buttonStart.Text = "ABORT";

                m_convertThreadData = new ConvertTiles(this,
                    textBoxInput.Text,
                    textBoxOutput.Text,
                    int.Parse(textBoxQuality.Text),
                    checkBoxIncludeKML.Checked);

                ThreadStart start = new ThreadStart(m_convertThreadData.Run);
                m_convertThread = new Thread(start);
                m_convertThread.Start();
                threadTimer.Start();
            }
            else
            {
                buttonStart.Text = "Aborting...";
                m_convertThreadData.Abort();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void threadTimer_Tick(object sender, EventArgs e)
        {
            if ((m_convertThreadData != null) && (m_convertThread.IsAlive == false))
            {
                threadTimer.Stop();
                m_convertThread.Join();
                m_convertThread = null;

                if (m_convertThreadData.Mode == RunMode.Aborting)
                {
                    textBoxLog.AppendText("Aborted.");
                }
                else
                {
                    textBoxLog.AppendText("Done.");
                }

                buttonStart.Text = "Restart";

                m_convertThreadData = null;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_convertThreadData != null)
            {
                threadTimer.Stop();
                m_convertThreadData.Abort();
                m_convertThread.Join();
            }
        }

        #region Thread-safe message logging
        public delegate void LogDelegate(string message);

        public void LogDelegateMethod(string message)
        {
            textBoxLog.AppendText(message + "\r\n");
        }

        public IAsyncResult ThreadSafeLogMessage(string message)
        {
            object[] myArray = new object[1];
            myArray[0] = message;
            return BeginInvoke(new LogDelegate(LogDelegateMethod), myArray);
        }
        #endregion  // Thread-safe message logging

        #region Thread-safe progress bar
        public delegate void ProgressDelegate(int index, int count);

        public void ProgressDelegateMethod(int index, int count)
        {
            mainProgressBar.Maximum = count;
            mainProgressBar.Value = index;
        }

        public IAsyncResult ThreadSafeProgress(int index, int count)
        {
            object[] myArray = new object[2];
            myArray[0] = index;
            myArray[1] = count;
            return BeginInvoke(new ProgressDelegate(ProgressDelegateMethod), myArray);
        }
        #endregion  // Thread-safe progress bar
    }
}
