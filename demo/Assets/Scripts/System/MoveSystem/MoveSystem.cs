using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AI;

namespace GEngine
{
    class MoveSystem : ComponentSystem
    {
        struct MoveGroup
        {
            public Transform Transform;
            public MoveComponent MoveComponent;
        }

        protected override void OnUpdate()
        {
            foreach (var one in GetEntities<MoveGroup>())
            {
                var moveComponent = one.MoveComponent;
                var navMeshAgent = moveComponent.gameObject.GetComponent<NavMeshAgent>();

                if (moveComponent.CornerPoints.Count <= 0)
                {
                    float dis = Vector3.Distance(one.Transform.position, navMeshAgent.destination);
                    if (dis < (navMeshAgent.stoppingDistance + 0.1f))
                    {
                        moveComponent.Role.ChangeState(RoleStateType.Stand);
                        //GameLogger.GetInstance().Output( "remainingDistance<0.1f. realtime:" + Time.realtimeSinceStartup );
                    }

                    continue;
                }

                var corners = moveComponent.CornerPoints;
                var targetPosition = new Vector3()
                {
                    x = corners[corners.Count - 1].X,
                    y = corners[corners.Count - 1].Y,
                    z = corners[corners.Count - 1].Z
                };

                navMeshAgent.SetDestination(targetPosition);
                moveComponent.Role.ChangeState(RoleStateType.Move);
                moveComponent.CornerPoints.Clear();
            }
        }
    }
}
