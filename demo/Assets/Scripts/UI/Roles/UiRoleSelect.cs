using System;
using System.Collections.Generic;
using Proto;
using UnityEngine;
using UnityEngine.UI;

namespace GEngine
{
    class UiRoleSelect : UiRoleBase
    {

        private Button _btnPlayer;
        private Button _btnCreate;

        private ToggleGroup _toggleGroup;

        // enter game
        private AppType _linkAppType;
        private string _account;
        private string _token;

        private List<UiRoleSelectCharacter> _players = new List<UiRoleSelectCharacter>();

        // 每个玩家显示行模板
        private GameObject _characterObj;

        // 玩家显示挂靠的父类
        private Transform _characterParent;

        public UiRoleSelect() : base(UiType.RoleSelect)
        {
            _linkAppType = AppType.Login;
        }

        protected override void OnAwake()
        {
            base.OnAwake();

            // 加载Perfab
            AssetBundleMgr.GetInstance().AsyncLoad(@"ui/roles/select_character", AsyncLoadCallBack, null);
        }

        protected override void AsyncLoadModelCallBackAfter()
        {

        }

        protected override bool IsLoaded()
        {
            return _characterObj != null;
        }

        protected override void OnInit()
        {
            // 事件
            EventDispatcher eDispatcher = EventDispatcher.GetInstance();
            eDispatcher.RegisterEvent<AppType>(eEventType.Connected, EventNetworkConnected);
            eDispatcher.RegisterEvent<AppType>(eEventType.Disconnect, EventNetworkDisconnect);

            MessagePackDispatcher msgDispatcher = MessagePackDispatcher.GetInstance();
            msgDispatcher.RegisterFollowPacket((int)Proto.MsgId.L2CGameToken, MsgGameToken);
            msgDispatcher.RegisterFollowPacket((int)Proto.MsgId.C2GLoginByTokenRs, MsgLoginByTokenRs);

            // UI
            _btnPlayer = GetUi<Button>(@"Character Select List/Button (Play)");
            _btnPlayer.onClick.AddListener(OnClickPlayer);

            _toggleGroup = GetUi<ToggleGroup>(@"Character Select List");

            _btnCreate = GetUi<Button>(@"Character Select List/Characters List/Content/Button (Create)");
            _btnCreate.onClick.AddListener(OnClickCreate);

            // 角色列表对象
            _characterParent = UiUtil.GetTransform(_gameObj, @"Character Select List/Characters List/Content/Characters");

            // 3d 模型
            RawImage rawImage = GetUi<RawImage>(@"Character");
            RectTransform transformObj = rawImage.GetComponent<RectTransform>();

            _renderTexture = new RenderTexture((int)transformObj.sizeDelta.x, (int)transformObj.sizeDelta.y, 24);
            rawImage.texture = _renderTexture;

            base.OnInit();
        }

        protected override void OnDestroy()
        {
            MessagePackDispatcher msgDispatcher = MessagePackDispatcher.GetInstance();
            msgDispatcher.RemoveFollowPacket((int)Proto.MsgId.C2GLoginByTokenRs, MsgLoginByTokenRs);
            msgDispatcher.RemoveFollowPacket((int)Proto.MsgId.L2CGameToken, MsgGameToken);

            // 事件
            EventDispatcher eDispatcher = EventDispatcher.GetInstance();
            eDispatcher.RemoveEvent<AppType>(eEventType.Connected, EventNetworkConnected);
            eDispatcher.RemoveEvent<AppType>(eEventType.Disconnect, EventNetworkDisconnect);

            if (_characterObj != null)
                UnityEngine.Object.Destroy(_characterObj);

            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            if (_characterObj == null)
                return;

            ToUiAccountInfo obj = UiMgr.GetInstance().GetUpdateData<ToUiAccountInfo>(UiUpdateDataType.AccountInfo);
            if (obj == null)
                return;

            if (obj.Version == _lastVersion)
                return;

            _lastVersion = obj.Version;
            _account = obj.Account;
            int size = obj.Players.Count;
            CheckUiComponents(size);

            for (int i = 0; i < size; i++)
            {
                _players[i].Update(obj.Players[i]);
            }
        }

        private void CheckUiComponents(int size)
        {
            if (_players.Count == size)
                return;

            int dis = size - _players.Count;
            if (dis > 0)
            {
                // 数据大于控件个数   
                for (int i = 0; i < dis; i++)
                {
                    GameObject newObj = UnityEngine.Object.Instantiate(_characterObj);
                    newObj.SetActive(true);

                    newObj.transform.SetParent(_characterParent.transform);
                    newObj.transform.Rotate(0, 0, 0);
                    newObj.transform.localScale = new UnityEngine.Vector3(1, 1, 1);

                    UiRoleSelectCharacter uiCh = new UiRoleSelectCharacter(this, newObj);
                    uiCh.GetUi<Toggle>().group = _toggleGroup;
                    uiCh.GetUi<Toggle>().isOn = i == 0;    // 第一个亮起
                    uiCh.GetUi<Toggle>().onValueChanged.AddListener(delegate
                    {
                        ToggleValueChanged(uiCh.GetUi<Toggle>(), uiCh);
                    });

                    _players.Add(uiCh);
                }
            }
            else
            {
                for (int i = 0; i < dis; i++)
                {
                    _players.RemoveAt(0);
                }
            }
        }

        void ToggleValueChanged(Toggle change, UiRoleSelectCharacter role)
        {
            if (!change.isOn)
                return;

            if (role.Gender == Gender.Female)
            {
                SetModelActive(_femaleObj, true);
                SetModelActive(_maleObj, false);
            }
            else
            {
                SetModelActive(_femaleObj, false);
                SetModelActive(_maleObj, true);
            }
        }

        private void AsyncLoadCallBack(object context, AssetBundle asset)
        {
            _characterObj = UnityEngine.Object.Instantiate(asset.LoadAsset(asset.GetAllAssetNames()[0])) as GameObject;
            if (_characterObj == null)
                return;

            _characterObj.SetActive(false);
        }

        private void OnClickPlayer()
        {
            foreach (var one in _players)
            {
                if (!one.IsOn())
                    continue;

                UiMgr.GetInstance().OpenModalBox0("选择角色", "正在进入地图...");
                Proto.SelectPlayer proto = new Proto.SelectPlayer();
                proto.PlayerSn = one.PlayerSn;

                NetworkMgr.GetInstance().SendPacket(Proto.MsgId.C2LSelectPlayer, proto);
                return;
            }
        }

        private void OnClickCreate()
        {
            this.CloseThisUi();
            UiMgr.GetInstance().OpenUi(UiType.RoleCreate);
        }

        private void EventNetworkConnected(AppType appType)
        {
            if (appType != AppType.Game)
                return;

            // 发送Token
            Proto.LoginByToken protoToken = new Proto.LoginByToken
            {
                Token = _token,
                Account = _account,
            };

            NetworkMgr.GetInstance().SendPacket(Proto.MsgId.C2GLoginByToken, protoToken);
        }

        private void EventNetworkDisconnect(AppType appType)
        {
            // 只关心 AppType.Login 的网络链接情况
            if (appType != AppType.Login)
                return;

            if (appType != _linkAppType)
                return;

            UiMgr.GetInstance().CloseModalBox0();
            UiMgr.GetInstance().OpenModalBox1("网络消息", $"网络断开，请重新登录...", delegate ()
            {
                UiMgr.GetInstance().CloseAll();
                UiMgr.GetInstance().OpenUi(UiType.Login);
            });
        }

        private void MsgGameToken(Google.Protobuf.IMessage msg)
        {
            Proto.GameToken gameInfo = msg as Proto.GameToken;
            if (gameInfo == null)
                return;

            _token = gameInfo.Token;

            UiMgr.GetInstance().OpenModalBox0("登录消息", "正在连接游戏服务器...");

            NetworkMgr.GetInstance().Disconnect();
            NetworkMgr.GetInstance().Connect(gameInfo.Ip, gameInfo.Port, AppType.Game);

            _linkAppType = AppType.Game;
        }

        private void MsgLoginByTokenRs(Google.Protobuf.IMessage msg)
        {
            Proto.LoginByTokenRs protoRs = msg as Proto.LoginByTokenRs;
            if (protoRs == null)
                return;

            switch (protoRs.ReturnCode)
            {
                case Proto.LoginByTokenRs.Types.ReturnCode.LgrcNotFoundAccount:
                    UiMgr.GetInstance().CloseModalBox0();
                    UiMgr.GetInstance().OpenModalBox1("登录消息", $"[No:{protoRs.ReturnCode}]Token与账号不匹配...", Show);
                    break;
                case Proto.LoginByTokenRs.Types.ReturnCode.LgrcTokenWrong:
                    UiMgr.GetInstance().CloseModalBox0();
                    UiMgr.GetInstance().OpenModalBox1("登录消息", $"[No:{protoRs.ReturnCode}]Token不匹配...", Show);
                    break;
                case Proto.LoginByTokenRs.Types.ReturnCode.LgrcUnkonwn:
                    UiMgr.GetInstance().CloseModalBox0();
                    UiMgr.GetInstance().OpenModalBox1("登录消息", $"[No:{protoRs.ReturnCode}]未知错误...", Show);
                    break;
                case Proto.LoginByTokenRs.Types.ReturnCode.LgrcOk:
                    UiMgr.GetInstance().OpenModalBox0("登录消息", "登录成功，正在进入游戏...");
                    break;
                default:
                    UiMgr.GetInstance().CloseModalBox0();
                    UiMgr.GetInstance().OpenModalBox1("登录消息", $"[No:{protoRs.ReturnCode}]未知错误...", Show);
                    break;
            }
        }
    }
}
