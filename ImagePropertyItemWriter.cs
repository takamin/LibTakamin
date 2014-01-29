using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace LibTakamin.Drawing.Imaging {
    
    // http://www.dinop.com/vc/exif03.html

    /// <summary>
    /// PropertyItemのID値
    /// </summary>
    public enum PropertyTags {
        /// <summary>
        /// 画像の概要
        /// </summary>
        ImageDescription = 0x010E,
        /// <summary>
        /// 画像を出力したソフトウェア
        /// </summary>
        Software = 0x0131,
        /// <summary>
        /// 画像の著作権者
        /// </summary>
        Copyright = 0x8298,
        /// <summary>
        /// メーカーのノート
        /// </summary>
        MakerNote = 0x927C,
        /// <summary>
        /// ユーザーのコメント
        /// </summary>
        UserComment = 0x9286,
    }
    /// <summary>
    /// PropertyItemのType値
    /// </summary>
    public enum PropertyTypes {
        /// <summary>
        /// ASCII文字列
        /// </summary>
        ASCII = 2,
    }
    /// <summary>
    /// 画像ファイルへメタデータ（PropertyItem）を追加するためのクラス。
    /// </summary>
    public class ImagePropertyItemWriter {
        //ファイル名
        private string filename = "";
        //イメージ（filenameを読み込んでコピーしたもの）
        private Image image = null;
        //メタデータを追加するときに使いまわすプロパティアイテム
        private PropertyItem propSeed = null;
        //PropertyItemのリスト
        private List<PropertyItem> propatyList = new List<PropertyItem>();
        //PropertyItemのIDからインスタンスへのディクショナリ
        private Dictionary<int, PropertyItem> propertyDictionaryById = new Dictionary<int, PropertyItem>();
        /// <summary>
        /// 既にあるファイルを読み込む。
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public ImagePropertyItemWriter Attach(string filename) {
            using (Image tmpImage = Image.FromFile(filename)) {
                Console.WriteLine("[PropertyItemAdder.Attach] " + filename);
                this.filename = filename;
                for (int i = 0; i < tmpImage.PropertyItems.Length; i++) {
                    PropertyItem prop = tmpImage.PropertyItems[i];
                    propatyList.Add(prop);
                    propertyDictionaryById.Add(prop.Id, prop);
                }
                if (tmpImage.PropertyItems.Length > 0) {
                    propSeed = tmpImage.PropertyItems[0];
                }
                image = new Bitmap(tmpImage);
            }
            return this;
        }
        /// <summary>
        /// PropertyItemの一覧を参照。設定はできません。
        /// </summary>
        public PropertyItem[] PropertyItem {
            get { return propatyList.ToArray(); }
        }
        /// <summary>
        /// 指定したIDのPropertyItemを得る。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PropertyItem GetPropertyItemById(PropertyTags id) {
            PropertyItem propertyItem = null;
            if (propertyDictionaryById.TryGetValue((int)id, out propertyItem)) {
                return propertyItem;
            }
            return null;
        }
        /// <summary>
        /// 読み込んだファイルを書き戻す。
        /// </summary>
        public void Done() {
            image.Save(filename);
            Console.WriteLine("[PropertyItemAdder.Done] " + filename);
        }
        /// <summary>
        /// 文字列のプロパティを設定する。
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ImagePropertyItemWriter SetPropertyItem(PropertyTags id, string value) {
            byte[] bytes = ImagePropertyItemWriter.StringToByteArray(value);
            SetPropertyItem(id, PropertyTypes.ASCII, bytes.Length, bytes);
            return this;
        }
        /// <summary>
        /// プロパティを設定する。
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="len"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private ImagePropertyItemWriter SetPropertyItem(PropertyTags id, PropertyTypes type, int len, byte[] value) {
            propSeed.Id = (int)id;
            propSeed.Type = (short)type;
            propSeed.Len = len;
            propSeed.Value = value;
            LogPropertyItem("[PropertyItemAdder.SetPropertyItem] ", propSeed);
            image.SetPropertyItem(propSeed);
            return this;
        }
        /// <summary>
        /// 文字列をASCIIバイト列に変換。
        /// 最後の要素はnull文字です。
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        static private byte[] StringToByteArray(string s) {
            char[] ca = s.ToCharArray();
            byte[] asciibytes = new byte[ca.Length + 1];
            for (int i = 0; i < ca.Length; i++) {
                asciibytes[i] = (byte)ca[i];
            }
            asciibytes[asciibytes.Length - 1] = 0;
            return asciibytes;
        }
        /// <summary>
        /// ASCIIバイト列を文字列に変換。
        /// </summary>
        /// <param name="asciibytes"></param>
        /// <returns></returns>
        static public string ByteArrayToString(byte[] asciibytes) {
            if (asciibytes.Length <= 1) {
                return "";
            }
            char[] ca = new char[asciibytes.Length - 1];
            for (int i = 0; i < ca.Length; i++) {
                ca[i] = (char)asciibytes[i];
            }
            return new string(ca);
        }
        /// <summary>
        /// PropertyItemをLogへ出力
        /// </summary>
        /// <param name="header"></param>
        /// <param name="prop"></param>
        public void LogPropertyItem(string header, PropertyItem prop) {
            StringBuilder pitembuf = new StringBuilder();
            pitembuf.Append(header).Append("{");
            pitembuf.Append("Id:");
            pitembuf.Append(prop.Id.ToString()).Append(",");
            pitembuf.Append("Type:");
            pitembuf.Append(prop.Type.ToString()).Append(",")
                .Append("Len:").Append(prop.Len.ToString()).Append(",")
                .Append("Value:");
            if (prop.Type == (short)PropertyTypes.ASCII) {
                char[] ca = new char[prop.Value.Length - 1];
                for (int j = 0; j < prop.Value.Length - 1; j++) {
                    ca[j] = (char)prop.Value[j];
                }
                string strValue = new String(ca);
                pitembuf.Append("'").Append(strValue).Append("', ");

            } else {
                pitembuf.Append("[");
                for (int j = 0; j < prop.Len; j++) {
                    pitembuf.Append(((int)prop.Value[j]).ToString()).Append(",");
                }
                pitembuf.Append("], ");
            }
            pitembuf.Append("}");
            Console.Write(pitembuf.ToString());
        }
    }
}
