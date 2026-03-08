using UnityEngine;

namespace Features.DogBreeds
{
    [CreateAssetMenu(fileName = "DogBreedsConfig", menuName = "Configs/DogBreedsConfig")]
    public sealed class DogBreedsConfig : ScriptableObject
    {
        private const string DEFAULT_BREED_DETAILS_URL_TEMPLATE = "https://dogapi.dog/api/v2/breeds/{0}";

        [SerializeField] private string _breedsListUrlTemplate = "https://dogapi.dog/api/v2/breeds?page[number]=1&page[size]={0}";
        [SerializeField] private string _breedDetailsUrlTemplate = DEFAULT_BREED_DETAILS_URL_TEMPLATE;
        [SerializeField] private int _maxDisplayCount = 10;

        public string BreedsListRequestTag => "breeds_list";
        public string BreedDetailsRequestTag => "breed_details";
        public int MaxDisplayCount => _maxDisplayCount;
        public string BuildBreedsListUrl() => string.Format(_breedsListUrlTemplate, _maxDisplayCount);
        public string BuildBreedDetailsUrl(string id) => string.Format(GetBreedDetailsUrlTemplate(), id);

        private string GetBreedDetailsUrlTemplate() =>
            string.IsNullOrWhiteSpace(_breedDetailsUrlTemplate) || _breedDetailsUrlTemplate.Contains("/facts?filter[breed]=")
                ? DEFAULT_BREED_DETAILS_URL_TEMPLATE
                : _breedDetailsUrlTemplate;
    }
}
