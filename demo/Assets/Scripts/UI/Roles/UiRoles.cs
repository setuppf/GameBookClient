using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEngine
{
    class UiRoles : UiBase
    {
        public UiRoles() : base(UiType.Roles, 0) { }

        protected override void OnAwake() { }

        protected override bool IsLoaded()
        {
            return true;
        }

        protected override void OnInit() { }
        protected override void OnDestroy() { }

        protected override void OnUpdate()
        {
            var uiMgr = UiMgr.GetInstance();
            ToUiAccountInfo obj = uiMgr.GetUpdateData<ToUiAccountInfo>(UiUpdateDataType.AccountInfo);
            if (obj == null)
                return;

            if (obj.Version == _lastVersion)
                return;

            _lastVersion = obj.Version;
            if (obj.Players.Count == 0)
            {
                uiMgr.OpenUi(UiType.RoleCreate);
            }
            else
            {
                uiMgr.OpenUi(UiType.RoleSelect);
            }

            CloseThisUi();
        }
    }
}
