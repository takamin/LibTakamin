using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace LibTakamin.Web {
    public class CssUtil {
        static public Color ParseColor(string s) {
            Color c = Color.Transparent;
            char head = s[0];
            if (head == '#') {
                int argb = (Int32)((UInt32)Convert.ToInt32(s.Substring(1), 16) | 0xff000000);
                c = Color.FromArgb(argb);
            } else if ('0' <= head && head <= '9') {
                int argb = (Int32)((UInt32)int.Parse(s) | 0xff000000);
                c = Color.FromArgb(argb);
            } else {
                c = Color.FromName(s);
            }
            return c;
        }
    }
}
