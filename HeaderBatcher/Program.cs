using System;
using System.Text;

namespace HeaderBatcher
{
    class Program
    {
        private static String   s_path              = "C:/Users/Lysander/Desktop/Test/FolderToBatch";
        private static String   s_newHeaderPath     = "C:/Users/Lysander/Desktop/Test/newHeader.txt";
        private static String[] s_oldHeadersPaths   = { 
                                                        "C:/Users/Lysander/Desktop/Test/newHeader.txt",
                                                        "C:/Users/Lysander/Desktop/Test/oldHeader1.txt"
                                                        
                                                    };
        private static String   s_blackListPath     = "C:/Users/Lysander/Desktop/Test/blackList.txt";
        private static String   s_witheListPath     = "C:/Users/Lysander/Desktop/Test/whiteList.txt";
        private static String[] s_ignoreHeadersPaths= {
                                                          "C:/Users/Lysander/Desktop/Test/oldHeader2.txt"
                                                      };

        static void Main(string[] args)
        {
            FileBatcher fb = new FileBatcher(s_newHeaderPath, s_oldHeadersPaths, s_ignoreHeadersPaths, s_blackListPath, s_witheListPath);

            try {
                int res = fb.BatchAll(s_path);
                Console.WriteLine("Headers Added : " + res);
            } catch(Exception e) {
                Console.WriteLine("## Exception ##");
                Console.WriteLine("#");
                Console.WriteLine("# Message     : " + e.Message);
                Console.WriteLine("#");
                Console.WriteLine("# Stack Trace : " + e.StackTrace);
                Console.WriteLine("#");
                Console.WriteLine("## End Of Exception ##");
            }

        }
    }
}
