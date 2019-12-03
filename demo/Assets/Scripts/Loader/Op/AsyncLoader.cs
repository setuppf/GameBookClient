using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GEngine {
    // AsyncLoader 有三个阶段：
    // 1.加载 进度条
    // 2.加载 本次需要加载的资源以及依赖项

    enum AsyncLoaderPhase {
        None,

        BeginLoadingBar,
        LoadingBar,

        BeginLoading,
        Loading,
        Loaded,
    }

    class AsyncLoader : MonoBehaviour {

        // 定义两个场景名称（加载场景和登录场景）
        public const string SceneLoader = "Loader";

        public const string SceneLoginAbPath = "scenes/login";
        public const string SceneLoginName = "Login";
        public const int SceneLoginMapId = 1;

        private AsyncLoader _instatnce;
        private AsyncLoaderPhase _phase = AsyncLoaderPhase.BeginLoadingBar;
        private UiLoadBar _uiLoading = null; // 加载界面

        public AsyncLoader GetInstance( ) {
            return _instatnce;
        }

        private AsyncLoaderCache _cache;

        private void Awake( ) {
            _instatnce = this;
            _cache = AsyncLoaderCache.GetInstance( );

            GameLogger.GetInstance( ).Trace( "Unity:AsyncSceneLoader Awake" );

            // 通知：一个新场景开始加载了
            EventDispatcher.GetInstance( ).Broadcasting( eEventType.AsyncLoaderScene );
        }

        public void Start( ) {
            _phase = AsyncLoaderPhase.BeginLoadingBar;
        }

        public void Update( ) {
            switch ( _phase ) {
                case AsyncLoaderPhase.BeginLoadingBar:
                    UpdateBeginLoadingBar( );
                    break;
                case AsyncLoaderPhase.LoadingBar:
                    UpdateLoadingBar( );
                    break;
                case AsyncLoaderPhase.BeginLoading:
                    UpdateBeginLoading( );
                    break;
                case AsyncLoaderPhase.Loading:
                    UpdateLoading( );
                    break;
                case AsyncLoaderPhase.Loaded:
                    UpdateLoaded( );
                    break;
            }
        }

        private void UpdateBeginLoadingBar( ) {
            _uiLoading = UiMgr.GetInstance( ).OpenUi( UiType.LoadingBar ) as UiLoadBar;
            _phase = AsyncLoaderPhase.LoadingBar;
        }

        private void UpdateLoadingBar( ) {
            if ( _uiLoading.GetState( ) != UiState.Ok )
                return;

            _phase = AsyncLoaderPhase.BeginLoading;
        }

        private void UpdateBeginLoading( ) {
            foreach ( var one in _cache.Loaders ) {
                one.Start( );
            }

            _phase = AsyncLoaderPhase.Loading;
        }

        private void UpdateLoading( ) {
            // 计算当前的总的加载进度
            float progress = 0;

            foreach ( var one in _cache.Loaders ) {
                if ( !one.IsCompleted ) {
                    one.Update( );
                    progress += one.Progress;
                }
                else {
                    progress += 1;
                }
            }

            progress = progress / _cache.Loaders.Count;
            //GameLogger.GetInstance( ).Trace( string.Format( "progress:{0}", progress ) );

            // ui 刷新
            _uiLoading.Fill( progress );

            foreach ( var one in _cache.Loaders ) {
                if ( !one.IsCompleted )
                    return;
            }

            _phase = AsyncLoaderPhase.Loaded;
        }

        private void UpdateLoaded( ) {
            GameLogger.GetInstance( ).Trace( "SceneManager.Loaded Scene:{0}", _cache.GetSceneName( ) );
            SceneManager.LoadScene( _cache.GetSceneName( ) );
        }
    }

}