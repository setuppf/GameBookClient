using System.Threading;
using UnityEngine;

namespace GEngine {
    // 继承了SingletonBehaviour的类，最好都写个 OnApplicationQuit
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour {
        protected static T _singleton;

        public void Awake( ) {
            _singleton = (T)(MonoBehaviour)this;
            UnityEngine.Debug.LogFormat( "Thread Hash:{0} SingletonBehaviour<T> Awake = {1}",
                Thread.CurrentThread.GetHashCode( ), _singleton.GetType( ).ToString( ) );

            OnAwake( );
        }

        public static T GetInstance( ) {
            return _singleton;
        }

        protected abstract void OnAwake( );
    }
}

