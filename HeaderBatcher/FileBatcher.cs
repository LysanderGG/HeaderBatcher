using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace HeaderBatcher
{
    class FileBatcher
    {
        private String[]    m_headersToRemove;
        private String      m_headerToInsert;

        private String      m_ignorePath;

        /// <summary>
        /// Creates a new FileBatcher.
        /// </summary>
        /// <param name="_headerToInsertPath">Path to the header to insert.</param>
        /// <param name="_headersToRemovePaths">Paths to the headers to remove.</param>
        /// <param name="_ignoreFilePath">Path to an ignore definition file.</param>
        public FileBatcher(String _headerToInsertPath, String[] _headersToRemovePaths, String _ignoreFilePath = null) {

            foreach(String path in _headersToRemovePaths) {
                m_headersToRemove = new String[_headersToRemovePaths.Length];
                int i = 0;
                if(File.Exists(path)) {
                    m_headersToRemove[i++] = File.ReadAllText(path, Encoding.UTF8);
                }
            }

            if(File.Exists(_headerToInsertPath)) {
                m_headerToInsert = File.ReadAllText(_headerToInsertPath, Encoding.UTF8);
            }
            
            if(File.Exists(_ignoreFilePath)) {
                m_ignorePath = _ignoreFilePath;
            }

        }
        
        /// <summary>
        /// Batches one file.
        /// Remove the first one of the old headers if any at the beginning of the file
        /// Add the new header at the beginning of the file
        /// </summary>
        /// <param name="_path">Path of the file to patch.</param>
        /// <returns>True if the file has been patched. False otherwise.</returns>
        public bool BatchOne(String _path) {
            if(!File.Exists(_path)) {
                return false;
            }

            String fileText = File.ReadAllText(_path, Encoding.UTF8);
            
            // Remove the first old header found.
            foreach(String headerToRemove in m_headersToRemove) {
                if(fileText.StartsWith(headerToRemove)) {
                    fileText = fileText.Remove(0, headerToRemove.Length);
                    break;
                }
            }

            // Insert the new one
            fileText = fileText.Insert(0, m_headerToInsert);
            
            File.WriteAllText(_path, fileText, Encoding.UTF8);

            return true;
        }
        
        /// <summary>
        /// Batch all files from the given path recursively.
        /// </summary>
        /// <param name="_path">Path to start.</param>
        /// <returns>The number of files edited.</returns>
        public int BatchAll(String _path) {
            int cnt = 0;
            
            String[] filePaths = Directory.GetFiles(_path);
            foreach(String filePath in filePaths) {
                bool res = BatchOne(filePath);
                if(res) { ++cnt; }
            }

            return cnt;
        }

    }
}
