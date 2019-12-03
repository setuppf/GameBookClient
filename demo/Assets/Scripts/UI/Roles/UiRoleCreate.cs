using UnityEngine;
using UnityEngine.UI;

namespace GEngine {
    class UiRoleCreate : UiRoleBase {

        private Button _btnCreate;

        public Toggle _toggleMale;
        public Toggle _toggleFemale;

        public InputField _inputName;

        private int _playerCount = 0;

        public UiRoleCreate() : base(UiType.RoleCreate)
        {
            _playerCount = 0;

            var uiMgr = UiMgr.GetInstance();
            ToUiAccountInfo obj = uiMgr.GetUpdateData<ToUiAccountInfo>(UiUpdateDataType.AccountInfo);
            if (obj == null)
                return;

            _lastVersion = obj.Version;
            _playerCount = obj.Players.Count;
        }

        protected override bool IsLoaded( ) {
            if( _maleObj == null )
                return false;

            if( _femaleObj == null )
                return false;

            return true;
        }

        protected override void OnInit( ) {
            _btnCreate = GetUi<Button>( @"Button (Create)" );
            _btnCreate.onClick.AddListener( OnClickCreate );

            _toggleMale = GetUi<Toggle>( @"Section (Gender)/Content Grid/Faction (1)" );
            _toggleMale.onValueChanged.AddListener( OnToggleMaleClick );

            _toggleFemale = GetUi<Toggle>( @"Section (Gender)/Content Grid/Faction (2)" );
            _toggleFemale.onValueChanged.AddListener( OnToggleFemaleClick );

            _inputName = GetUi<InputField>( @"Input Field (Character Name)" );

            // 3d 模型
            RawImage rawImage = GetUi<RawImage>( @"Character" );
            RectTransform transformObj = rawImage.GetComponent<RectTransform>( );

            _renderTexture = new RenderTexture( (int)transformObj.sizeDelta.x, (int)transformObj.sizeDelta.y, 24 );
            rawImage.texture = _renderTexture;

            if( _toggleMale.isOn && _maleObj != null ) {
                SetModelActive( _maleObj, true );
            }

            if( _toggleFemale.isOn && _femaleObj != null ) {
                SetModelActive( _femaleObj, true );
            }

            base.OnInit( );
        }

        protected override void OnUpdate( ) {
            var uiMgr = UiMgr.GetInstance();
            ToUiAccountInfo obj = uiMgr.GetUpdateData<ToUiAccountInfo>(UiUpdateDataType.AccountInfo);
            if (obj == null)
                return;

            if (obj.Version == _lastVersion)
                return;

            _lastVersion = obj.Version;
            if (obj.Players.Count > _playerCount)
            {
                this.CloseThisUi();
                UiMgr.GetInstance().CloseModalBox0();  // 关闭正在显示的“角色正在创建中”
                uiMgr.OpenUi(UiType.RoleSelect);
            }
        }

        public void OnToggleMaleClick( bool value ) {
            if( value ) {
                SetModelActive( _maleObj, true );
            } else {
                SetModelActive( _maleObj, false );
            }
        }

        public void OnToggleFemaleClick( bool value ) {
            if( value ) {
                SetModelActive( _femaleObj, true );
            } else {
                SetModelActive( _femaleObj, false );
            }
        }

        private void OnClickCreate( ) {
            if( string.IsNullOrEmpty( _inputName.text ) ) {
                UiMgr.GetInstance( ).OpenModalBox1( "创建角色", "角色名不能为空", null );
                return;
            }

            UiMgr.GetInstance( ).OpenModalBox0( "创建角色", "角色正在创建中..." );
            Proto.CreatePlayer proto = new Proto.CreatePlayer( );
            proto.Name = _inputName.text;
            if( _toggleFemale.isOn )
                proto.Gender = Proto.Gender.Female;
            else
                proto.Gender = Proto.Gender.Male;

            NetworkMgr.GetInstance( ).SendPacket( Proto.MsgId.C2LCreatePlayer, proto );
        }

        protected override void AsyncLoadModelCallBackAfter( ) {
            if( _uiState != UiState.Ok )
                return;

            if( _toggleMale.isOn ) {
                SetModelActive( _maleObj, true );
                SetModelActive( _femaleObj, false );
            }

            if( _toggleFemale.isOn ) {
                SetModelActive( _maleObj, false );
                SetModelActive( _femaleObj, true );
            }
        }
    }
}
