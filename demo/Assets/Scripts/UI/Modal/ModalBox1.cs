using System;
using UnityEngine.UI;

namespace GEngine {
    class ModalBox1 : UiBase {
        public string Tip { get; set; }
        public string Title { get; set; }
        public Action CloseAction;

        private Text _txtTips;
        private Text _txtTitle;
        private Button _BtnOk;

        public ModalBox1( ) : base( UiType.ModalBox1, 0 ) { }

        protected override void OnAwake( ) { }
        protected override bool IsLoaded( ) {
            return true;
        }

        protected override void OnInit( ) {
            _txtTitle = GetUi<Text>( @"Inner Box/Content Group/Title" );
            _txtTitle.text = Title;

            _txtTips = GetUi<Text>( @"Inner Box/Content Group/Content" );
            _txtTips.text = Tip;

            _BtnOk = GetUi<Button>( $@"Inner Box/Button Group/Button (Ok)" );
            _BtnOk.onClick.AddListener( OnClick );

        }

        protected override void OnDestroy( ) { }

        protected override void OnUpdate( ) { }

        private void OnClick( ) {
            CloseAction?.Invoke( );
            CloseThisUi( );
        }
    }
}
