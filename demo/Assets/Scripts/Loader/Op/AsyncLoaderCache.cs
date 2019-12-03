
using System.Collections.Generic;

namespace GEngine {
    // 一次加载，可能是多个资源
    // 例如，同时加载配置文件和场景
    class AsyncLoaderCache : SingletonObject<AsyncLoaderCache> {
        public List<AsyncLoaderProgress> Loaders;

        public AsyncLoaderCache( ) {
            Loaders = new List<AsyncLoaderProgress>( );
        }

        public string GetSceneName( ) {
            foreach( var one in Loaders ) {
                var scene = one as AsyncLoaderScene;
                if( scene != null ) {
                    return scene.SceneName;
                }
            }

            return "";
        }

        public int GetMapId( ) {
            foreach( var one in Loaders ) {
                var scene = one as AsyncLoaderScene;
                if( scene != null ) {
                    return scene.MapId;
                }
            }

            return 0;
        }
    }
}