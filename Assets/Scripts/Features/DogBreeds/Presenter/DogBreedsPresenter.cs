using System;
using System.Threading;
using Core;
using Services;
using UnityEngine;
using Zenject;

namespace Features.DogBreeds
{
    public sealed class DogBreedsPresenter : IPresenter, IInitializable, IDisposable
    {
        [Inject] private DogBreedsView _view;
        [Inject] private DogBreedsModel _model;
        [Inject] private DogBreedsRequestService _requestService;
        [Inject] private INavigationService _navigation;

        private QueuedRequestFlow _listFlow;
        private QueuedRequestFlow _detailFlow;
        private int _loadingIndex = -1;
        private int _loadingRequestVersion = -1;
        private bool _isActive;
        private bool _disposed;

        public void Initialize()
        {
            _listFlow = new(_requestService.CancelBreedsList, _view.ShowLoading, _view.HideLoading);
            _detailFlow = new(_requestService.CancelBreedDetails);
            _navigation.Register(TabType.DogBreeds, this);
            _view.BreedClicked += OnBreedClicked;
            _view.PopupClosed += OnPopupClosed;
        }

        public void Dispose()
        {
            _disposed = true;
            _isActive = false;
            if (_view)
            {
                _view.BreedClicked -= OnBreedClicked;
                _view.PopupClosed -= OnPopupClosed;
            }

            CancelListRequest(false);
            CancelDetailRequest(false);
        }

        public void Activate()
        {
            _isActive = true;
            _view.Show();
            _view.HidePopup();
            _view.HideLoading();
            _view.ResetItemLoading();

            if (_model.Breeds.Count > 0)
                _view.PopulateList(_model.Breeds);

            BeginListRequest();
        }

        public void Deactivate()
        {
            if (_disposed)
                return;

            _isActive = false;
            _view.HidePopup();
            _view.Hide();
            CancelListRequest(true);
            CancelDetailRequest(true);
        }

        private void BeginListRequest()
        {
            _listFlow.StartNewSession(out var token, out var version);
            LoadBreedsList(token, version).Forget();
        }

        private void BeginDetailRequest(int index)
        {
            _view.HidePopup();
            CancelDetailRequest(true);
            _loadingIndex = index;
            _detailFlow.StartNewSession(out var token, out var version);
            _loadingRequestVersion = version;
            LoadBreedDescription(index, token, version).Forget();
        }

        private async Awaitable LoadBreedsList(CancellationToken ct, int requestVersion)
        {
            _listFlow.BeginTrackedRequest();

            try
            {
                var breeds = await _requestService.LoadBreeds(ct);
                if (!IsListRequestCurrent(ct, requestVersion) || breeds == null)
                    return;

                _model.SetBreeds(breeds);
                _view.PopulateList(_model.Breeds);
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                if (IsListRequestCurrent(ct, requestVersion))
                    Debug.LogException(e);
            }
            finally
            {
                _listFlow.EndTrackedRequest(requestVersion);
            }
        }

        private void OnBreedClicked(int index)
        {
            if (index < 0 || index >= _model.Breeds.Count)
                return;

            BeginDetailRequest(index);
        }

        private async Awaitable LoadBreedDescription(int index, CancellationToken ct, int requestVersion)
        {
            var breed = _model.Breeds[index];
            _view.ShowItemLoading(index);

            try
            {
                var description = await _requestService.LoadBreedDescription(breed.Id, ct);
                description = string.IsNullOrWhiteSpace(description) ? breed.Description : description;
                if (!IsDetailRequestCurrent(ct, requestVersion) || string.IsNullOrWhiteSpace(description))
                    return;

                _view.ShowPopup(breed.Name, description);
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                if (IsDetailRequestCurrent(ct, requestVersion))
                    Debug.LogException(e);
            }
            finally
            {
                CompleteDetailLoading(index, requestVersion);
            }
        }

        private bool IsListRequestCurrent(CancellationToken ct, int requestVersion) => _isActive && _listFlow.IsCurrent(ct, requestVersion);
        private bool IsDetailRequestCurrent(CancellationToken ct, int requestVersion) => _isActive && _detailFlow.IsCurrent(ct, requestVersion);
        private void OnPopupClosed() => _view.HidePopup();

        private void CancelListRequest(bool updateView)
        {
            _listFlow.Cancel(updateView);
        }

        private void CancelDetailRequest(bool updateView)
        {
            _detailFlow.Cancel(false);
            if (updateView)
                ResetDetailLoading();
            else
                ClearDetailLoadingState();
        }

        private void CompleteDetailLoading(int index, int requestVersion)
        {
            if (_disposed || !_isActive || requestVersion != _loadingRequestVersion || !_view)
                return;

            _view.HideItemLoading(index);
            _loadingIndex = -1;
            _loadingRequestVersion = -1;
        }

        private void ResetDetailLoading()
        {
            if (_loadingIndex >= 0)
                _view.HideItemLoading(_loadingIndex);

            ClearDetailLoadingState();
        }

        private void ClearDetailLoadingState()
        {
            _loadingIndex = -1;
            _loadingRequestVersion = -1;
        }
    }
}
