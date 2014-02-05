using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Drawing;

namespace LibTakamin.Configuration {
    public class AppSettingsWrapper {

        private NameValueCollection appSettings = null;
        /// <summary>
        /// コンストラクタ。
        /// 
        /// 引数のappSettingsについて、
        /// System.Configurationが参照設定されていない場合があり、
        /// コンパイルエラーになることがある。
        /// プロジェクトの参照設定を右クリックして[参照の追加]を選択し、
        /// [.NET]タブで「System.Configuration」を参照。
        /// 
        /// </summary>
        /// <param name="appSettings">利用するアプリケーションのSystem.Configuration.ConfigurationManager.AppSettingsを指定します。</param>
        public AppSettingsWrapper(NameValueCollection appSettings) {
            this.appSettings = appSettings;
        }

        /// <summary>
        /// 設定情報読み出し
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dflValue"></param>
        /// <returns></returns>
        public string GetValueAsString(string key, string dflValue) {
            string value = appSettings[key];
            if (value == null) {
                return dflValue;
            }
            return value;
        }
        /// <summary>
        /// 設定情報読み出し
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dflValue"></param>
        /// <returns></returns>
        private int GetValueAsInt(string key, int dflValue) {
            string s = appSettings[key];
            if (s == null) {
                return dflValue;
            }
            return int.Parse(s);
        }
        /// <summary>
        /// 設定情報読み出し
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dflValue"></param>
        /// <returns></returns>
        protected double GetAppSettingDouble(string key, double dflValue) {
            string s = appSettings[key];
            if (s == null) {
                return dflValue;
            }
            return double.Parse(s);
        }
        protected Color GetAppSettingColor(string key, Color dflValue) {
            string s = appSettings[key];
            if (s == null) {
                return dflValue;
            }
            return LibTakamin.Web.CssUtil.ParseColor(s);
            //Color c = dflValue;
            //char head = s[0];
            //if (head == '#') {
            //    int argb = (Int32)((UInt32)Convert.ToInt32(s.Substring(1), 16) | 0xff000000);
            //    c = Color.FromArgb(argb);
            //} else if ('0' <= head && head <= '9') {
            //    int argb = (Int32)((UInt32)int.Parse(s) | 0xff000000);
            //    c = Color.FromArgb(argb);
            //} else {
            //    c = Color.FromName(s);
            //}
            //return c;
        }
    }
}
