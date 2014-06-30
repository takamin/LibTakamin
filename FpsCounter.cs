/*
 * Tracking Stage Version 1.10
 * Copyright © 2013-2014 Ritsumeikan University All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace kyokko.TrackingStage {
    /// <summary>
    /// 周波数のカウンタ
    /// </summary>
    public class FreqCounter {
        private List<DateTime> times = new List<DateTime>();
        private double freq = double.MaxValue;
        /// <summary>
        /// ためる数
        /// </summary>
        public int BufferCount { get; set; }
        /// <summary>
        /// 周波数
        /// </summary>
        public double Freq { get { return freq; } }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FreqCounter() { BufferCount = 10; }
        
        /// <summary>
        /// カウント更新
        /// </summary>
        public void Update() {
            //現在時刻の記録
            DateTime now = DateTime.Now;
            times.Add(now);

            //不要な過去の時刻を削除する
            int count = times.Count;
            int removeCount = count - BufferCount;
            if (removeCount > 0) {
                int removeStartIndex = count - removeCount;
                times.RemoveRange(0, removeCount);
                count -= removeCount;
            }
            //周波数の計算
            double freq = double.MaxValue;
            if (count > 0) {
                TimeSpan span = (now - times[0]);
                if (span.TotalSeconds > 0.0) {
                    freq = count / span.TotalSeconds;
                }
            }
            this.freq = freq;
        }
    }
}
