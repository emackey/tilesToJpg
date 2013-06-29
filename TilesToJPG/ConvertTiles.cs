using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilesToJPG
{
    public enum RunMode
    {
        Idle, Running, Aborting, Complete
    }

    public class ConvertTiles
    {
        private string m_inputFolderName;
        private string m_outputFolderName;
        private RunMode m_runMode;
        private MainForm m_parent;

        public ConvertTiles(MainForm parent, string inputName, string outputName)
        {
            m_parent = parent;
            m_inputFolderName = inputName;
            m_outputFolderName = outputName;
            m_runMode = RunMode.Idle;
        }

        public void Abort()
        {
            m_runMode = RunMode.Aborting;
        }

        public RunMode Mode
        {
            get { return m_runMode; }
        }

        public void Run()
        {
            m_runMode = RunMode.Running;

            string[] scanFiles;
            try
            {
                scanFiles = Directory.GetFiles(m_inputFolderName,
                    "*.*", SearchOption.AllDirectories);
            }
            catch (Exception ex)
            {
                m_parent.ThreadSafeLogMessage("Error scanning folder: " + ex.Message);
                m_runMode = RunMode.Aborting;
                return;
            }

            m_parent.ThreadSafeLogMessage("Found files: " + scanFiles.Length.ToString());

            int fileIndex = 1, fileCount = scanFiles.Length;
            foreach (string file in scanFiles)
            {
                m_parent.ThreadSafeProgress(fileIndex, fileCount);
                //m_parent.ThreadSafeLogMessage(file);

                if (m_runMode != RunMode.Running)
                {
                    break;
                }

                ++fileIndex;
            }

            if (m_runMode == RunMode.Running)
            {
                m_runMode = RunMode.Complete;
            }
        }
    }
}
