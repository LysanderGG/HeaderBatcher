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

        private String      m_path;
        private String      m_ignorePath;

        public FileBatcher(String _path, String _headerToInsertPath, String[] _headersToRemovePaths, String _ignoreFilePath = null) {
            m_path = _path;

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
        
        public int BatchAll() {
            int cnt = 0;
            
            String[] filePaths = Directory.GetFiles(m_path);
            foreach(String filePath in filePaths) {
                BatchOne(filePath);
            }

            return cnt;
        }

    }
}
