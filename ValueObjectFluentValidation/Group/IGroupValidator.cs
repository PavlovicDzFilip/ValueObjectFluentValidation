namespace ValueObjectFluentValidation.Group;

public interface IGroupValidator<out T1, out T2>
{
    GroupResult<TValueObject> WhenValid<TValueObject>(Func<T1, T2, TValueObject> createValueObjectFunc);
    Task<GroupResult<TValueObject>> WhenValidAsync<TValueObject>(Func<T1, T2, TValueObject> createValueObjectFunc);
}

public interface IGroupValidator<T1, T2, T3>
{
    GroupResult<TValueObject> WhenValid<TValueObject>(Func<T1, T2, T3, TValueObject> func);
}

public interface IGroupValidator<T1, T2, T3, T4>
{
    GroupResult<TValueObject> WhenValid<TValueObject>(Func<T1, T2, T3, T4, TValueObject> func);
}