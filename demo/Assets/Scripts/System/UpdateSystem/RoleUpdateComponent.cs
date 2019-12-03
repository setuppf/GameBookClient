using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GEngine
{
    class RoleUpdateComponent : MonoBehaviour
    {
        private RoleAppear _role;
        public RoleAppear Role => _role;
        public void AttachRole(RoleAppear role)
        {
            _role = role;
        }

        void Update()
        {
            _role.UpdateState();
        }
    }
}
