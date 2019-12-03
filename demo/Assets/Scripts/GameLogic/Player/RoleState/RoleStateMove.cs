using UnityEngine;

namespace GEngine
{
    class RoleStateMove : RoleState
    {
        public override RoleStateType GetState()
        {
            return RoleStateType.Move;
        }

        public override RoleStateType Update()
        {
            return GetState();
        }

        public override void EnterState(RoleStateType lastStateType)
        {
            var animation = _parentObj.GetGameObject().GetComponent<Animation>();
            animation.Play("move");
        }

        public override void LeaveState()
        {

        }
    }
}