using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace GEngine
{
    partial class UiMgr : SingletonBehaviour<UiMgr>
    {

        // 当前窗口实例
        private readonly Dictionary<UiComplexType, UiBase> _uiInses = new Dictionary<UiComplexType, UiBase>();
        private readonly HashSet<UiComplexType> _removeObjs = new HashSet<UiComplexType>();
        private readonly Dictionary<UiComplexType, UiBase> _addObjs = new Dictionary<UiComplexType, UiBase>();

        // 初始加载的基本资源
        private readonly List<string> _baesUrl = new List<string>();

        //private const string _urlCanvasBg = "ui/canvas-bg";
        private const string _urlCanvas = "ui/canvas";

        protected override void OnAwake()
        {
            //_baesUrl.Add(_urlCanvasBg);
            _baesUrl.Add(_urlCanvas);
            RegisterEvent();
        }

        public bool MouseInGui()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.pressPosition = Input.mousePosition;
            eventData.position = Input.mousePosition;

            List<RaycastResult> list = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, list);

            return list.Count > 0;
        }

        public void Update()
        {
            foreach (var key in _removeObjs)
            {
                UiBase uibase = _uiInses[key];
                uibase.Destroy();
                _uiInses.Remove(key);
            }
            _removeObjs.Clear();

            foreach (var one in _addObjs.Values)
            {
                _uiInses.Add(one.GetUiComplexType(), one);

                // 开始资源加载
                GameLogger.GetInstance().Trace("-- ui open:" + one.GetType());
                var uiAb = UiFactory.GetInstance().GetAssetBundle(one.GetUiType());
                if (uiAb != null)
                {
                    AssetBundleMgr.GetInstance().AsyncLoad(uiAb, AsyncLoadCallBack, one.GetUiComplexType());
                }
                else
                {
                    // 空界面，做逻辑切换用的，不需要gameobject表现
                    one.AttachGameObject(null);
                }
            }

            _addObjs.Clear();
            foreach (var one in _uiInses)
            {
                one.Value.Update();
            }
        }

        public void Init()
        {
            // 加载 _canvas
            foreach (var one in _baesUrl)
            {
                AssetBundleMgr.GetInstance().AsyncLoad(one, AsyncLoadCallBackCanvasBg, one);
            }
        }

        public bool IsInited()
        {

            foreach (var one in _baesUrl)
            {
                if (AssetBundleMgr.GetInstance().GetAb(one) == null)
                    return false;
            }

            return true;
        }

        void RegisterEvent()
        {
            EventDispatcher eventDis = EventDispatcher.GetInstance();

            eventDis.RegisterEvent(eEventType.SceneLoadStart, EventSceneLoadStart);
            eventDis.RegisterEvent<int>(eEventType.SceneLoadCompleted, EventSceneLoadCompleted);
            eventDis.RegisterEvent<AppType>(eEventType.Disconnect, EventNetworkDisconnect);
        }

        private void EventNetworkDisconnect(AppType appType)
        {
            if (appType == AppType.Login)
                return;

            CloseModalBox0();
            OpenModalBox1("网络消息", $"[{appType}]网络断开，请重新登录...", delegate ()
            {
                UiMgr.GetInstance().CloseAll();
                UiMgr.GetInstance().OpenUi(UiType.Login);
            });
        }

        private void EventSceneLoadStart()
        {
            CloseAll();
        }

        private void EventSceneLoadCompleted(int mapId)
        {
            // 每个场景都会有默认的界面显示

            // 默认打开的界面
            // Loading 场景，这里mapid == 0，不处理
            if (mapId <= 0)
                return;

            ResourceWorld refMap = ResourceAll.GetInstance().MapMgr.GetReference(mapId);
            if (refMap == null || refMap.UiRes == (int)UiType.None)
                return;

            UiType uiType = Util.ChangeToEnum<UiType>(refMap.UiRes);
            if (uiType != UiType.None)
            {
                OpenUi(uiType);
            }
        }

        public ModalBox0 OpenModalBox0(string title, string tips)
        {
            ModalBox0 modal = OpenUi(UiType.ModalBox0) as ModalBox0;
            if (modal == null)
            {
                GameLogger.GetInstance().Debug(" !!!!! create modal0 is failed.");
                return null;
            }

            modal.Title = title;
            modal.Tip = tips;
            return modal;
        }

        public void OpenModalBox1(string title, string tips, Action closeAction)
        {
            ModalBox1 modal = OpenUi(UiType.ModalBox1) as ModalBox1;
            if (modal == null)
            {
                GameLogger.GetInstance().Debug(" !!!!! create modal1 is failed.");
                return;
            }

            modal.Tip = tips;
            modal.Title = title;
            modal.CloseAction = closeAction;
        }

        public void CloseModalBox0()
        {
            CloseUi(UiType.ModalBox0);
        }

        public UiBase OpenUi(UiType uiType)
        {
            return OpenUi(uiType, 0);
        }

        public UiBase OpenUi(UiType uiType, ulong sn)
        {
            UiComplexType key = new UiComplexType(uiType, sn);
            if (_uiInses.ContainsKey(key))
            {
                _uiInses[key].Show();
                return _uiInses[key];
            }

            if (_addObjs.ContainsKey(key))
            {
                return _addObjs[key];
            }

            UiBase uiIns = UiFactory.GetInstance().Create(uiType, sn);
            if (uiIns == null)
            {
                GameLogger.GetInstance().Output("!!!! OpenUi Error. Create ui == null. uiType:" + uiType);
                return null;
            }

            _addObjs.Add(uiIns.GetUiComplexType(), uiIns);
            return uiIns;
        }

        public void CloseAll()
        {
            foreach (var one in _uiInses.Values)
            {
                _removeObjs.Add(one.GetUiComplexType());
            }

            GameLogger.GetInstance().Trace("-- ui close all");
        }

        public void CloseUi(UiType uiType)
        {
            CloseUi(uiType, 0);
        }

        public void CloseUi(UiType uiType, ulong sn)
        {
            UiComplexType key = new UiComplexType(uiType, sn);

            // 处理删除的UI
            if (!_uiInses.ContainsKey(key))
                return;

            GameLogger.GetInstance().Trace("-- ui remove:" + _uiInses[key].GetType());
            _removeObjs.Add(key);
        }

        #region callback

        private void AsyncLoadCallBackCanvasBg(object context, AssetBundle ab)
        {
            if (context == null)
                return;

            string url = context as string;
            AssetBundleMgr.GetInstance().SetDontRelease(url);
        }

        private void AsyncLoadCallBack(object context, AssetBundle ab)
        {
            UiComplexType key = (UiComplexType)context;

            UiBase uiObj = null;
            if (_uiInses.ContainsKey(key))
                uiObj = _uiInses[key];

            if (uiObj == null && _addObjs.ContainsKey(key))
                uiObj = _addObjs[key];

            // 回调的时候，界面已经没有了
            if (uiObj == null)
                return;

            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {

                string canvasUrl = _urlCanvas;
                //if (key.UiType == UiType.LoadingBar || key.UiType == UiType.Login)
                //{
                //    // 登录和加载界面的背影略有不同
                //    canvasUrl = _urlCanvasBg;
                //}

                AssetBundle abBg = AssetBundleMgr.GetInstance().GetAb(canvasUrl);
                if (abBg == null)
                {
                    GameLogger.GetInstance().Debug($"!!!!!GetAb failed. {canvasUrl}");
                    return;
                }

                GameObject canvasObj = MonoBehaviour.Instantiate(abBg.LoadAsset(abBg.GetAllAssetNames()[0])) as GameObject;
                if (canvasObj == null)
                    return;

                canvasObj.transform.Rotate(0, 0, 0);
                canvas = canvasObj.GetComponent<Canvas>();
            }

            // 对像挂在 canvas 之下
            GameObject obj =
                MonoBehaviour.Instantiate(ab.LoadAsset(ab.GetAllAssetNames()[0]), canvas.transform) as GameObject;
            if (obj == null)
                return;

            obj.transform.Rotate(0, 0, 0);
            //obj.transform.localScale = new Vector3( 1, 1, 1 );
            uiObj.AttachGameObject(obj);
        }

        #endregion

    }

}