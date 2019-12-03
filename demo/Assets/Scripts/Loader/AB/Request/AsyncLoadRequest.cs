using System.Collections.Generic;
using UnityEngine;

namespace GEngine {
    // 同一个资源，可能在一瞬间有无数个请求
    // ①加载url，以及其所有依赖的加载
    // ②维护加载资源的回调
    class AsyncLoadRequest {
        public List<AsyncLoadCallBack> Callbacks;

        // 一个Request可能关心几个URL（因为有依赖包）
        private readonly HashSet<string> _urlRequestList;

        private readonly string _mainUrl;
        private bool _isCompleted = false;

        public AsyncLoadRequest( string mainUrl, string[] depUrl ) {
            Callbacks = new List<AsyncLoadCallBack>( );
            _urlRequestList = new HashSet<string> { mainUrl };

            this._mainUrl = mainUrl;

            if ( depUrl != null ) {
                foreach ( var one in depUrl ) {
                    if ( _urlRequestList.Contains( one ) )
                        continue;

                    _urlRequestList.Add( one );
                }
            }
        }

        public string GetUnLoadUrl( ) {

            // 取一个还没有加载的URL
            foreach ( var one in _urlRequestList ) {
                var ab = AssetBundleMgr.GetInstance( ).GetAb( one );
                if ( ab != null )
                    continue;

                return one;
            }

            return "";
        }

        // 完全加载完成
        public bool CompletedCallback( ) {
            if ( !_isCompleted )
                return false;

            foreach ( var one in Callbacks ) {
                one.LoadCompleted( AssetBundleMgr.GetInstance( ).GetAb( _mainUrl ) );
            }

            return true;
        }

        // url 加载成功之后的回调
        public void UrlLoadedCallBack( string url, AssetBundle ab ) {
            // 没有找到，一定有错
            if ( !_urlRequestList.Contains( url ) )
                throw new System.Exception( string.Format( "UrlLoadCallBack is not found url in UrlRequestList" ) );

            foreach ( var one in _urlRequestList ) {
                var abCache = AssetBundleMgr.GetInstance( ).GetAb( one );
                if ( abCache == null ) {
                    // 为 mainUrl 创建一个依赖资源的请求
                    AssetBundleMgr.GetInstance( ).CreateDependsReques( one, _mainUrl );
                    return;
                }
            }

            _isCompleted = true;
        }
    }

}