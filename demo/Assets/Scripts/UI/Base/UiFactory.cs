using System;
using System.Collections.Generic;

namespace GEngine
{
    class UiInfo
    {
        public delegate T CreateUiCallback<T>();

        public string AssetBundlePath;
        public Delegate Fun;

        public UiInfo(string assetBundlePath)
        {
            AssetBundlePath = assetBundlePath;
        }

        public void AttachCallbace<T>(CreateUiCallback<T> fun)
        {
            Fun = fun;
        }
    }

    class UiFactory : SingletonObject<UiFactory>
    {
        private Dictionary<UiType, string> _uiAbInfo = new Dictionary<UiType, string>();

        public UiFactory()
        {
            _uiAbInfo.Add(UiType.LoadingBar, "ui/loadingbar");
            _uiAbInfo.Add(UiType.Login, "ui/login");
            _uiAbInfo.Add(UiType.RoleCreate, "ui/roles/create");
            _uiAbInfo.Add(UiType.RoleSelect, "ui/roles/select");
            _uiAbInfo.Add(UiType.ModalBox0, "ui/modal/0");
            _uiAbInfo.Add(UiType.ModalBox1, "ui/modal/1");
            _uiAbInfo.Add(UiType.ModalBox2, "ui/modal/2");
        }

        public string GetAssetBundle(UiType uiType)
        {
            if (_uiAbInfo.ContainsKey(uiType))
                return _uiAbInfo[uiType];

            return null;
        }

        public UiBase Create(UiType uiType, ulong sn)
        {
            switch (uiType)
            {
                case UiType.LoadingBar:
                    return new UiLoadBar();
                case UiType.Roles:
                    return new UiRoles();
                case UiType.Login:
                    return new UiLogin();
                case UiType.RoleCreate:
                    return new UiRoleCreate();
                case UiType.RoleSelect:
                    return new UiRoleSelect();
                case UiType.ModalBox0:
                    return new ModalBox0();
                case UiType.ModalBox1:
                    return new ModalBox1();
                default:
                    GameLogger.GetInstance().Debug($" !!!!! can't found handler. uiType:{uiType}");
                    break;
            }

            return null;
        }
    }

}