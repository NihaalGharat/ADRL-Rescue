namespace ADRL.Core.Utilities
{
    using UnityEngine;

    public static class UnityExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();

            if (component == null)
                component = gameObject.AddComponent<T>();

            return component;
        }

        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            return component.gameObject.GetOrAddComponent<T>();
        }

        public static void DestroyChildren(this Transform transform)
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                Object.Destroy(child.gameObject);
            }
        }

        public static void DestroyChildrenImmediate(this Transform transform)
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                Object.DestroyImmediate(child.gameObject);
            }
        }

        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;

            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }

        public static void SetLayerRecursively(this GameObject gameObject, string layerName)
        {
            var layer = LayerMask.NameToLayer(layerName);
            gameObject.SetLayerRecursively(layer);
        }

        public static bool HasComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.GetComponent<T>() != null;
        }

        public static T FindComponentInScene<T>() where T : Component
        {
            return Object.FindObjectOfType<T>();
        }

        public static T[] FindComponentsInScene<T>() where T : Component
        {
            return Object.FindObjectsOfType<T>();
        }

        public static Vector3 GetPosition(this Component component)
        {
            return component.transform.position;
        }

        public static void SetPosition(this Component component, Vector3 position)
        {
            component.transform.position = position;
        }
    }
}
