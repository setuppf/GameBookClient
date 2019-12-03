
using UnityEngine;

namespace GEngine
{
    abstract class UiRoleBase : UiBase
    {
        protected Camera _camera;
        protected RenderTexture _renderTexture;

        protected GameObject _maleObj = null;
        protected GameObject _femaleObj = null;

        protected GameObject _cameraRenderRole = null;

        public UiRoleBase(UiType uiType) : base(uiType, 0) { }

        protected override void OnInit()
        {
            CreateCamera();
        }

        private void CreateCamera()
        {
            _cameraRenderRole = new GameObject("CameraRenderRole");
            Transform transform = _cameraRenderRole.GetComponent<Transform>();
            transform.position = new Vector3(0, 0, -5);
            transform.Rotate(0, 0, 0);
            transform.localScale = new Vector3(1, 1, 1);

            _camera = _cameraRenderRole.AddComponent<Camera>();
            _camera.clearFlags = CameraClearFlags.Color | CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            _camera.orthographic = true; // 正交投影
            _camera.fieldOfView = 32.0f;
            _camera.depth = 50.0f;
            _camera.nearClipPlane = 1.6f;
            _camera.farClipPlane = 60.0f;
            _camera.orthographicSize = 1.5f;
            _camera.targetTexture = _renderTexture;

            // 增加一个灯光效果
            Light lightObj = _cameraRenderRole.AddComponent<Light>();
            lightObj.type = LightType.Directional;
            lightObj.color = Color.gray;
        }

        protected override void OnAwake()
        {
            // 加载模型
            AssetBundleMgr.GetInstance().AsyncLoad(@"models/player/01", AsyncLoadModelCallBack, new { isFemale = false });
            AssetBundleMgr.GetInstance().AsyncLoad(@"models/player/02", AsyncLoadModelCallBack, new { isFemale = true });
        }

        protected void SetModelActive(GameObject obj, bool isOn)
        {
            if (_uiState != UiState.Ok)
                return;

            if (!isOn)
            {
                obj.SetActive(false);
                return;
            }

            obj.SetActive(true);
            Animation anims = obj.GetComponent<Animation>();
            anims.wrapMode = WrapMode.Loop;
            if (!anims.isPlaying)
            {
                anims.Play("stand");
            }
        }

        protected abstract void AsyncLoadModelCallBackAfter();

        private void AsyncLoadModelCallBack(object context, AssetBundle asset)
        {
            var loadingInfo = new { isFemale = false };
            loadingInfo = Util.ChangeType(context, loadingInfo);

            GameObject playerObj;
            if (!loadingInfo.isFemale)
            {
                _maleObj = MonoBehaviour.Instantiate(asset.LoadAsset(asset.GetAllAssetNames()[0])) as GameObject;
                _maleObj.SetActive(false);
                playerObj = _maleObj;
            }
            else
            {
                _femaleObj = MonoBehaviour.Instantiate(asset.LoadAsset(asset.GetAllAssetNames()[0])) as GameObject;
                _femaleObj.SetActive(false);
                playerObj = _femaleObj;
            }

            Transform transform = playerObj.GetComponent<Transform>();
            transform.position = new Vector3(0, -0.8f, 0);
            transform.Rotate(0, 180, 0);
            transform.localScale = new Vector3(1, 1, 1);

            AsyncLoadModelCallBackAfter();
        }

        protected override void OnDestroy()
        {
            if (_maleObj != null)
                UnityEngine.Object.Destroy(_maleObj);

            if (_femaleObj != null)
                UnityEngine.Object.Destroy(_femaleObj);

            UnityEngine.Object.Destroy(_cameraRenderRole);
        }
    }
}
