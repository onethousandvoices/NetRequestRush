namespace Services
{
    public readonly record struct TextRequestResult(
        bool IsSuccess,
        string Data,
        string Error
    );

    public readonly record struct BinaryRequestResult(
        bool IsSuccess,
        byte[] Data,
        string Error
    );
}
