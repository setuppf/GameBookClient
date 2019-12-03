using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEngine
{
    enum RoleStateType
    {
        Stand,
        Move,
    }

    abstract class RoleState : StateTemplate<RoleStateType, RoleAppear>
    {
    }
}
