using System;
using System.Collections.Generic;

namespace GEngine {

    // 客户端内部事件
    enum eEventType : int {
        AsyncLoaderScene = 0, // 一个新的场景加载发生了，通知相关类，清空对象等等
        SceneLoadStart,     // 场景开始加载
        SceneLoadCompleted, // 场景加载完成

        Connectting, // 网络正在链接
        Disconnect, // 网络断开
        Connected, // 网络链接成功
    }

    public class EventDispatcherException : Exception {
        public EventDispatcherException( string msg ) : base( msg ) { }
    }

    class EventDispatcher : SingletonBehaviour<EventDispatcher> {

        private Dictionary<eEventType, Delegate> _eventTable = new Dictionary<eEventType, Delegate>( );

        #region 注册事件、删除事件

        public delegate void EventCallback( );

        public delegate void EventCallback<T>( T arg1 );

        public delegate void EventCallback<T, U>( T arg1, U arg2 );
        //public delegate void EventCallback<T, U, V>( T arg1, U arg2, V arg3 );

        private void CheckRegisterEvent( eEventType eType, Delegate handler ) {
            if( !_eventTable.ContainsKey( eType ) )
                return;

            Delegate d = _eventTable[eType];
            if( d != null && d.GetType( ) != handler.GetType( ) ) {
                throw new EventDispatcherException( string.Format( "注册一个消息事件出错，类型：{0}，原类型：{1}，新增类型：{2}", eType,
                    d.GetType( ).Name, handler.GetType( ).Name ) );
            }
        }

        public void RegisterEvent( eEventType eType, EventCallback handler ) {
            if( !_eventTable.ContainsKey( eType ) ) {
                _eventTable.Add( eType, null );
            }

            CheckRegisterEvent( eType, handler );

            _eventTable[eType] = (EventCallback)_eventTable[eType] + handler;
        }

        public void RegisterEvent<T>( eEventType eType, EventCallback<T> handler ) {
            if( !_eventTable.ContainsKey( eType ) ) {
                _eventTable.Add( eType, null );
            }

            CheckRegisterEvent( eType, handler );

            _eventTable[eType] = (EventCallback<T>)_eventTable[eType] + handler;
        }

        public void RegisterEvent<T, U>( eEventType eType, EventCallback<T, U> handler ) {
            if( !_eventTable.ContainsKey( eType ) ) {
                _eventTable.Add( eType, null );
            }

            CheckRegisterEvent( eType, handler );

            _eventTable[eType] = (EventCallback<T, U>)_eventTable[eType] + handler;

        }

        private void CheckRemoveEvent( eEventType eType, Delegate handler ) {
            if( !_eventTable.ContainsKey( eType ) )
                throw new EventDispatcherException( string.Format( "删除一个消息事件出错，类型：{0}，没找到该消息", eType ) );

            Delegate d = _eventTable[eType];
            if( d == null ) {
                throw new EventDispatcherException( string.Format( "删除一个消息事件出错，类型：{0}，没找到该消息", eType ) );
            } else if( d.GetType( ) != handler.GetType( ) ) {
                throw new EventDispatcherException( string.Format( "删除一个消息事件出错，类型：{0}，原类型：{1}，删除类型：{2}", eType,
                    d.GetType( ).Name, handler.GetType( ).Name ) );
            }
        }

        public void RemoveEvent( eEventType eType, EventCallback handler ) {

            CheckRemoveEvent( eType, handler );

            _eventTable[eType] = (EventCallback)_eventTable[eType] - handler;
            if( _eventTable[eType] == null ) {
                _eventTable.Remove( eType );
            }

        }

        public void RemoveEvent<T>( eEventType eType, EventCallback<T> handler ) {

            CheckRemoveEvent( eType, handler );

            _eventTable[eType] = (EventCallback<T>)_eventTable[eType] - handler;
            if( _eventTable[eType] == null ) {
                _eventTable.Remove( eType );
            }

        }

        public void RemoveEvent<T, U>( eEventType eType, EventCallback<T, U> handler ) {
            CheckRemoveEvent( eType, handler );

            _eventTable[eType] = (EventCallback<T, U>)_eventTable[eType] - handler;
            if( _eventTable[eType] == null ) {
                _eventTable.Remove( eType );
            }
        }

        #endregion

        public void Broadcasting( eEventType eType ) {
            _broadcasting.Add( new BroadcastInfo( ) { eType = eType, obj1 = null, obj2 = null } );
        }

        public void Broadcasting<T>( eEventType eType, T arg1 ) {
            _broadcasting.Add( new BroadcastInfo( ) { eType = eType, obj1 = arg1, obj2 = null } );
        }

        public void Broadcasting<T, U>( eEventType eType, T arg1, U arg2 ) {
            _broadcasting.Add( new BroadcastInfo( ) { eType = eType, obj1 = arg1, obj2 = arg2 } );
        }

        struct BroadcastInfo {
            public eEventType eType;
            public Object obj1;
            public Object obj2;
        }

        private List<BroadcastInfo> _broadcasting = new List<BroadcastInfo>( );

        protected override void OnAwake( ) { }

        protected void OnDestroy( ) {
            _eventTable.Clear( );
        }

        private void Update( ) {
            // 事件调用的时候，可能产生新的事件
            List<BroadcastInfo> temp = new List<BroadcastInfo>( );
            temp.AddRange( _broadcasting );
            _broadcasting.Clear( );

            foreach( BroadcastInfo one in temp ) {
                if( !_eventTable.ContainsKey( one.eType ) )
                    continue;

                Delegate handler;
                if( _eventTable.TryGetValue( one.eType, out handler ) ) {
                    if( one.obj1 == null && one.obj2 == null ) {
                        handler.DynamicInvoke( );
                        continue;
                    }

                    if( one.obj2 == null ) {
                        handler.DynamicInvoke( one.obj1 );
                    } else {
                        handler.DynamicInvoke( one.obj1, one.obj2 );
                    }
                }
            }

            _broadcasting.Clear( );
        }
    }

}