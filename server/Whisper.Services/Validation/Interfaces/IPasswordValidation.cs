namespace Whisper.Services.Validation.Interfaces
{
    public interface IPasswordValidation
    {
        bool IsStrong(string password);
    }
}
