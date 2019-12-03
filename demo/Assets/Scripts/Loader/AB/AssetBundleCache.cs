using System;
using System.Collections.Generic;
using UnityEngine;

namespace GEngine {
    // 用于AB文件内容的缓存
    class AssetBundleCache : IDisposable, IComparable<AssetBundleCache> {

        private AssetBundle _ab = null;
        private float _updateTime;
        private bool _dontRelease; // 不释放资源

        public AssetBundleCache( AssetBundle value ) {
            this._ab = value;
            this._updateTime = Time.realtimeSinceStartup;
            this._dontRelease = false;
        }

        public void SetDontRelease( ) {
            _dontRelease = true;
        }

        public bool DontRelease( ) {
            return _dontRelease;
        }

        public AssetBundle AB => _ab;

        // 更新使用时间，用使用时间来决定删除时间
        public void UpdateTime( ) {
            this._updateTime = Time.realtimeSinceStartup;
        }

        public int CompareTo( AssetBundleCache obj ) {
            return _updateTime.CompareTo( obj._updateTime );
        }

        public void Dispose( ) {
            _ab.Unload( true );
            _ab = null;
        }
    }

    class AssetBundleCacheMgr {

        // url , AssetBundleCache
        private readonly Dictionary<string, AssetBundleCache> _list;

        private int _cache = 0;

        public AssetBundleCacheMgr( int cacheCount = 0 ) {
            this._list = new Dictionary<string, AssetBundleCache>( cacheCount );
            this._cache = cacheCount;
        }

        public void Add( string url, AssetBundle ab ) {
            _list.Add( url, new AssetBundleCache( ab ) );

            // 根据UpdateTime，把最高的一个取掉
            if ( _list.Count >= _cache && _cache > 0 ) {
                var tmpList = new List<KeyValuePair<string, AssetBundleCache>>( _list );
                tmpList.Sort( ( s1, s2 ) => s2.Value.CompareTo( s1.Value ) );

                foreach ( var one in tmpList ) {
                    if ( !one.Value.DontRelease( ) ) {
                        one.Value.Dispose( );
                        _list.Remove( one.Key );
                        break;
                    }
                }

                Resources.UnloadUnusedAssets( );
            }
        }

        public AssetBundleCache GetAssetBundle( string url ) {
            if ( !_list.ContainsKey( url ) )
                return null;

            return _list[url];
        }

        public void UnLoadAll( ) {
            var tmp = new List<string>( _list.Keys );

            foreach ( var key in tmp ) {
                var one = _list[key];

                // 不释放
                if ( one.DontRelease( ) )
                    continue;

                one.Dispose( );
                _list.Remove( key );
            }
        }
    }

}