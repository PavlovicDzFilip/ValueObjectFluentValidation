namespace ValueObjectFluentValidation.Util;

public class Result<TSuccess, TFailure>
{
    private readonly TSuccess? _success;
    private readonly TFailure? _failure;
    private readonly bool _isSuccessful;

    public Result(TSuccess success)
    {
        _success = success;
        _isSuccessful = true;
    }

    public Result(TFailure failure)
    {
        _failure = failure;
        _isSuccessful = false;
    }

    public static implicit operator Result<TSuccess, TFailure>(TSuccess success) => new(success);

    public static implicit operator Result<TSuccess, TFailure>(TFailure failure) => new(failure);

    public bool TryGet(out TSuccess? value)
    {
        value = default;
        if (!_isSuccessful)
        {
            return false;
        }

        value = _success;
        return true;
    }

    public TSuccess Success
    {
        get
        {
            if (!TryGet(out var value))
            {
                throw new InvalidOperationException();
            }

            return value!;
        }
    }

    public TFailure Failure
    {
        get
        {
            if (_isSuccessful)
            {
                throw new InvalidOperationException();
            }

            return _failure!;
        }
    }
}