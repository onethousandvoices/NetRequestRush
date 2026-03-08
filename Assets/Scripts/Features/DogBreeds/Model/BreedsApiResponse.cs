using System;

namespace Features.DogBreeds
{
    [Serializable]
    public class BreedsApiResponse
    {
        public BreedApiData[] data;
    }

    [Serializable]
    public class BreedApiDetailsResponse
    {
        public BreedApiData data;
    }

    [Serializable]
    public class BreedApiData
    {
        public string id;
        public BreedAttributes attributes;
    }

    [Serializable]
    public class BreedAttributes
    {
        public string name;
        public string description;
    }
}
