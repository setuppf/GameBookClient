using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GEngine {
    abstract class AsyncLoaderProgress {
        public string Name;
        public float Progress; // 当前资源当前下载进度

        public bool IsCompleted; // 是否加载完成

        public abstract void Start( );
        public abstract void Update( );
    }

    // 测试进度条的加载，每秒1格，10秒加载完成
    class AsyncLoaderTest : AsyncLoaderProgress {
        public override void Start( ) {
            _curTimeout = 10;
            CoroutineEngine.GetInstance( ).Execute( TimerChange( ) );
        }

        public override void Update( ) {
            IsCompleted = Progress > 0.999f;
        }

        private int _curTimeout;

        private IEnumerator TimerChange( ) {
            if ( _curTimeout > 0 ) {
                yield return new WaitForSeconds( 1.0f );

                Progress = Progress + 0.1f;
                _curTimeout = _curTimeout - 1;
                CoroutineEngine.GetInstance( ).Execute( TimerChange( ) );
            }
        }
    }

    class AsyncLoaderScene : AsyncLoaderProgress {
        public readonly string AbPath;      // 资源包的名字
        public readonly string SceneName;   // 资源包中地图的名字
        public int MapId { get; set; }

        public AsyncLoaderScene( string abPath, string sceneName, int mapId ) {
            this.Name = "Loading Scene";

            this.AbPath = abPath;
            this.SceneName = sceneName;
            this.MapId = mapId;

            this.Progress = 0;
            this.IsCompleted = false;
        }

        public override void Start( ) {
            GameLogger.GetInstance( ).Trace( $"Try download: {SceneName}, path: {AbPath}" );
            AssetBundleMgr.GetInstance( ).AsyncLoad( AbPath, AsyncLoadCallBack, null );
        }

        private void AsyncLoadCallBack( object context, AssetBundle asset ) {
            IsCompleted = true;
        }

        public override void Update( ) {
            Progress = AssetBundleMgr.GetInstance( ).AsyncLoadProgress( AbPath );
        }
    }
}