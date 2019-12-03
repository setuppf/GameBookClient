
using UnityEngine;

namespace GEngine {
    class CameraFollowBehaviour : MonoBehaviour {

        //相机相对于玩家的位置 
        public Vector3 Offset = new Vector3( 0, 4, 8 );

        // player transform
        public Transform Target = null;

        // 相机移动时的旋转速度
        public float Speed = 2;

        // 远近平滑移动值 
        public float Smooth = 50f;

        public float sensitivetyZ = 2f;

        void Update( ) {
            if( Target == null )
                return;

            Vector3 pos = Target.position + Offset;

            //调整相机与玩家之间的距离 
            this.transform.position = Vector3.Lerp( this.transform.position, pos, Speed * Time.deltaTime );

            //获取旋转角度 
            Quaternion angel = Quaternion.LookRotation( Target.position - this.transform.position );
            this.transform.rotation = Quaternion.Slerp( this.transform.rotation, angel, Speed * Time.deltaTime );
            //transform.LookAt(Target.transform.position);

            // 鼠标轴控制相机的远近 
            if (((Input.mouseScrollDelta.y < 0 && Camera.main.fieldOfView >= 3)) || (Input.mouseScrollDelta.y > 0 && Camera.main.fieldOfView <= 80))
            {
                Camera.main.fieldOfView += Input.mouseScrollDelta.y * Smooth * Time.deltaTime;
            }

        }
    }
}
