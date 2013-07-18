using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HeaderBatcher
{
    class FileBatcher
    {
        private String[]    m_headersToRemove;
        private String[]    m_headersToIgnore;
        private String      m_headerToInsert;

        private String[]    m_blackList;
        private String[]    m_whiteList;

        /// <summary>
        /// Creates a new FileBatcher.
        /// </summary>
        /// <param name="_headerToInsertPath">Path to the header to insert.</param>
        /// <param name="_headersToRemovePaths">Paths to the headers to remove.</param>
        /// <param name="_headersToIgnorePaths">Paths to headers to ignore. The files containing these headers won't be patched.</param>
        /// <param name="_blackListFilePath">Path to the black list file.</param>
        /// <param name="_whiteListFilePath">Path to the white list file.</param>
        public FileBatcher(String _headerToInsertPath, String[] _headersToRemovePaths = null, String[] _headersToIgnorePaths = null, String _blackListFilePath = null, String _whiteListFilePath = null) {

            if(_headersToRemovePaths == null) {
                m_headersToRemove = null;
            } else {
                int i = 0;
                m_headersToRemove = new String[_headersToRemovePaths.Length];   
                foreach(String path in _headersToRemovePaths) {
                    if(path != null && File.Exists(path)) {
                        m_headersToRemove[i++] = File.ReadAllText(path, Encoding.UTF8);
                    }
                }
            }

            if(_headersToIgnorePaths == null) {
                m_headersToIgnore = null;
            } else {
                int i = 0;
                m_headersToIgnore = new String[_headersToIgnorePaths.Length];
                foreach(String path in _headersToIgnorePaths) {
                    if(path != null && File.Exists(path)) {
                        m_headersToIgnore[i++] = File.ReadAllText(path, Encoding.UTF8);
                    }
                }
            }

            if(File.Exists(_headerToInsertPath)) {
                m_headerToInsert = File.ReadAllText(_headerToInsertPath, Encoding.UTF8);
            }
            
            if(File.Exists(_blackListFilePath)) {
                m_blackList = File.ReadAllLines(_blackListFilePath, Encoding.UTF8);
            }

            if(File.Exists(_whiteListFilePath)) {
                m_whiteList = File.ReadAllLines(_whiteListFilePath, Encoding.UTF8);
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

            if(!IsWhiteListed(_path) || IsBlackListed(_path)) {
                return false;
            }

            String fileText = File.ReadAllText(_path, Encoding.UTF8);
            
            // Is the file containing a header we have to ignore ?
            if(m_headersToIgnore != null) {
                foreach(String headerToIgnore in m_headersToIgnore) {
                    if(headerToIgnore != null && fileText.StartsWith(headerToIgnore)) {
                        fileText = fileText.Remove(0, headerToIgnore.Length);
                        return false;
                    }
                }
            }

            // Remove the first old header found.
            if(m_headersToRemove != null) {
                foreach(String headerToRemove in m_headersToRemove) {
                    if(headerToRemove != null && fileText.StartsWith(headerToRemove)) {
                        fileText = fileText.Remove(0, headerToRemove.Length);
                        break;
                    }
                }
            }

            // Insert the new header
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
            if(!File.Exists(_path) && !Directory.Exists(_path)) {
                throw new Exception("The given path is not a directory nor a file.");
            }

            // If the path refers to a file, just call BatchOne.
            if(File.Exists(_path)) {
                return (BatchOne(_path) ? 1 : 0);
            }

            // Else recursive call into the directory.
            int nbModifiedFiles = 0;

            // Process files in the path directory
            String[] filePaths = Directory.GetFiles(_path);
            foreach(String filePath in filePaths) {
                bool res = BatchOne(filePath.Replace("\\", "/"));
                if(res) { ++nbModifiedFiles; }
            }

            // Recursive call on sub directories
            String[] subDirPaths = Directory.GetDirectories(_path);
            foreach(String subDirPath in subDirPaths) {   
                nbModifiedFiles += BatchAll(subDirPath);
            }

            return nbModifiedFiles;
        }

        /// <summary>
        /// Tells if a file must be patched by the FileBatcher, using the rules in the Ignore File.
        /// </summary>
        /// <param name="_path">Path of the file to patched or not.</param>
        /// <returns></returns>
        private bool IsBlackListed(String _path) {
            if(m_blackList != null) {
                foreach(String str in m_blackList) {
                    if(FileBatcher.MatchPath(str, _path)) {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Tells if a file should be patched using the WhiteList
        /// </summary>
        /// <param name="_path">Path of the file.</param>
        /// <returns>True if the file is whitelisted or if there is no white list. False otherwise.</returns>
        private bool IsWhiteListed(String _path) {
            if(m_whiteList == null) {
                return true;
            }

            foreach(String str in m_whiteList) {
                if(FileBatcher.MatchPath(str, _path)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tells if a path matches a path format.
        /// It can have the following formats : 
        /// # C:/somepath/file.sth
        /// # C:/somepath/*
        /// # C:/somepath/*.sth
        /// # *.sth
        /// </summary>
        /// <param name="_pathFormat">Path format.</param>
        /// <param name="_path">Path to verify the matching.</param>
        /// <returns>True if it matches, False otherwise.</returns>
        private static bool MatchPath(String _pathFormat, String _path) {
            // Path like C:/somepath/file.sth
            if(_pathFormat.Equals(_path)) {
                return true;
            }

            if(_pathFormat.Contains('*')) {
                // Path like C:/somepath/* 
                if(_pathFormat.EndsWith("*")) {
                    if(_path.StartsWith(_pathFormat.Substring(0, _pathFormat.Length - 1))) {
                        return true;
                    }
                }
                    
                // Path like *.sth
                if(_pathFormat.StartsWith("*")) {
                    if(_path.EndsWith(_pathFormat.Substring(1, _pathFormat.Length - 1))) {
                        return true;
                    }
                }

                // Path like C:/somepath/*.sth
                if(!_pathFormat.StartsWith("*") && _pathFormat.Contains("*.")) {
                    String[] strArray = _pathFormat.Split(new Char[]{'*'});
                    if(strArray.Length > 2) {
                        Console.WriteLine(_pathFormat + " is not a valid path to ignore. Continue with next one.");
                    }
                    String path = strArray[0];
                    String ext  = strArray[1];

                    if(_path.StartsWith(path) && _path.EndsWith(ext)) {
                        return true;
                    }
                }
            }

            return false;
        }

    }
}
