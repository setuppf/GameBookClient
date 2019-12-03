using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GEngine
{
    struct CacheMsg
    {
        public int MsgId;
        public Google.Protobuf.IMessage Msg;
    }

    class GameMain : SingletonBehaviour<GameMain>
    {

        private AccountInfo _accountInfo = null;
        private World _currentWorld;
        public World CurrentWorld => _currentWorld;

        private Player _mainPlayer = null;
        public Player MainPlayer => _mainPlayer;

        private bool _isLoadedMap = false;
        private readonly List<CacheMsg> _msgCache = new List<CacheMsg>();

        protected override void OnAwake()
        {
            var gesture = gameObject.AddComponent<Gesture>();
            gesture.GameMain = this;
        }

        public void Init()
        {
            ResourceAll.GetInstance().Init();
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("OnSceneLoaded: " + scene.name + " LoadSceneMode:" + mode);

            // AsyncLoaderScene
            if (scene.name.Equals(AsyncLoader.SceneLoader))
            {
                var scrpts = Camera.main.gameObject.GetComponent<AsyncLoader>();
                if (scrpts == null)
                {
                    Camera.main.gameObject.AddComponent<AsyncLoader>();
                }
                return;
            }

            _isLoadedMap = true;
            int mapId = AsyncLoaderCache.GetInstance().GetMapId();
            if (mapId <= 0)
                return;

            _currentWorld = new World(ResourceAll.GetInstance().MapMgr.GetReference(mapId));

            // 触发一个场景加载完毕的事件
            EventDispatcher.GetInstance().Broadcasting<int>(eEventType.SceneLoadCompleted, mapId);

            // 初始化角色位置
            if (_mainPlayer != null && Camera.main.gameObject != null)
            {
                // 增加一个相机跟随组件
                CameraFollowBehaviour cTrack = Camera.main.gameObject.GetComponent<CameraFollowBehaviour>();

                if (cTrack == null)
                    cTrack = Camera.main.gameObject.AddComponent<CameraFollowBehaviour>();

                if (_mainPlayer.GetGameObject() != null)
                    cTrack.Target = _mainPlayer.GetGameObject().GetComponent<Transform>();
            }

            // 场景加载完成之后，处理缓存的协议
            HandlerCacheMsg();
        }

        void Start()
        {
            MessagePackDispatcher msgDispatcher = MessagePackDispatcher.GetInstance();
            msgDispatcher.RegisterDefaultHandler(MsgDefaultHandler);

            msgDispatcher.RegisterFollowPacket((int)Proto.MsgId.L2CPlayerList, MsgPlayerList);
            msgDispatcher.RegisterFollowPacket((int)Proto.MsgId.G2CSyncPlayer, MsgPlayer);
            msgDispatcher.RegisterFollowPacket((int)Proto.MsgId.S2CEnterWorld, MsgEnterWorld);
        }

        void OnDestroy()
        {
            MessagePackDispatcher msgDispatcher = MessagePackDispatcher.GetInstance();
            if (msgDispatcher != null)
            {
                msgDispatcher.RemoveFollowPacket((int)Proto.MsgId.L2CPlayerList, MsgPlayerList);
                msgDispatcher.RemoveFollowPacket((int)Proto.MsgId.G2CSyncPlayer, MsgPlayer);
                msgDispatcher.RemoveFollowPacket((int)Proto.MsgId.S2CEnterWorld, MsgEnterWorld);
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnUpdate()
        {

        }

        public void LoadScene(string sceneAbPath, string sceneResName, int mapId)
        {
            // 通知，有新的场景需要加载了
            EventDispatcher.GetInstance().Broadcasting(eEventType.SceneLoadStart);

            // 设置 加载场景需要加载的数据
            AsyncLoaderCache cache = AsyncLoaderCache.GetInstance();
            cache.Loaders.Clear();

            AsyncLoaderScene ssloader = new AsyncLoaderScene(sceneAbPath, sceneResName, mapId);
            cache.Loaders.Add(ssloader);

            // 加载“加载场景”
            _isLoadedMap = false;
            SceneManager.LoadScene(AsyncLoader.SceneLoader);
        }

        #region 事件处理

        #endregion

        #region 处理协议

        private void HandlerCacheMsg()
        {
            while (_msgCache.Count > 0)
            {
                CacheMsg cmsg = _msgCache[0];
                _currentWorld.HanderMsg(cmsg.MsgId, cmsg.Msg);
                _msgCache.RemoveAt(0);
            }
        }

        private void MsgDefaultHandler(int msgId, Google.Protobuf.IMessage msg)
        {
            if (!_isLoadedMap)
            {
                _msgCache.Add(new CacheMsg() { Msg = msg, MsgId = msgId });
            }
            else
            {
                _currentWorld.HanderMsg(msgId, msg);
            }
        }

        private void MsgPlayerList(Google.Protobuf.IMessage msg)
        {
            Proto.PlayerList protoRs = msg as Proto.PlayerList;
            if (protoRs == null)
                return;

            if (_accountInfo == null)
                _accountInfo = new AccountInfo();

            _accountInfo.Parse(protoRs);
        }

        private void MsgPlayer(Google.Protobuf.IMessage msg)
        {
            Proto.SyncPlayer proto = msg as Proto.SyncPlayer;
            if (proto == null)
                return;

            GameLogger.GetInstance().Debug($"sync player sn:{proto.Player.Sn}");

            if (_mainPlayer == null)
                _mainPlayer = new Player();

            _mainPlayer.Parse(proto.Player);
        }

        private void MsgEnterWorld(Google.Protobuf.IMessage msg)
        {
            Proto.EnterWorld protoEnter = msg as Proto.EnterWorld;
            if (protoEnter == null)
                return;

            GameLogger.GetInstance().Debug($"Enter world. world id:{protoEnter.WorldId}");
            ResourceWorld refMap = ResourceAll.GetInstance().MapMgr.GetReference((int)protoEnter.WorldId);
            if (refMap == null)
                return;

            LoadScene(refMap.AbPath, refMap.ResName, (int)protoEnter.WorldId);
        }
        #endregion
    }
}

