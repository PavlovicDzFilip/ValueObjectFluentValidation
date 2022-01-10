namespace ValueObjectFluentValidation.Util;

public class Either<TLeft, TRight>
{
    private readonly TLeft? _left;
    private readonly TRight? _right;
    private readonly bool _isLeft;

    public Either(TLeft left)
    {
        _left = left;
        _isLeft = true;
    }

    public Either(TRight right)
    {
        _right = right;
        _isLeft = false;
    }

    public static implicit operator Either<TLeft, TRight>(TLeft left) => new(left);

    public static implicit operator Either<TLeft, TRight>(TRight right) => new(right);

    public void WhenLeft(Action<TLeft> action)
    {
        if (_isLeft)
        {
            action(_left!);
        }
    }

    public void WhenRight(Action<TRight> action)
    {
        if (!_isLeft)
        {
            action(_right!);
        }
    }

    public async Task WhenLeftAsync(Func<TLeft, Task> action)
    {
        if (_isLeft)
        {
            await action(_left!);
        }
    }

    public async Task WhenRightAsync(Func<TRight, Task> action)
    {
        if (!_isLeft)
        {
            await action(_right!);
        }
    }
}