using UnityEngine;

namespace GEngine {
    class AsyncLoadCallBack {
        // 异步加载回调函数（上下文，加载资源）
        public delegate void CallBack( object context, AssetBundle asset );

        public AsyncLoadCallBack( CallBack fun, object contex ) {
            CallBackFun = fun;
            Contex = contex;
        }

        public CallBack CallBackFun { get; private set; }

        public object Contex { get; private set; }

        public void LoadCompleted( AssetBundle ab ) {
            if ( CallBackFun != null ) {
                CallBackFun( Contex, ab );
            }
        }
    }
}

