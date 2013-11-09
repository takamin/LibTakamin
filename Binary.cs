using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibTakamin {
    class Binary {
        static string Dump(byte[] buffer) {
            int pageBytes = 1024;
            int colBytes = 16;
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < buffer.Length; i++) {

                if (i % pageBytes == 0) {
                }
                if (i % colBytes == 0) {
                }
                byte b = buffer[i];
                sb.Append(" ").Append(b.ToString("X2"));
                if ((i+1) % colBytes == 0) {
                    sb.Append("\n");
                }
                if ((i + 1) % pageBytes == 0) {
                    for (int j = 0; j < colBytes; j++) {
                        sb.Append("---");
                    }
                }
            }



            return sb.ToString();
        }
    }
}
