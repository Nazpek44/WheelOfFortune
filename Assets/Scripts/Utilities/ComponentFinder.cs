using UnityEngine;

namespace VertigoDemo.WheelOfFortune.Utilities
{
    /// <summary>
    /// Editor time helper for wiring serialized references by child name.
    /// Only ever called from <c>Reset()</c>, never from <c>OnValidate()</c> or a
    /// runtime path, so it cannot scan the scene while the game is running.
    /// </summary>
    public static class ComponentFinder
    {
        public static T FindChildByName<T>(Component root, string targetName) where T : Component
        {
            if (!root)
                return null;

            T[] children = root.GetComponentsInChildren<T>(true);

            for (int i = 0; i < children.Length; i++)
            {
                T child = children[i];

                if (child && child.name == targetName)
                    return child;
            }

            return null;
        }

        public static GameObject FindGameObjectByName(Component root, string targetName)
        {
            if (!root)
                return null;

            Transform[] children = root.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i];

                if (child && child.name == targetName)
                    return child.gameObject;
            }

            return null;
        }
    }
}
