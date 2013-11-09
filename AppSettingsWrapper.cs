using System;
using System.Collections.Specialized;
using System.Configuration;

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
    }
}
