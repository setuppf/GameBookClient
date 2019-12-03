using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GEngine
{

    enum ReadyToLoadStateType
    {
        Init1, // 准备AssetBundle
        Init2, // 准备界面的背景，否则加载的界面没有Canvas
        LoadScene,
        Ok
    }

    public class ReadyToLoad : MonoBehaviour
    {
        public string ResPath;
        public string ReferencePath;
        public string ServerIp = "127.0.0.1";
        public int ServerPort = 7071;

        private ReadyToLoadStateType _state = ReadyToLoadStateType.Init1;

        void Awake()
        {

            // 创建EventSystem（UI事件）
            var go = new GameObject("EventSystem");
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            var inputMofule = go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            inputMofule.forceModuleActive = true;
            DontDestroyOnLoad(go);

            // 事件分发
            go = new GameObject("EventDispatcher");
            go.AddComponent<EventDispatcher>();
            DontDestroyOnLoad(go);

            // 资源管理
            go = new GameObject("AssetBundleMgr");
            go.AddComponent<AssetBundleMgr>();
            DontDestroyOnLoad(go);

            // 协程加载
            go = new GameObject("CoroutineEngine");
            go.AddComponent<CoroutineEngine>();
            DontDestroyOnLoad(go);

            // 网络
            go = new GameObject("NetworkMgr");
            go.AddComponent<NetworkMgr>();
            DontDestroyOnLoad(go);

            // GameMain
            go = new GameObject("GameMain");
            go.AddComponent<GameMain>();
            DontDestroyOnLoad(go);

            go = new GameObject("UiMgr");
            go.AddComponent<UiMgr>();
            DontDestroyOnLoad(go);
        }

        void Start()
        {
            Global.GetInstance().SetResPath(ResPath);
            Global.GetInstance().SetReferencePath(ReferencePath);
            Global.GetInstance().SetServer(ServerIp, ServerPort);

            AssetBundleMgr.GetInstance().Init();
            _state = ReadyToLoadStateType.Init1;
        }

        public void Update()
        {
            switch (_state)
            {
                case ReadyToLoadStateType.Init1:
                    {
                        if (!AssetBundleMgr.GetInstance().IsInited())
                            break;

                        UiMgr.GetInstance().Init();
                        _state = ReadyToLoadStateType.Init2;
                        break;
                    }
                case ReadyToLoadStateType.Init2:
                    {
                        if (!UiMgr.GetInstance().IsInited())
                            break;

                        _state = ReadyToLoadStateType.LoadScene;
                        break;
                    }
                case ReadyToLoadStateType.LoadScene:
                    {

                        // 初始化加载，下面二行可以优化成加载类
                        GameMain.GetInstance().Init();

                        // 设置 加载场景需要加载的数据
                        AsyncLoaderCache cache = AsyncLoaderCache.GetInstance();
                        cache.Loaders.Clear();

                        AsyncLoaderScene ssloader = new AsyncLoaderScene(AsyncLoader.SceneLoginAbPath, AsyncLoader.SceneLoginName, AsyncLoader.SceneLoginMapId);
                        cache.Loaders.Add(ssloader);

                        //AsyncLoaderTest testLoader = new AsyncLoaderTest( );
                        //cache.Loaders.Add( testLoader );

                        // 准备开始游戏了，加载“加载场景”
                        SceneManager.LoadScene(AsyncLoader.SceneLoader);

                        _state = ReadyToLoadStateType.Ok;
                        break;
                    }

                case ReadyToLoadStateType.Ok:
                    break;
            }
        }
    }

}