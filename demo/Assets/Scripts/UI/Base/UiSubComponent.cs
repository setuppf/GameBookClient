using System;
using UnityEngine;
using UnityEngine.UI;

namespace GEngine {
    abstract class UiSubComponent : IDisposable {
        protected UiBase _parentUi;
        protected GameObject _gameObj;

        protected UiSubComponent( UiBase uiBase, GameObject obj ) {
            _parentUi = uiBase;
            _gameObj = obj;
        }

        public T GetUi<T>( string name = null ) {
            Transform tf = UiUtil.GetTransform( _gameObj, name );
            if ( tf == null )
                throw new Exception( $"Get failed; name:{name}" );
            return tf.GetComponent<T>( );
        }

        public abstract void Update( ToUiData data );

        public void Dispose( ) {
            if ( _gameObj != null )
                UnityEngine.Object.Destroy( _gameObj );
        }
    }
}
