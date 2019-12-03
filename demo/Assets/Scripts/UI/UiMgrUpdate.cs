using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEngine {

    enum UiUpdateDataType {
        AccountInfo, // 账号数据
    }

    abstract class ToUiData {
        public int Version;
    }

    internal abstract class IToUi<T> where T : ToUiData {
        protected UiUpdateDataType _uiUpdataType;
        protected int _version;
        protected IToUi( UiUpdateDataType type ) {
            _uiUpdataType = type;
        }

        protected void UpdataUiData( T obj ) {
            _version++;
            obj.Version = _version;
            UiMgr.GetInstance( ).UpdateUiData( _uiUpdataType, obj );
        }

        protected abstract void ToUi( );
    }

    public delegate T ToUi<out T>( );

    partial class UiMgr {
        private readonly Dictionary<UiUpdateDataType, ToUiData> _updateDatas = new Dictionary<UiUpdateDataType, ToUiData>( );

        public void UpdateUiData( UiUpdateDataType eType, ToUiData obj ) {
            if ( !_updateDatas.ContainsKey( eType ) ) {
                _updateDatas.Add( eType, obj );
            } else {
                _updateDatas[eType] = obj;
            }
        }

        public T GetUpdateData<T>( UiUpdateDataType eType ) where T : ToUiData {
            if ( !_updateDatas.ContainsKey( eType ) )
                return default( T );

            return _updateDatas[eType] as T;
        }
    }
}
