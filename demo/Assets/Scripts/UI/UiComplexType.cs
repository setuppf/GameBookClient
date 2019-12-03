using System;

namespace GEngine {
    // 同一个窗口可能拥有者不同，例如不同玩家的属性界面
    struct UiComplexType : IComparable<UiComplexType> {
        public UiType UiType;
        public ulong Sn;

        public UiComplexType( UiType uiType ) {
            this.UiType = uiType;
            this.Sn = 0;
        }

        public UiComplexType( UiType uiType, ulong sn ) {
            this.UiType = uiType;
            this.Sn = sn;
        }

        public int CompareTo( UiComplexType obj ) {
            if ( this.UiType != obj.UiType )
                return -1;

            if ( this.Sn != obj.Sn )
                return -1;

            return 0;
        }
    }
}