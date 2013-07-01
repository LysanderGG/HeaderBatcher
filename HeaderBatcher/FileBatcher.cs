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
        private String      m_headerToInsert;

        private String[]    m_toIgnore;

        /// <summary>
        /// Creates a new FileBatcher.
        /// </summary>
        /// <param name="_headerToInsertPath">Path to the header to insert.</param>
        /// <param name="_headersToRemovePaths">Paths to the headers to remove.</param>
        /// <param name="_ignoreFilePath">Path to an ignore definition file.</param>
        public FileBatcher(String _headerToInsertPath, String[] _headersToRemovePaths, String _ignoreFilePath = null) {

            int i = 0;
            m_headersToRemove = new String[_headersToRemovePaths.Length];   
            foreach(String path in _headersToRemovePaths) {
                if(File.Exists(path)) {
                    m_headersToRemove[i++] = File.ReadAllText(path, Encoding.UTF8);
                }
            }

            if(File.Exists(_headerToInsertPath)) {
                m_headerToInsert = File.ReadAllText(_headerToInsertPath, Encoding.UTF8);
            }
            
            if(File.Exists(_ignoreFilePath)) {
                m_toIgnore = File.ReadAllLines(_ignoreFilePath, Encoding.UTF8);
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

            if(!MustBeBatched(_path)) {
                return false;
            }

            String fileText = File.ReadAllText(_path, Encoding.UTF8);
            
            // Remove the first old header found.
            foreach(String headerToRemove in m_headersToRemove) {
                if(headerToRemove != null && fileText.StartsWith(headerToRemove)) {
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
        private bool MustBeBatched(String _path) {
            if(m_toIgnore != null) {
                foreach(String str in m_toIgnore) {
                    // Path like C:/somepath/file.sth
                    if(str.Equals(_path)) {
                        return false;
                    }

                    if(str.Contains('*')) {
                        // Path like C:/somepath/* 
                        if(str.EndsWith("*")) {
                            if(_path.StartsWith(str.Substring(0, str.Length - 1))) {
                                return false;
                            }
                        }
                    
                        // Path like *.sth
                        if(str.StartsWith("*")) {
                            if(_path.EndsWith(str.Substring(1, str.Length - 1))) {
                                return false;
                            }
                        }

                        // Path like C:/somepath/*.sth
                        if(!str.StartsWith("*") && str.Contains("*.")) {
                            String[] strArray = str.Split(new Char[]{'*'});
                            if(strArray.Length > 2) {
                                Console.WriteLine(str + " is not a valid path to ignore. Continue with next one.");
                            }
                            String path = strArray[0];
                            String ext  = strArray[1];

                            if(_path.StartsWith(path) && _path.EndsWith(ext)) {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

    }
}
