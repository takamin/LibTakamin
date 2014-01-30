using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibTakamin {
    public static class FileUtil {
        static public long GetSize(string filename) {
            long filelen = -1;
            try {
                FileInfo fileInfo = new FileInfo(filename);
                filelen = fileInfo.Length;
            } catch (Exception) { }
            return filelen;
        }
    }
}
