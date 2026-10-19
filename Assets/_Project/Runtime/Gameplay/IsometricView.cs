using UnityEngine;
using UnityEngine.UI;

namespace Zombtoy.Gameplay
{
    // Fixed camera for the foundation. Cinemachine is available for production rigs.
    [RequireComponent(typeof(Camera))]
    public sealed class IsometricView : MonoBehaviour
    {
        public Transform target;
        public int width = 480;
        public int height = 270;
        private Camera view;
        private RenderTexture image;
        private GameObject canvas;

        private void Start()
        {
            view = GetComponent<Camera>();
            image = new RenderTexture(width, height, 24) { filterMode = FilterMode.Point, name = "MVP 480x270" };
            image.Create();
            view.targetTexture = image;
            canvas = new GameObject("Low resolution presentation", typeof(Canvas), typeof(CanvasScaler));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.GetComponent<Canvas>().sortingOrder = -100;
            var output = new GameObject("Point filtered image", typeof(RectTransform), typeof(RawImage));
            output.transform.SetParent(canvas.transform, false);
            var rect = output.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            output.GetComponent<RawImage>().texture = image;
            output.GetComponent<RawImage>().raycastTarget = false;
        }

        private void LateUpdate()
        {
            if (target == null) return;
            transform.rotation = Quaternion.Euler(30, 45, 0);
            transform.position = target.position + Vector3.up - transform.forward * 24f;
        }

        private void OnDestroy()
        {
            if (view != null) view.targetTexture = null;
            if (image != null) { image.Release(); Destroy(image); }
            if (canvas != null) Destroy(canvas);
        }
    }
}
