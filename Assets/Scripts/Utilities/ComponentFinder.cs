using UnityEngine;

namespace VertigoDemo.WheelOfFortune.Utilities
{
    public static class ComponentFinder
    {
        public static T FindChildByName<T>(Component root, string targetName) where T : Component
        {
            if (root == null)
                return null;

            T[] children = root.GetComponentsInChildren<T>(true);

            foreach (T child in children)
            {
                if (child.name == targetName)
                    return child;
            }

            return null;
        }

        public static GameObject FindGameObjectByName(Component root, string targetName)
        {
            if (root == null)
                return null;

            Transform[] children = root.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in children)
            {
                if (child.name == targetName)
                    return child.gameObject;
            }

            return null;
        }
    }
}