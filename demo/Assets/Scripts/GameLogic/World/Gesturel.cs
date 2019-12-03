using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GEngine;
using UnityEngine;

namespace GEngine {

    public enum eGestureState {
        None,
        Down,
        Up,
        HoldDown, // 持续按下
        SwipeStart, // 滑动开始
        SwipeEnd, // 滑动结束
    }

    class Gesture : MonoBehaviour {

        public GameMain GameMain;
        Vector3 m_downPos;
        Vector3 m_upPos;

        float m_fUpTime;
        float m_fDownTime;

        eGestureState m_mouseState;

        public void Update( ) {

            if( Input.GetKeyDown( KeyCode.Mouse0 ) ) {
                if( !UiMgr.GetInstance( ).MouseInGui( ) ) {
                    m_fDownTime = Time.time;
                    m_downPos = Input.mousePosition;
                    m_mouseState = eGestureState.Down;
                } else {
                    //GameLogger.GetInstance().Trace( string.Format( "{0} MouseInGUI", Time.realtimeSinceStartup ) );
                }
            }

            if( Input.GetKeyUp( KeyCode.Mouse0 ) ) {

                // GameLogger.GetInstance().Trace( string.Format( "{0} m_mouseState:{1}", Time.realtimeSinceStartup, m_mouseState ) );

                m_upPos = Input.mousePosition;

                float distance = Vector3.Distance( m_upPos, m_downPos );
                switch( m_mouseState ) {
                    case eGestureState.Down: {
                        if( distance > 50f ) {
                            UpdateSwipeEnd( ); // 滑动判断
                        } else {
                            float intervalTime = 0.2f;
                            if( Time.time - m_fUpTime < intervalTime && distance < 10f ) {
                                UpdateDoubleClick( );
                            } else {
                                UpdateClick( );
                            }
                        }
                    }
                    break;
                }

                m_fUpTime = Time.time;
                m_mouseState = eGestureState.Up;
            }
        }

        private void UpdateDoubleClick( ) {
            //GameLogger.GetInstance().Trace( string.Format( "#### PlayerGestureControl. UpdateDoubleClick. {0}", Time.realtimeSinceStartup ) );
        }

        private void UpdateClick( ) {

            //GameLogger.GetInstance().Trace( string.Format( "#### PlayerGestureControl. UpdateClick. {0}", Time.realtimeSinceStartup ) );
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
            if( Physics.Raycast( ray, out hit ) ) {
                if( hit.collider is TerrainCollider ) {
                    GameMain.MainPlayer.MoveTo( hit.point );
                } else if( hit.collider is CharacterController ) {
                    GameMain.CurrentWorld.SetSelectObj( hit.collider.gameObject );
                }
            }
        }

        private void UpdateSwipeEnd( ) {
            //GameLogger.GetInstance()
            //    .Trace( string.Format( "#### PlayerGestureControl. UpdateSwipeEnd. {0} m_upPos:{1} m_downPos:{2}",
            //        Time.realtimeSinceStartup, m_upPos, m_downPos ) );

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay( m_downPos );
            if( !Physics.Raycast( ray, out hit ) )
                return;

            if( !( hit.collider is CharacterController ) )
                return;

            ray = Camera.main.ScreenPointToRay( m_upPos );
            if( !Physics.Raycast( ray, out hit ) )
                return;

            if( hit.collider is TerrainCollider ) {
                GameMain.MainPlayer.MoveTo( hit.point );
            }

            GameMain.CurrentWorld.CancelSelectObj( );
        }
    }
}