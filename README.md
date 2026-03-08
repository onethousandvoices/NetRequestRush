# NetRequestRush

A Unity 6 tabbed UI project that combines a clicker loop with serialized external API requests, showing how multiple gameplay panels can share one request queue, runtime-loaded UI composition and a centralized DI setup.

## Overview

NetRequestRush is a compact Unity 6 application built around three tabs: a clicker panel, a live weather panel and a dog breeds browser. Runtime UI is loaded from `Resources` at startup through Zenject, shared services handle navigation, audio and request orchestration, and feature presenters keep view logic separate from state and async flows.

All external data goes through a single `RequestQueueService` based on Unity `Awaitable` and `UnityWebRequest`, so forecast polling, icon downloads, breed list loading and breed details are serialized, cancellable by tag and safe to stop when the active tab changes. Project tuning is config-driven through `ScriptableObject` assets under `Resources/Configs`.

## Key Features

- **Three feature tabs** - Clicker, Weather and Dog Breeds run inside one runtime UI shell driven by `NavigationService`
- **Serialized network requests** - all external GET requests pass through `RequestQueueService` with tagged cancellation and session-aware request flows
- **Clicker gameplay loop** - energy-gated clicks, passive auto collect, regeneration, audio feedback and pooled currency fly VFX
- **Live weather panel** - polls `weather.gov`, caches the latest forecast, loads condition icons on demand and cancels stale async work
- **Dog breeds browser** - loads a configured breed list from `dogapi.dog`, reuses pooled list items and opens detail popups per selected breed
- **Config-driven startup** - `ScriptableObject` configs, `Resources`-loaded prefabs and Zenject bindings keep startup wiring centralized in `MainSceneInstaller`

## Tech Stack

| Technology | Role |
|---|---|
| Unity 6 | Engine |
| URP | Rendering pipeline |
| Zenject | Dependency injection and composition root |
| Unity Awaitable | Async orchestration and cancellation-aware flows |
| UnityWebRequest | HTTP text and binary downloads |
| DOTween | UI punch, popup and fly animations |
| TextMeshPro | Runtime UI text |

## Architecture

```
SampleScene
    |
    v
MainSceneInstaller (MonoInstaller, composition root)
    |
    +-- NavigationService      (tab activation and presenter routing)
    +-- RequestQueueService    (serialized GET queue, tagged cancellation)
    +-- AudioService           (UI sound playback)
    |
    +-- ClickerPresenter
    |       +-- ClickerModel
    |       +-- ClickerView
    |       +-- CurrencyFlyEffect.Pool
    |
    +-- WeatherPresenter
    |       +-- WeatherModel
    |       +-- WeatherView
    |
    +-- DogBreedsPresenter
            +-- DogBreedsModel
            +-- DogBreedsRequestService
            +-- DogBreedsView
            +-- BreedListItemView.Pool
```

Runtime startup flow:

1. `MainSceneInstaller` loads `ScriptableObject` configs from `Resources/Configs`
2. Startup UI prefabs are instantiated under the main canvas and injected through Zenject
3. `NavigationService` activates the default `Clicker` tab
4. Feature presenters manage their own request sessions, cancellation and view state
5. Shared services keep network, audio and tab switching behavior centralized

## Running

1. Open `Assets/Scenes/SampleScene.unity`
2. Enter Play Mode in Unity `6000.0.59f2`
3. Switch between Clicker, Weather and Dog Breeds from the tab bar
