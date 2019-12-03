using UnityEngine;

namespace GEngine
{
    class RoleStateStand : RoleState
    {
        public override RoleStateType GetState()
        {
            return RoleStateType.Stand;
        }

        public override RoleStateType Update()
        {
            return GetState();
        }

        public override void EnterState(RoleStateType lastStateType)
        {
            if (_parentObj.GetGameObject() == null)
                return;

            var animation = _parentObj.GetGameObject().GetComponent<Animation>();
            animation.Play("stand");
        }

        public override void LeaveState()
        {

        }
    }
}