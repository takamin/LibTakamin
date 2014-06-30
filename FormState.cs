/*
 * Tracking Stage Version 1.10
 * Copyright © 2013-2014 Ritsumeikan University All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace kyokko.TrackingStage {
    /// <summary>
    /// 状態遷移基本クラス
    /// </summary>
    public class FormState {
        /// <summary>
        /// 次の状態を表すID
        /// </summary>
        protected const String NEXT_STATE_ID = "//NEXTSTATE";

        /// <summary>
        /// ロガー
        /// </summary>
        protected static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 状態遷移イベント
        /// </summary>
        private event EventHandler TransitionState;

        /// <summary>
        /// フォーム
        /// </summary>
        private Form form = null;

        /// <summary>
        /// 子状態のディクショナリ。キーは状態ID
        /// </summary>
        private Dictionary<String, FormState> stateMap = new Dictionary<String, FormState>();
        
        /// <summary>
        /// 子状態のリスト
        /// </summary>
        private List<FormState> stateList = new List<FormState>();

        /// <summary>
        /// 現在の遷移先状態
        /// </summary>
        private FormState currState = null;

        /// <summary>
        /// 遷移先状態ID（状態遷移イベント発報後に有効）
        /// </summary>
        private String NextStateId { get; set; }
        
        /// <summary>
        /// 状態遷移ID
        /// </summary>
        private String Id { get; set; }
        
        /// <summary>
        /// 親状態
        /// </summary>
        private FormState ParentState { get; set; }
        
        /// <summary>
        /// 親状態遷移中の位置
        /// </summary>
        private int Index { get; set; }
        
        /// <summary>
        /// 画面表示メッセージ
        /// </summary>
        protected String Message { get; set; }

        /// <summary>
        /// 遷移状態を参照する。
        /// nullの場合は、状態マシンが開始していない。
        /// 最上位の状態マシン以外では必ずnull。
        /// </summary>
        public FormState CurrentState { get { return currState; } }

        /// <summary>
        /// フォームインスタンスの設定
        /// </summary>
        /// <param name="form"></param>
        public virtual void SetForm(Form form) {
            this.form = form;
            for (int i = 0; i < stateList.Count; i++) {
                FormState fts = stateList[i] as FormState;
                fts.SetForm(form);
            }
        }

        /// <summary>
        /// 状態マシンの開始。内包する状態の一番最初の状態から開始する。
        /// </summary>
        public void Start() {
            currState = GetFirstState(this);
            List<FormState> pathList = currState.GetCanonicalPathList();
            EnterState(pathList, 0);
        }
        /// <summary>
        /// 状態マシンの終了。
        /// </summary>
        public void Stop() {
            if (currState != null) {
                List<FormState> pathList = currState.GetCanonicalPathList();
                ExitState(pathList, 0);
            }
        }
        /// <summary>
        /// 状態マシンの再起動
        /// </summary>
        public void Restart() {
            Stop();
            Start();
        }

        /// <summary>
        /// この状態で処理するイベントハンドラの登録と解除
        /// </summary>
        /// <param name="bInstall"></param>
        protected virtual void OnInstallEvent(bool bInstall) {
            debug("OnInstallEvent(" + bInstall + ")");
        }

        /// <summary>
        /// 状態へ遷移したときの処理
        /// </summary>
        protected virtual void OnEnter() {
            debug("OnEnter");
        }
        
        /// <summary>
        /// 状態から出るときの処理
        /// </summary>
        protected virtual void OnExit() {
            debug("OnExit");
        }

        /// <summary>
        /// 子状態を追加
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        protected void AddState(String id, FormState state, String message) {
            state.ParentState = this;
            state.Id = id;
            stateMap.Add(id, state);
            state.Message = message;
            state.Index = stateList.Count;
            stateList.Add(state);
        }

        /// <summary>
        /// フォームのメインスレッドのコンテキストで処理を実行（同期）。
        /// Invokeが不要なら直接実行します。
        /// </summary>
        /// <param name="method"></param>
        protected virtual void Invoke(MethodInvoker method) {
            if (form.InvokeRequired) {
                form.Invoke(method);
            } else {
                method.Invoke();
            }
        }

        /// <summary>
        /// 状態遷移する
        /// </summary>
        /// <param name="nextStateId"></param>
        public void TransitionTo(String nextStateId) {
            logger.Info("状態遷移:\"" + nextStateId + "\"");
            NextStateId = nextStateId;
            if (TransitionState != null) {
                TransitionState(this, new EventArgs());
            }
        }

        /// <summary>
        /// 状態遷移イベントの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void currentState_TransitionState(object sender, EventArgs e) {
            FormState stateSender = sender as FormState;
            if (stateSender != null) {
                if (!transitionInProcess) {
                    transitionInProcess = true;
                    form.BeginInvoke((MethodInvoker)delegate {
                        info(stateSender.NextStateId + "への状態遷移が処理されます。");
                        DoTransitionState(stateSender.NextStateId);
                    });
                } else {
                    info("既に状態遷移が発生していますので、" + stateSender.NextStateId + "への遷移は処理されません。");
                }
            }
        }
        //状態遷移処理中フラグ
        private bool transitionInProcess = false;

        /// <summary>
        /// 状態遷移する
        /// </summary>
        private void DoTransitionState(string nextStateId) {
            //遷移先状態を取得
            FormState nextState = GetState(nextStateId);
            if (nextState == null) {
                logger.Error("遷移先状態が見つかりません。"
                    + "class " + currState.GetType().FullName + " "
                    + "状態パス:\"" + currState.GetCanonicalPath() + "\" から、\"" + currState.NextStateId + "\"");
            }

            //遷移元と先の状態でルート状態から共通の祖先状態の数を取得。
            List<FormState> nextStateParents = nextState.GetCanonicalPathList();
            List<FormState> currStateParents = currState.GetCanonicalPathList();
            int commonCount = GetCommonPathCount(nextStateParents, currStateParents);

            //現在状態から互いに共通の祖先の状態まで、すべての親状態のStateOutの処理を実行
            ExitState(currStateParents, commonCount);

            currState = nextState;//現在状態の書き換え

            //互いに共通の祖先の状態から遷移先状態まで順に降りながらStateInを実行する。
            transitionInProcess = false;
            EnterState(nextStateParents, commonCount);
        }

        /// <summary>
        /// 親状態遷移インスタンスのリストを得る。
        /// リストの先頭から最上位の状態遷移のインスタンスが格納され、最後の要素はthisになります。
        /// </summary>
        /// <returns></returns>
        private List<FormState> GetCanonicalPathList() {
            List<FormState> list = new List<FormState>();
            list.Insert(0, this);
            FormState state = this;
            while ((state = state.ParentState) != null) {
                list.Insert(0, state);
            }
            return list;
        }

        /// <summary>
        /// 完全な状態IDを返す。
        /// </summary>
        /// <returns></returns>
        private string GetCanonicalPath() {
            string canonicalPath = "";
            FormState state = this;
            while (state != null) {
                if (state.ParentState != null) {
                    canonicalPath = "/" + state.Id + canonicalPath;
                }
                state = state.ParentState;
            }
            return canonicalPath;
        }
        
        static private int GetCommonPathCount(List<FormState> pathList1, List<FormState> pathList2) {
            int commonCount = 0;
            while (
                commonCount < pathList1.Count &&
                commonCount < pathList2.Count &&
                pathList1[commonCount] == pathList2[commonCount]) {
                commonCount++;
            }
            return commonCount;
        }
        /// <summary>
        /// State-Outの処理を実行する。
        /// </summary>
        /// <param name="statePathList"></param>
        /// <param name="commonParentCount"></param>
        private void ExitState(List<FormState> statePathList, int commonParentCount) {
            Invoke((MethodInvoker)delegate {
                debug(".OnExit() ");
                for (int i = statePathList.Count - 1; i >= commonParentCount; i--) {
                    statePathList[i].OnExit();
                    statePathList[i].OnInstallEvent(false);
                    statePathList[i].TransitionState -= new EventHandler(currentState_TransitionState);
                }
                logger.Info(" ---------------------------------------------------------");
                logger.Info(" STATE OUT:" + currState.GetCanonicalPath());
                logger.Info(" =========================================================");
                logger.Debug("");
            });
        }
        /// <summary>
        /// State-Inの処理を実行する。
        /// </summary>
        /// <param name="statePathList"></param>
        /// <param name="commonParentCount"></param>
        private void EnterState(List<FormState> statePathList, int commonParentCount) {
            Invoke((MethodInvoker)delegate {
                debug(".OnEnter() ");
                logger.Info(" =========================================================");
                logger.Info(" STATE IN:" + currState.GetCanonicalPath());
                logger.Info(" ---------------------------------------------------------");
                for (int i = commonParentCount; i < statePathList.Count; i++) {
                    statePathList[i].TransitionState += new EventHandler(currentState_TransitionState);
                    statePathList[i].OnInstallEvent(true);
                    statePathList[i].OnEnter();
                }
                form.Refresh();
            });
        }
        /// <summary>
        /// 指定された状態パスに相当する状態のインスタンスを取得する。
        /// </summary>
        /// <param name="statePathString"></param>
        /// <returns></returns>
        private FormState GetState(String statePathString) {
            FormState state = null;

            if (statePathString == FormState.NEXT_STATE_ID) {
                FormState tempState = currState;
                while (state == null) {
                    //現在の状態の次の状態を取得
                    if (tempState.ParentState == null) {
                        //親状態がない場合は状態遷移しない。
                        state = currState;
                    } else {
                        //親オブジェクトの中で現在状態の次の状態を取得
                        state = tempState.ParentState.GetChildStateByIndex(tempState.Index + 1);
                        if (state == null) {
                            //次の状態がない場合、親状態へ移動。
                            tempState = tempState.ParentState;
                        }
                    }
                }
            } else {
                statePathString += "/";
                char[] chars = statePathString.ToCharArray();
                int startIndex = 0;
                if (chars[startIndex] == '/') {
                    state = this;
                    startIndex++;
                } else {
                    state = currState;
                }
                int length = statePathString.Length;
                String pathElement = "";
                for (int i = startIndex; i < length; i++) {
                    if (chars[i] == '/') {
                        if (pathElement == "") {
                            state = null;
                            break;
                        } else if (pathElement != ".") {
                            if (pathElement == "..") {
                                state = state.ParentState;
                            } else {
                                state = state.GetChildStateById(pathElement);
                            }
                        }
                        pathElement = "";
                        if (state == null) {
                            break;
                        }
                    } else {
                        pathElement += chars[i];
                    }
                }
            }
            if (state != null) {
                //この時点での状態に子状態がある場合は最初の子状態を遷移先とする。
                state = GetFirstState(state);
                logger.Debug("GetState(\"" + statePathString + "\") 遷移先: " + state.GetCanonicalPath());
            } else {
                logger.Debug("GetState(\"" + statePathString + "\") 遷移先: null");
            }
            return state;
        }

        /// <summary>
        /// 子要素中で一番最初の状態を得る
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private FormState GetFirstState(FormState state) {
            while (state.GetChildStateCount() > 0) {
                state = state.GetChildStateByIndex(0);
            }
            return state;
        }

        /// <summary>
        /// 子の状態数を返す。
        /// </summary>
        /// <returns></returns>
        private int GetChildStateCount() {
            return stateList.Count;
        }

        /// <summary>
        /// 子状態の参照
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private FormState GetChildStateByIndex(int index) {
            return (index < stateList.Count) ? stateList[index] : null;
        }

        /// <summary>
        /// 子状態をIDで参照
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private FormState GetChildStateById(String id) {
            FormState value = null;
            return stateMap.TryGetValue(id, out value) ? value : null;
        }
        /// <summary>
        /// デバッグログ出力
        /// </summary>
        /// <param name="s"></param>
        protected void debug(string s) {
            logger.Debug(this.GetType().Name + "(#" + GetCanonicalPath() + ")." + s);
        }
        /// <summary>
        /// Infoログ出力
        /// </summary>
        /// <param name="s"></param>
        protected void info(string s) {
            logger.Info(this.GetType().Name + "(#" + GetCanonicalPath() + ")." + s);
        }
    }
}
