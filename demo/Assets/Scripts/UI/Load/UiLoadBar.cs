using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GEngine {
    class UiLoadBar : UiBase {

        private float _maxWidth;
        private Image _fillImg;
        public UiLoadBar( ) : base( UiType.LoadingBar, 0 ) { }

        protected override void OnAwake( ) { }
        protected override bool IsLoaded( ) {
            return true;
        }

        protected override void OnInit( ) {
            _fillImg = GetUi<Image>( "Fill Rect/Fill Mask/Fill" );
            _maxWidth = _fillImg.rectTransform.rect.width;
            Fill( 0.0f );
        }

        protected override void OnDestroy( ) { }

        protected override void OnUpdate( ) { }

        public void Fill( float progress ) {
            _fillImg.rectTransform.sizeDelta = new Vector2( _maxWidth * progress, _fillImg.rectTransform.sizeDelta.y );
            //GameLogger.GetInstance( ).Debug( "ui loading bar. fill. progress:" + " width:" + _maxWidth * progress );
        }

    }

}