using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GEngine {
    // 加载中的状态
    enum LoadStateType : int {
        Start,
        Loding,
        Completed,
    }

// 加载中的AB信息
    class LoadingAbInfo {
        public string Url;
        public float Progress; // 当前下载进度
        public byte ErrorCount; // 下载失败次数

        public LoadStateType State; // 下载状态

        public LoadingAbInfo( string url ) {
            this.Url = url;
            this.ErrorCount = 0;
            this.Progress = 0f;
            this.State = LoadStateType.Start;
        }
    }

    class AssetBundleMgr : SingletonBehaviour<AssetBundleMgr> {
        private bool _isInited = false;
        private string _assetBundleName = "assetbundle";

        public string GetExtension( ) {
            string ext = ".android";
            if ( Application.platform == RuntimePlatform.WindowsPlayer ||
                 Application.platform == RuntimePlatform.WindowsEditor )
                ext = ".win";
            else if ( Application.platform == RuntimePlatform.Android )
                ext = ".android";

            return ext;
        }

        protected override void OnAwake( ) {
            RegisterEvent( );
        }

        #region 缓存数据

        // 一般AB数据
        private AssetBundleCacheMgr _cacheMgr = new AssetBundleCacheMgr( );

        public AssetBundle GetAb( string url ) {

            string urlExt = Path.GetExtension( url );
            if ( urlExt != GetExtension( ) ) {
                url = url + GetExtension( );
            }

            AssetBundleCache abCache = _cacheMgr.GetAssetBundle( url );
            if ( abCache == null )
                return null;

            return abCache.AB;
        }

        public void SetDontRelease( string url ) {
            string urlExt = Path.GetExtension( url );
            if ( urlExt != GetExtension( ) ) {
                url = url + GetExtension( );
            }

            AssetBundleCache abCache = _cacheMgr.GetAssetBundle( url );
            if ( abCache == null ) {
                UnityEngine.Debug.Log( string.Format( "!!!!! SetDontRelease failed. {0}", url ) );
                return;
            }

            // todo 如果有依赖项，依赖项也需要DontRelease
            abCache.SetDontRelease( );
        }

        #endregion

        #region 请求加载数据

        // 同一时间，一个资源只能有一个下载，但是对于每一个资源请求，都必须要有回应
        private readonly Dictionary<string, AsyncLoadRequest> _requests = new Dictionary<string, AsyncLoadRequest>( );

        // 等待下载
        private readonly Queue<string> _waitingQueue = new Queue<string>( );

        // 正在下载
        private readonly List<LoadingAbInfo> _loadingQueue = new List<LoadingAbInfo>( );

        public void AsyncLoad( string url, AsyncLoadCallBack.CallBack onAsyncRequestCallBack, object context ) {
            url = url + GetExtension( );
            //UnityEngine.Debug.Log( string.Format( " ## {0}", url ) );

            AssetBundleCache cab = _cacheMgr.GetAssetBundle( url );
            if ( cab == null ) {
                CreateRequest( url, onAsyncRequestCallBack, context );
                return;
            }

            // 缓存中已有AB数据，检查一下依赖项量是否完成（有可能正在加载）
            string[] dependsObj = GetAbDependencies( url );
            if ( dependsObj == null ) {
                // 没有依赖项，缓存中已有AB数据，直接回调
                if ( onAsyncRequestCallBack != null ) {
                    onAsyncRequestCallBack( context, cab.AB );
                }

                return;
            }

            // 遍历依赖项是否在缓存中
            bool isOk = true;
            foreach ( var one in dependsObj ) {
                if ( _cacheMgr.GetAssetBundle( one ) != null )
                    continue;

                isOk = false;
                break;
            }

            if ( isOk ) {
                // 缓存中有全部数据，直接回调
                if ( onAsyncRequestCallBack != null ) {
                    onAsyncRequestCallBack( context, cab.AB );
                }
                return;
            }

            // AB包不全，需要创建requset
            CreateRequest( url, onAsyncRequestCallBack, context );
        }

        private void CreateRequest( string url, AsyncLoadCallBack.CallBack onAsyncRequestCallBack, object context ) {
            // ①：先创建一个回调
            AsyncLoadCallBack callbackFun = new AsyncLoadCallBack( onAsyncRequestCallBack, context );

            // ③是否已有加载数据，已有，注册一个回调函数，等待返回
            if ( _requests.ContainsKey( url ) ) {
                _requests[url].Callbacks.Add( callbackFun );
                return;
            }

            // ②：没有加载相关数据，新建一个异步请求，注册一个回调函数        
            // url可能已存在于缓存，需要请求的是依赖项
            AsyncLoadRequest request =
                new AsyncLoadRequest( url, AssetBundleMgr.GetInstance( ).GetAbDependencies( url ) );
            request.Callbacks.Add( callbackFun );
            _requests.Add( url, request );

            AssetBundle ab = GetAb( url );
            if ( ab == null ) {
                RegisterLoadedCallBack( url, request.UrlLoadedCallBack );
            }
            else {
                // 主ab包已经加载了，需要加载的是依赖包
                string depsUrl = request.GetUnLoadUrl( );
                RegisterLoadedCallBack( depsUrl, request.UrlLoadedCallBack );
                url = depsUrl;
            }

            // ④：没有正在下载的相同url，排个队
            if ( !IsLoading( url ) ) {
                _waitingQueue.Enqueue( url );
            }
        }

        public void CreateDependsReques( string url, string mainUrl ) {
            // 注册一个回调
            AsyncLoadRequest request = _requests[mainUrl];
            if ( request == null )
                throw new Exception( "CreateDependsReques AsyncLoadRequest == null" );

            RegisterLoadedCallBack( url, request.UrlLoadedCallBack );

            // 没有正在下载的相同url，排个队
            if ( !IsLoading( url ) ) {
                _waitingQueue.Enqueue( url );
            }
        }

        private bool IsLoading( string url ) {
            foreach ( var one in _waitingQueue ) {
                if ( !one.Equals( url ) )
                    continue;

                return true;
            }

            foreach ( var one in _loadingQueue ) {
                if ( !one.Url.Equals( url ) )
                    continue;

                return true;
            }

            return false;
        }

        private float GetUrlProgress( string url ) {
            foreach ( var loadItem in _loadingQueue ) {
                if ( !loadItem.Url.Equals( url ) )
                    continue;

                return loadItem.Progress;
            }
            return 0f;
        }

        // 外部数据请求加载进度
        public float AsyncLoadProgress( string url ) {

            string urlExt = Path.GetExtension( url );
            if ( urlExt != GetExtension( ) ) {
                url = url + GetExtension( );
            }

            int total = 1; // 总资源数

            float progress = 0f;
            var ab = GetAb( url );
            if ( ab != null )
                progress += 1;

            string[] depUrl = GetAbDependencies( url );
            if ( depUrl != null ) {
                total += depUrl.Length;
                foreach ( var one in depUrl ) {
                    ab = GetAb( one );
                    if ( ab != null )
                        progress += 1;
                    else {
                        progress += GetUrlProgress( one );
                    }
                }
            }

            return progress / total;
        }

        #region 单个加载

        private const int MaxWorkingCnt = 10; // 同时下载最大数
        private const int MaxTryNum = 3; // 最多重试次数

        private void Update( ) {
            // 下载量充足，增加新的下载
            if ( _loadingQueue.Count < MaxWorkingCnt ) {
                int cnt = MaxWorkingCnt - _waitingQueue.Count;

                for ( int i = 0; i < cnt; i ++ ) {
                    if ( _waitingQueue.Count <= 0 )
                        break;

                    string url = _waitingQueue.Dequeue( );
                    _loadingQueue.Add( new LoadingAbInfo( url ) );
                }
            }

            // 检查下载状态
            for ( var index = _loadingQueue.Count - 1; index >= 0; index -- ) {
                LoadingAbInfo info = _loadingQueue[index];
                if ( info.State == LoadStateType.Loding )
                    continue;

                // 三次都没有下载下来
                if ( info.ErrorCount >= MaxTryNum ) {
                    GameLogger.GetInstance( )
                        .Output( string.Format( "!!!!! AssetBundle CreateFromWWW failed: {0}\n\t ErrorCount >= {1}",
                            info.Url, MaxTryNum ) );
                    _loadingQueue.RemoveAt( index );
                    continue;
                }

                StartCoroutine( OnAssetLoadStart( info ) );
            }

            // 检查request的完成情况
            List<string> tmpKeys = new List<string>( _requests.Keys );
            foreach ( var one in tmpKeys ) {
                AsyncLoadRequest request = _requests[one];
                if ( request.CompletedCallback( ) )
                    _requests.Remove( one );
            }
        }

        public string MakeUrl( string url ) {
            if ( string.IsNullOrEmpty( url ) )
                return string.Empty;

            string rsUrl = Path.Combine( Global.GetInstance( ).GetResPath( ), url );
            rsUrl = rsUrl.Replace( "\\", "/" );
            rsUrl = "file://" + rsUrl;

            //GameLogger.GetInstance( ).Trace( rsUrl );
            return rsUrl;
        }

        private IEnumerator OnAssetLoadStart( LoadingAbInfo info ) {

#if TRACE
            //float timeBegin = Time.realtimeSinceStartup;
            //GameLogger.GetInstance().Trace( "AssetBundle CreateFromWWW Star: {0}", info.Url );
#endif

            info.State = LoadStateType.Loding;
            yield return null;

            var url = MakeUrl( info.Url );
            WWW www = new WWW( url );
            while ( !www.isDone ) {
                info.Progress = www.progress;
                yield return null;
            }

            // 加载失败了
            if ( !string.IsNullOrEmpty( www.error ) ) {
                GameLogger.GetInstance( ).Trace( "AssetBundle CreateFromWWW failed: {0}\n\t{1}", url, www.error );
                UrlLoadCompleted( info, null );
                yield break;
            }

            AssetBundle ab = www.assetBundle;
            if ( ab == null ) {
                GameLogger.GetInstance( ).Trace( "AssetBundle CreateFromWWW failed: {0}\n\tab == null", url );
                UrlLoadCompleted( info, null );
                yield break;
            }

#if TRACE
            //GameLogger.GetInstance().Trace( "### Load {0} successful, time = {1}", info.Url, Time.realtimeSinceStartup - timeBegin );
#endif

            UrlLoadCompleted( info, ab );
        }

        // 查看是否有依赖项
        private string[] GetAbDependencies( string url ) {

            // 加载的是 AssetBundle，没有依赖相
            if ( url.Equals( _assetBundleName ) || url.Equals( _assetBundleName + GetExtension( ) ) )
                return null;

            string urlExt = Path.GetExtension( url );
            if ( urlExt != GetExtension( ) ) {
                url = url + GetExtension( );
            }

            AssetBundleCache cb = this._cacheMgr.GetAssetBundle( _assetBundleName + GetExtension( ) );
            if ( cb == null ) {
                throw new Exception( " Get AssetBundle is null" );
            }

            AssetBundleManifest manifest = cb.AB.LoadAsset( "AssetBundleManifest" ) as AssetBundleManifest;
            if ( manifest == null ) {
                throw new Exception( " Get AssetBundleManifest is null" );
            }

            return manifest.GetAllDependencies( url );
        }

        private void UrlLoadCompleted( LoadingAbInfo info, AssetBundle ab ) {

            if ( ab == null ) {
                if ( info.ErrorCount >= MaxTryNum ) { }
                else {
                    // 再试几次下载
                    info.State = LoadStateType.Start;
                    info.ErrorCount += 1;
                }
                //GameLogger.GetInstance( ).Debug( "!!!!! Failed..UrlLoadCompleted:" + info.Url );
                return;
            }

            GameLogger.GetInstance( ).Debug( "UrlLoadCompleted:" + info.Url );

            // 缓存起来
            _cacheMgr.Add( info.Url, ab );

            // 修改下载数据状态
            info.State = LoadStateType.Completed;

            // 通知Request，有一个AB下载完成
            if ( !_loadedCallback.ContainsKey( info.Url ) )
                throw new Exception($"!!!UrlLoadCompleted failed. Url:{info.Url}");

            // 回调
            _loadedCallback[info.Url].DynamicInvoke( info.Url, ab );
            _loadedCallback.Remove( info.Url );

            // 加载完成
            _loadingQueue.Remove( info );
        }

        #endregion

        #region 加载后回调

        public delegate void UrlLoadCallBack( string url, AssetBundle ab );

        private Dictionary<string, Delegate> _loadedCallback = new Dictionary<string, Delegate>( );

        private void RegisterLoadedCallBack( string url, UrlLoadCallBack handler ) {
            if ( !_loadedCallback.ContainsKey( url ) ) {
                _loadedCallback.Add( url, null );
            }

            _loadedCallback[url] = (UrlLoadCallBack)_loadedCallback[url] + handler;
        }

        #endregion

        #endregion

        #region 事件

        public void Init( ) {
            _isInited = false;
            CreateRequest( _assetBundleName + GetExtension( ), AsyncLoadCallBack, null );
        }

        public bool IsInited( ) {
            return _isInited;
        }

        private void AsyncLoadCallBack( object context, AssetBundle ab ) {
            _isInited = true;
        }

        private void OnApplicationQuit( ) {
            //EventAsyncLoaderScene( );
        }

        private void RegisterEvent( ) {
            //EventDispatcher.GetInstance( ).RegisterEvent( eEventType.AsyncLoaderScene, EventAsyncLoaderScene );
        }

        // todo 时机有问题
        private void EventAsyncLoaderScene( ) {
            //_cacheMgr.UnLoadAll( );
            //Resources.UnloadUnusedAssets( );
            //System.GC.Collect( );
        }

        #endregion

    }

}