namespace Note.Application.Validator.Abstraction;

public interface IValidator<in T>
{
    Dictionary<string, string[]> Validate(T input);
}