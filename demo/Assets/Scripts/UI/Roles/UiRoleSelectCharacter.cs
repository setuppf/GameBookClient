using UnityEngine;
using UnityEngine.UI;

namespace GEngine {
    class UiRoleSelectCharacter : UiSubComponent {

        private readonly Text _txtName;
        private ulong _playerSn;
        private Proto.Gender _gender;

        public ulong PlayerSn => _playerSn;
        public Proto.Gender Gender => _gender;

        public UiRoleSelectCharacter( UiBase parent, GameObject obj ) : base( parent, obj ) {
            _txtName = GetUi<Text>( @"Name Text" );
        }

        public override void Update( ToUiData data ) {
            ToUiPlayerProperies playerData = data as ToUiPlayerProperies;
            if ( playerData == null )
                return;

            _playerSn = playerData.Id;
            _gender = playerData.Gender;
            _txtName.text = playerData.Name;
        }

        public bool IsOn( ) {
            return GetUi<Toggle>( ).isOn;
        }
    }
}
