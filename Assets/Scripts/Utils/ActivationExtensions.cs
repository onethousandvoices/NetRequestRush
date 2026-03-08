using UnityEngine;

public static class ActivationExtensions
{
    public static void SetActiveSafe(this GameObject gameObject, bool isActive)
    {
        if (gameObject && gameObject.activeSelf != isActive)
            gameObject.SetActive(isActive);
    }

    public static void SetActiveSafe(this Component component, bool isActive)
    {
        if (component)
            component.gameObject.SetActiveSafe(isActive);
    }
}
