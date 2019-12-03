using System;
using UnityEngine.UI;

namespace GEngine {

    class ModalBox0 : UiBase {
        public string Tip { get; set; }
        public string Title { get; set; }

        private Text _txtTips;
        private Text _txtTitle;

        public ModalBox0( ) : base( UiType.ModalBox0, 0 ) {
        }

        protected override void OnAwake( ) { }
        protected override bool IsLoaded( ) {
            return true;
        }

        protected override void OnInit( ) {
            _txtTitle = GetUi<Text>( @"Inner Box/Content Group/Title" );
            _txtTitle.text = Title;

            _txtTips = GetUi<Text>( @"Inner Box/Content Group/Content" );
            _txtTips.text = Tip;
        }

        protected override void OnDestroy( ) {
        }

        protected override void OnUpdate( ) {
        }
    }
}
