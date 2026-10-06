namespace Note.Application.Wrapper;

public abstract class Output
{
    protected Output(int code)
    {
        Code = code;
    }

    public int Code { get; set; }
}

public class OutputOk<T> : Output
{
    public OutputOk(int code, T value) : base(code)
    {
        Value = value;
    }

    public T Value { get; set; }
}

public class OutputError : Output
{
    public OutputError(int code, string error, string errorDescription) : base(code)
    {
        Error = error;
        ErrorDescription = errorDescription;
    }

    public string Error { get; set; }
    public string ErrorDescription { get; set; }
}

public class OutputErrorValidation : Output
{
    public OutputErrorValidation(int code, Dictionary<string, string[]> errors) : base(code)
    {
        Errors = errors;
    }

    public Dictionary<string, string[]> Errors { get; set; }
}