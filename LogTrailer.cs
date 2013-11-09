using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace LibTakamin {

    /// <summary>
    /// ファイルの末尾を追加読み込みするクラス。
    /// インスタンス化して、読み込みたいファイルのファイル名をFilenameプロパティを設定し、
    /// その後は定期的にReadメソッドを呼び出して行をファイル末尾の行を取得できます。
    /// サイズの大きいファイルの初回のReadメソッドでは、ファイルをすべて読み込みますので、
    /// しばらく処理時間が必要です。
    /// 2回目以降の読み込みは、前回読み込んだ後に追加された情報だけを読み取ります。
    /// </summary>
    /// 
    public class LogTrailReader {

        #region イベント
        /// <summary>
        /// エンコーディング変更イベント。
        /// ファイルの初回読み込み時にJcodeによるエンコーディング自動判定により発生します。
        /// </summary>
        public event EventHandler EncodingChanged;

        /// <summary>
        /// エンコーディング変更イベントを発生させる。
        /// </summary>
        /// <param name="e"></param>
        protected void OnEncodingChanged(EventArgs e) {
            if (EncodingChanged != null) {
                EncodingChanged(this, e);
            }
        }
        #endregion

        #region プライベートフィールド
        #region 定数
        /// <summary>
        /// 読み込みバッファサイズ
        /// </summary>
        private const int READ_BUFFER_SIZE = 1024 * 1024;
        #endregion

        /// <summary>
        /// 読み込みバッファ
        /// </summary>
        private byte[] buffer = new byte[READ_BUFFER_SIZE];
        
        /// <summary>
        /// ファイル名
        /// </summary>
        private string filename = "";
        
        /// <summary>
        /// ファイルの読み込み位置。
        /// </summary>
        private long seekPos = 0;

        /// <summary>
        /// 改行されていない最終行
        /// </summary>
        private string lastline = "";
        
        /// <summary>
        /// 文字エンコーディング
        /// </summary>
        private Encoding encoding = null;

        #endregion

        #region プロパティ
        /// <summary>
        /// ファイル名。
        /// </summary>
        public string Filename {
            get {
                return filename;
            }
            set {
                if (this.filename != value) {
                    this.filename = value;
                    seekPos = 0;
                    lastline = "";
                    encoding = null;
                }
            }
        }
        /// <summary>
        /// 文字エンコーディング
        /// </summary>
        public Encoding Encoding {
            get {
                return encoding;
            }
            set {
                if (value == null) {
                    value = Encoding.Default;
                }
                if (encoding == null || !encoding.Equals(value)) {
                    encoding = value;
                    seekPos = 0;
                    lastline = "";
                    OnEncodingChanged(new EventArgs());
                }
            }
        }
        #endregion

        #region パブリックメソッド 
        /// <summary>
        /// 前回読み込み時から追加された行の読み込み
        /// </summary>
        /// <param name="lines"></param>
        public void Read(List<string> lines) {
            long filelen = -1;
            try {
                FileInfo fileInfo = new FileInfo(filename);
                filelen = fileInfo.Length;
            } catch (Exception) {
                return;
            }

            if (filelen >= 0 && seekPos < filelen) {
                FileStream fstream = new FileStream(filename,
                    FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite);
                fstream.Seek(seekPos, SeekOrigin.Begin);
                while (seekPos < filelen) {
                    int readsize = buffer.Length;
                    if (filelen - seekPos < buffer.Length) {
                        readsize = (int)(filelen - seekPos);
                    }
                    readsize = fstream.Read(buffer, 0, readsize);
                    if (readsize >= 0) {
                        seekPos += readsize;
                        if (encoding == null) {
                            byte[] buf = buffer.Take(readsize).ToArray();
                            Encoding assumed = net.dobon.dotnet._string_.detectcode.Jcode.GetCode(buf);
                            if (assumed == null) {
                                assumed = System.Text.Encoding.Default;
                            }
                            encoding = assumed;
                            OnEncodingChanged(new EventArgs());
                        }
                        Parse(buffer, readsize, lines);
                    }
                }
                fstream.Close();
            }
        }
        #endregion

        #region プライベートメソッド
        /// <summary>
        /// バイト列を行単位に分割して文字列配列に格納
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="size"></param>
        /// <param name="lines"></param>
        private void Parse(byte[] buffer, int size, List<string> lines) {
            System.Diagnostics.Debug.WriteLine(
                new StringBuilder(filename).Append(" ")
                .Append(size).Append("bytes read"));
            int charsUsed = 0;
            char[] charsArray = Convert(buffer, size, out charsUsed);
            foreach (char c in charsArray) {
                if (c == '\n') {
                    lines.Add(lastline);
                    lastline = "";
                } else if (c != '\r') {
                    lastline += c;
                }
            }
        }

        /// <summary>
        /// バイト列を文字列に変換
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="size"></param>
        /// <param name="encoding"></param>
        /// <param name="charsUsed"></param>
        /// <returns></returns>
        private char[] Convert(byte[] buffer, int size, out int charsUsed) {
            Decoder decoder = encoding.GetDecoder();
            int charCount = decoder.GetCharCount(buffer, 0, size);
            char[] chars = new char[charCount];
            int bytesUsed;
            bool completed;
            decoder.Convert(
                buffer, 0, buffer.Length, chars, 0, charCount, true,
                out bytesUsed, out charsUsed, out completed);
            return chars;
        }
        #endregion
    }
}
