using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

public static class InstallerResourceUtility
{
    public static void BindByConcreteType<T>(DiContainer container, IEnumerable<T> instances) where T : class
    {
        foreach (var instance in instances)
            container.Bind(instance.GetType()).FromInstance(instance).AsSingle();
    }

    public static List<Component> InstantiateComponents(IReadOnlyList<GameObject> prefabs, Transform parent, Predicate<Component> include)
    {
        var result = new List<Component>();

        for (var i = 0; i < prefabs.Count; i++)
        {
            var components = Object.Instantiate(prefabs[i], parent, false).GetComponents<MonoBehaviour>();
            for (var j = 0; j < components.Length; j++)
            {
                var component = components[j];
                if (!component)
                    continue;
                if (!include(component))
                    continue;

                result.Add(component);
            }
        }

        return result;
    }
}
