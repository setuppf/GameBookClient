
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace GEngine
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NavMeshAgent))]
    class MoveComponent : MonoBehaviour
    {
        public List<Proto.Vector3> CornerPoints = new List<Proto.Vector3>();

        private Vector3 _nextPosition = Vector3.zero;

        private RoleAppear _role;
        public RoleAppear Role => _role;
        public void AttachRole(RoleAppear role)
        {
            _role = role;
        }

        void Awake()
        {
            var rigidbody = GetComponent<Rigidbody>();
            //_collider = GetComponent<CapsuleCollider>( );

            rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            //_collider.center = new Vector3( 0, 1, 0 );
            //_collider.radius = 1;

            Vector3 extraGravityForce = Physics.gravity;
            rigidbody.AddForce(extraGravityForce);

            var navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.speed = 2f;
            navMeshAgent.acceleration = 360;
            navMeshAgent.angularSpeed = 1f;
            navMeshAgent.stoppingDistance = 0.1f;

            CoroutineEngine.GetInstance().Execute(TimerChange());
        }

        private IEnumerator TimerChange()
        {
            while (true)
            {
                yield return new WaitForSeconds(1.0f);
                GameLogger.GetInstance().Debug($"player position:{gameObject.transform.position}");
            }
        }

        void Update()
        {
            var navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
            if (!navMeshAgent.hasPath)
                return;

            Vector3 comparePos;
            if (navMeshAgent.path.corners.Length > 2)
            {
                comparePos = navMeshAgent.path.corners[1];
            }
            else
            {
                comparePos = navMeshAgent.destination;
            }

            if (_nextPosition == comparePos)
                return;

            _nextPosition = comparePos;

            gameObject.transform.LookAt(comparePos);
            //GameLogger.GetInstance().Debug($"player position:{gameObject.transform.position}, destination position:{navMeshAgent.destination}, nextPosition:{navMeshAgent.nextPosition}, comparePos:{comparePos}");
        }
    }
}
