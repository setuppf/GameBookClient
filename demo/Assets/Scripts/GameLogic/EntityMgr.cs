using Unity.Entities;
using UnityEngine;

namespace GEngine
{
    class EntityMgr : MonoBehaviour
    {
        public EntityArchetype MoveArchetype;

        public void Awake()
        {
            var entityManager = Unity.Entities.World.Active.GetOrCreateManager<EntityManager>();
            MoveArchetype = entityManager.CreateArchetype(typeof(Transform), typeof(MoveComponent));
        }

    }
}
