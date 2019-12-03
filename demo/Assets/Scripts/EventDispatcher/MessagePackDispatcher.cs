using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace GEngine {
    class MessagePackDispatcher : SingletonObject<MessagePackDispatcher> {

        public delegate void OnProtocolNoticeDelegate( Google.Protobuf.IMessage msg );
        public delegate void OnProtocolNoticeDelegateEx( int msgId, Google.Protobuf.IMessage msg );

        private readonly Dictionary<int, OnProtocolNoticeDelegate> _msgNoticeHandler = new Dictionary<int, OnProtocolNoticeDelegate>( );
        private OnProtocolNoticeDelegateEx _msgHandlerDefault;

        protected void OnDestroy( ) {
            _msgNoticeHandler.Clear( );
        }

        public void RegisterDefaultHandler( OnProtocolNoticeDelegateEx callback ) {
            _msgHandlerDefault += callback;
        }

        public void RegisterFollowPacket( int msgId, OnProtocolNoticeDelegate callback ) {
            if( !_msgNoticeHandler.ContainsKey( msgId ) )
                _msgNoticeHandler.Add( msgId, null );

            _msgNoticeHandler[msgId] += callback;
        }

        public void RemoveFollowPacket( int msgId, [CanBeNull] OnProtocolNoticeDelegate callback ) {
            if( !_msgNoticeHandler.ContainsKey( msgId ) )
                return;

            if( callback == null )
                return;

            _msgNoticeHandler[msgId] -= callback;

            if( _msgNoticeHandler[msgId] == null ) {
                _msgNoticeHandler.Remove( msgId );
            }
        }

        public void Broadcasting( int msgId, Google.Protobuf.IMessage msg ) {
            if( !_msgNoticeHandler.ContainsKey( msgId ) ) {
                _msgHandlerDefault?.Invoke( msgId, msg );
                return;
            }

            OnProtocolNoticeDelegate callback;
            if( !_msgNoticeHandler.TryGetValue( msgId, out callback ) ) {
                UnityEngine.Debug.LogFormat( "MessagePackDispatcher 分发事件出错，没有实例关心 msgId：{0}", msgId );
                return;
            }

            if( callback == null ) {
                UnityEngine.Debug.LogFormat( "MessagePackDispatcher 分发事件出错，没有实例关心 msgId：{0}", msgId );
                return;
            }

            callback( msg );
        }
    }

}