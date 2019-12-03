using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GEngine
{
    enum UiState
    {
        None,
        Loading,
        LoadedGameObj,
        Ok,
    }

    abstract class UiBase : SnObject
    {
        protected UiState _uiState = UiState.None;

        protected readonly UiType _uiType;
        protected GameObject _gameObj;
        protected bool _isEmptyGameObj = false;
        protected bool _isHide = false;

        protected UiBase(UiType uiType, ulong sn) : base(sn)
        {
            _uiType = uiType;
            _uiState = UiState.None;
            _isHide = false;
            _gameObj = null;
            _isEmptyGameObj = false;
        }

        public UiState GetState()
        {
            return _uiState;
        }

        public UiComplexType GetUiComplexType()
        {
            return new UiComplexType(_uiType, _sn);
        }

        public UiType GetUiType()
        {
            return _uiType;
        }

        #region 数据更新

        protected int _lastVersion = -1;

        #endregion

        #region Close Button

        protected Button _btnClose;

        protected void InitCloseBtn(string path)
        {
            _btnClose = GetUi<Button>(path);
            if (_btnClose != null)
                _btnClose.onClick.AddListener(CloseThisUi);
        }

        public void CloseThisUi()
        {
            Hide();
            UiMgr.GetInstance().CloseUi(_uiType, _sn);
        }

        public void Destroy()
        {
            OnDestroy();
            if (_gameObj != null)
                UnityEngine.Object.Destroy(_gameObj);
        }

        #endregion

        public void AttachGameObject(GameObject obj)
        {
            _gameObj = obj;

            if (_gameObj == null)
                _isEmptyGameObj = true;

            // 还没有完成初始化，对象不可见
            _gameObj?.SetActive(false);
        }

        public void Hide()
        {
            _isHide = true;
            _gameObj?.SetActive(false);
        }

        public void Show()
        {
            _isHide = false;
            _gameObj?.SetActive(true);
        }

        public void Update()
        {
            switch (_uiState)
            {
                case UiState.None:
                    {
                        OnAwake();

                        if (_isEmptyGameObj)
                            _uiState = UiState.LoadedGameObj;
                        else
                            _uiState = _gameObj == null ? UiState.Loading : UiState.LoadedGameObj;

                        break;
                    }
                case UiState.Loading:
                    {
                        if (_isEmptyGameObj)
                            _uiState = UiState.LoadedGameObj;
                        else
                        {
                            if (_gameObj != null)
                                _uiState = UiState.LoadedGameObj;
                        }

                        break;
                    }
                case UiState.LoadedGameObj:
                    {
                        if (!IsLoaded())
                            break;

                        OnInit();
                        OnUpdate();

                        // 初始化完成之后，界面对象才可见
                        if (_gameObj != null)
                            _gameObj.SetActive(true);

                        _uiState = UiState.Ok;
                        break;
                    }
                case UiState.Ok:
                    {
                        OnUpdate();
                        break;
                    }
            }
        }

        protected T GetUi<T>(string name)
        {
            Transform tf = UiUtil.GetTransform(_gameObj, name);
            if (tf == null)
                throw new Exception($"Get failed; name:{name}");
            return tf.GetComponent<T>();
        }

        protected abstract void OnAwake();
        protected abstract bool IsLoaded();

        protected abstract void OnInit();
        protected abstract void OnUpdate();
        protected abstract void OnDestroy();
    }

}