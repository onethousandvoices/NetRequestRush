using System.Collections.Generic;

namespace Features.DogBreeds
{
    public sealed class DogBreedsModel
    {
        private readonly List<BreedData> _breeds = new();

        public IReadOnlyList<BreedData> Breeds => _breeds;

        public void SetBreeds(List<BreedData> breeds)
        {
            _breeds.Clear();
            _breeds.AddRange(breeds);
        }
    }
}
