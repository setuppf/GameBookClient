using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace GEngine
{
    class RoleAppear : StateTemplateMgr<RoleStateType, RoleAppear>
    {
        protected ulong _sn;
        public ulong Sn => _sn;

        protected string _name;
        protected Proto.Gender _gender;
        protected Vector3 _position = Vector3.zero;

        protected GameObject _3dObj = null;

        public RoleAppear()
        {
            InitStateTemplateMgr(RoleStateType.Stand);
        }

        public GameObject GetGameObject()
        {
            return _3dObj;
        }

        public void Load3DObj()
        {
            string path = _gender == Proto.Gender.Female ? @"models/player/02" : @"models/player/01";
            AssetBundleMgr.GetInstance().AsyncLoad(path, AsyncLoadModelCallBack, null);
        }

        public void Parse(Proto.Role proto)
        {
            if (proto.Sn > 0)
                _sn = proto.Sn;

            if (!string.IsNullOrEmpty(proto.Name))
                _name = proto.Name;

            if (proto.Gender != Proto.Gender.None)
                _gender = proto.Gender;

            //
            _position.x = proto.Position.X;
            _position.y = proto.Position.Y;
            _position.z = proto.Position.Z;

            Debug.Log($"role appear. proto pos:{proto.Position} pos:{_position}");
        }

        protected void AsyncLoadModelCallBack(object context, AssetBundle asset)
        {
            _3dObj = Object.Instantiate(asset.LoadAsset(asset.GetAllAssetNames()[0])) as GameObject;
            if (_3dObj == null)
                return;

            // GameObjectEntity 需要先false，再true 才有效果
            // GameObjectEntity 才会进入EntityManager
            _3dObj.SetActive(false);

            Transform transform = _3dObj.GetComponent<Transform>();
            transform.position = _position;
            transform.Rotate(0, 0, 0);
            transform.localScale = new Vector3(1, 1, 1);

            _3dObj.AddComponent<GameObjectEntity>();
            var moveComponent = _3dObj.AddComponent<MoveComponent>();
            moveComponent.AttachRole(this);

            var roleComponent = _3dObj.AddComponent<RoleUpdateComponent>();
            roleComponent.AttachRole(this);

            _3dObj.SetActive(true);

            if (_sn == GameMain.GetInstance().MainPlayer.Sn)
            {
                GameMain.GetInstance().MainPlayer.SetGameObject(_3dObj);
                _3dObj.name = "MainPlayer";
                Object.DontDestroyOnLoad(_3dObj);

                CameraFollowBehaviour cTrack = Camera.main.gameObject.GetComponent<CameraFollowBehaviour>();
                if (cTrack != null)
                    cTrack.Target = _3dObj.GetComponent<Transform>();
            }
            else
            {
                _3dObj.name = "Sync_" + _name;
            }
        }

        #region 状态相关

        protected override void RegisterState()
        {
            RegisterStateClass(RoleStateType.Stand, new StateTemplateCreator<RoleStateType, StateTemplate<RoleStateType, RoleAppear>>(RoleStateType.Stand, () => new RoleStateStand()));
            RegisterStateClass(RoleStateType.Move, new StateTemplateCreator<RoleStateType, StateTemplate<RoleStateType, RoleAppear>>(RoleStateType.Stand, () => new RoleStateMove()));
        }

        #endregion
    }
}
