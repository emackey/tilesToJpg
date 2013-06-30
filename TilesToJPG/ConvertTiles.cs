using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
        private int m_quality;
        private RunMode m_runMode;
        private MainForm m_parent;
        private ImageCodecInfo m_imageCodecInfo;
        private EncoderParameters m_encoderParameters;

        public ConvertTiles(MainForm parent, string inputName, string outputName, int quality)
        {
            m_parent = parent;
            m_inputFolderName = inputName;
            m_outputFolderName = outputName;
            m_quality = quality;
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

            m_parent.ThreadSafeLogMessage("Found " + scanFiles.Length.ToString() + " files.");

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            m_imageCodecInfo = codecs.Where((info) => info.FormatDescription.Equals("JPEG")).First();

            m_encoderParameters = new EncoderParameters(1);
            m_encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, m_quality);

            int inputNameLength = m_inputFolderName.Length;

            int fileIndex = 1, fileCount = scanFiles.Length;
            foreach (string inFileName in scanFiles)
            {
                m_parent.ThreadSafeProgress(fileIndex, fileCount);

                if (!inFileName.Substring(0, inputNameLength).Equals(m_inputFolderName))
                {
                    m_parent.ThreadSafeLogMessage("Input folder name mismatch: " + inFileName);
                }
                else
                {
                    string outFileName = m_outputFolderName + inFileName.Substring(inputNameLength);
                    string extension = Path.GetExtension(inFileName).ToLowerInvariant();

                    if (extension.Equals(".kml"))
                    {
                        processKML(inFileName, outFileName);
                    }
                    else if (extension.Equals(".png"))
                    {
                        processPNG(inFileName, outFileName.Substring(0, outFileName.Length - 4) + ".jpg");
                    }
                }

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

        private void processKML(string inFileName, string outFileName)
        {
            string text = File.ReadAllText(inFileName);

            Directory.CreateDirectory(Path.GetDirectoryName(outFileName));
            File.WriteAllText(outFileName, text.Replace(".png", ".jpg"));
        }

        private void processPNG(string inFileName, string outFileName)
        {
            m_parent.ThreadSafeLogMessage(outFileName);

            Bitmap bitmap = new Bitmap(Image.FromFile(inFileName));
            bitmap.Save(outFileName, m_imageCodecInfo, m_encoderParameters);
        }
    }
}
