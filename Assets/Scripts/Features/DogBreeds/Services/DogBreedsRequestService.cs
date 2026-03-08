using System;
using System.Collections.Generic;
using System.Threading;
using Services;
using UnityEngine;
using Zenject;

namespace Features.DogBreeds
{
    public sealed class DogBreedsRequestService
    {
        [Inject] private DogBreedsConfig _config;
        [Inject] private IRequestQueueService _requestQueue;

        public void CancelBreedsList() => _requestQueue.Cancel(_config.BreedsListRequestTag);
        public void CancelBreedDetails() => _requestQueue.Cancel(_config.BreedDetailsRequestTag);

        public async Awaitable<List<BreedData>> LoadBreeds(CancellationToken ct)
        {
            var result = await _requestQueue.EnqueueText(_config.BuildBreedsListUrl(), _config.BreedsListRequestTag, ct);
            if (!result.IsSuccess || string.IsNullOrEmpty(result.Data))
                return null;

            var response = JsonUtility.FromJson<BreedsApiResponse>(result.Data);
            if (response?.data == null)
                return null;

            var count = Math.Min(response.data.Length, _config.MaxDisplayCount);
            var breeds = new List<BreedData>(count);
            for (var i = 0; i < count; i++)
            {
                var breed = response.data[i];
                if (breed?.attributes == null || string.IsNullOrWhiteSpace(breed.id) || string.IsNullOrWhiteSpace(breed.attributes.name))
                    continue;

                breeds.Add(new(breed.id, breed.attributes.name, breed.attributes.description));
            }

            return breeds.Count > 0 ? breeds : null;
        }

        public async Awaitable<string> LoadBreedDescription(string breedId, CancellationToken ct)
        {
            var result = await _requestQueue.EnqueueText(_config.BuildBreedDetailsUrl(breedId), _config.BreedDetailsRequestTag, ct);
            if (!result.IsSuccess || string.IsNullOrEmpty(result.Data))
                return null;

            var response = JsonUtility.FromJson<BreedApiDetailsResponse>(result.Data);
            var attributes = response?.data?.attributes;
            if (attributes == null)
                return null;

            var description = attributes.description;
            return string.IsNullOrWhiteSpace(description) ? null : description;
        }
    }
}
