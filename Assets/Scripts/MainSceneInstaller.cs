using System;
using System.Collections.Generic;
using System.Reflection;
using Features.Clicker;
using Features.DogBreeds;
using Features.Weather;
using Services;
using UI;
using UnityEngine;
using Zenject;

public sealed class MainSceneInstaller : MonoInstaller
{
    private const string CONFIGS_FOLDER = "Configs";
    private const string RUNTIME_UI_FOLDER = "Prefabs/StartupUI";
    private const string SUPPORT_UI_FOLDER = "Prefabs/UI";
    private const string CURRENCY_FLY_PREFAB_NAME = "CurrencyFlyEffect";
    private const string BREED_LIST_ITEM_PREFAB_NAME = "BreedListItem";
    private static readonly Assembly PROJECT_ASSEMBLY = typeof(MainSceneInstaller).Assembly;

    [SerializeField] private Canvas _mainCanvas;
    [SerializeField] private AudioService _audioService;

    public override void InstallBindings()
    {
        var configs = Resources.LoadAll<ScriptableObject>(CONFIGS_FOLDER);
        var runtimeUiPrefabs = LoadResources<GameObject>(RUNTIME_UI_FOLDER);
        var canvasTransform = (RectTransform)_mainCanvas.transform;
        InstallerResourceUtility.BindByConcreteType(Container, configs);

        var runtimeUi = InstallerResourceUtility.InstantiateComponents(runtimeUiPrefabs, canvasTransform, ShouldBindRuntimeUiComponent);
        ConfigureStartupPanels(runtimeUi);
        InstallerResourceUtility.BindByConcreteType(Container, runtimeUi);

        var dogBreedsView = GetRuntimeUiComponent<DogBreedsView>(runtimeUi);
        var vfxPool = CreateVfxPool(canvasTransform);
        var currencyFlyPrefab = LoadSupportPrefabComponent<CurrencyFlyEffect>(CURRENCY_FLY_PREFAB_NAME);
        var breedItemPrefab = LoadSupportPrefabComponent<BreedListItemView>(BREED_LIST_ITEM_PREFAB_NAME);

        BindServices();
        BindDomain();
        BindNavigation();
        BindPools(vfxPool, currencyFlyPrefab, breedItemPrefab, dogBreedsView);
        InjectRuntimeUi(runtimeUi);
    }

    private static List<T> LoadResources<T>(string folderName) where T : UnityEngine.Object
    {
        var resources = Resources.LoadAll(folderName);
        var result = new List<T>(resources.Length);

        for (var i = 0; i < resources.Length; i++)
        {
            if (resources[i] is T typedResource)
                result.Add(typedResource);
        }

        return result;
    }

    private static bool ShouldSkipInjectedUiType(Type type) =>
        typeof(AudioService).IsAssignableFrom(type) || !type.Name.EndsWith("View", StringComparison.Ordinal);

    private static bool ShouldBindRuntimeUiComponent(Component component) =>
        component.GetType().Assembly == PROJECT_ASSEMBLY && !ShouldSkipInjectedUiType(component.GetType());

    private static T GetRuntimeUiComponent<T>(IReadOnlyList<Component> instances) where T : Component
    {
        for (var i = 0; i < instances.Count; i++)
        {
            if (instances[i] is T typedInstance)
                return typedInstance;
        }

        throw new InvalidOperationException($"Runtime UI instance not registered: {typeof(T).Name}");
    }

    private static void ConfigureStartupPanels(IReadOnlyList<Component> runtimeUi)
    {
        for (var i = 0; i < runtimeUi.Count; i++)
        {
            switch (runtimeUi[i])
            {
                case ClickerView clickerView: clickerView.Show(); continue;
                case WeatherView weatherView: weatherView.Hide(); continue;
                case DogBreedsView dogBreedsView: dogBreedsView.Hide(); break;
            }
        }
    }

    private static RectTransform CreateVfxPool(Transform parent)
    {
        var vfxPool = (RectTransform)new GameObject("VFXPool", typeof(RectTransform)).transform;
        vfxPool.SetParent(parent, false);
        vfxPool.gameObject.SetActiveSafe(true);
        return vfxPool;
    }

    private static T LoadSupportPrefabComponent<T>(string prefabName) where T : Component
    {
        var prefab = Resources.Load<GameObject>($"{SUPPORT_UI_FOLDER}/{prefabName}");
        if (!prefab)
            throw new InvalidOperationException($"Prefab not found in Resources/{SUPPORT_UI_FOLDER}: {prefabName}");

        var component = prefab.GetComponent<T>();
        if (component)
            return component;

        throw new InvalidOperationException($"Component {typeof(T).Name} not found on prefab: {prefabName}");
    }

    private void BindServices()
    {
        Container.Inject(_audioService);
        Container.BindInterfacesAndSelfTo<RequestQueueService>().AsSingle();
        Container.Bind<AudioService>().FromInstance(_audioService).AsSingle();
        Container.Bind<IAudioService>().FromInstance(_audioService).AsSingle();
    }

    private void BindDomain()
    {
        Container.Bind<ClickerModel>().AsSingle();
        Container.Bind<WeatherModel>().AsSingle();
        Container.Bind<DogBreedsModel>().AsSingle();
        Container.Bind<DogBreedsRequestService>().AsSingle();
        Container.BindInterfacesAndSelfTo<ClickerPresenter>().AsSingle();
        Container.BindInterfacesAndSelfTo<WeatherPresenter>().AsSingle();
        Container.BindInterfacesAndSelfTo<DogBreedsPresenter>().AsSingle();
        Container.BindInterfacesAndSelfTo<TabBarPresenter>().AsSingle();
    }

    private void BindNavigation()
    {
        Container.BindInterfacesAndSelfTo<NavigationService>().AsSingle();
        Container.BindExecutionOrder<NavigationService>(100);
    }

    private void BindPools(RectTransform vfxPool, CurrencyFlyEffect currencyFlyPrefab, BreedListItemView breedItemPrefab, DogBreedsView dogBreedsView)
    {
        Container.BindMemoryPool<CurrencyFlyEffect, CurrencyFlyEffect.Pool>()
            .WithInitialSize(10)
            .FromComponentInNewPrefab(currencyFlyPrefab)
            .UnderTransform(vfxPool);

        Container.BindMemoryPool<BreedListItemView, BreedListItemView.Pool>()
            .WithInitialSize(5)
            .FromComponentInNewPrefab(breedItemPrefab)
            .UnderTransform(dogBreedsView.ListContainer);
    }

    private void InjectRuntimeUi(IReadOnlyList<Component> runtimeUi)
    {
        for (var i = 0; i < runtimeUi.Count; i++)
            Container.Inject(runtimeUi[i]);
    }
}
