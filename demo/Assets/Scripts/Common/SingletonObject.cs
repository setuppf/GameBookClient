using System.Threading;

namespace GEngine {
    public class SingletonObject<T> where T : class, new() {
        private static T _singleton;

        public static T GetInstance( ) {
            if( _singleton == null ) {
                _singleton = new T( );
#if !Editor
                UnityEngine.Debug.LogFormat( "Thread Hash:{0} SingletonObject<T> Awake = {1}", Thread.CurrentThread.GetHashCode( ), _singleton.GetType( ).ToString( ) );
#endif
            }

            return _singleton;
        }

        public static bool IsInstance( ) {
            return _singleton != null ? true : false;
        }
    }
}