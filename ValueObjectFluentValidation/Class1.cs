using ValueObjectFluentValidation.Single;


namespace ValueObjectFluentValidation
{
    //internal class GroupValidator<T1, T2> : IGroupValidator<T1, T2>
    //{
    //    private readonly ISinglePropertyValidatorBuilder<T1> _firstSinglePropertyValidator;
    //    private readonly ISinglePropertyValidatorBuilder<T2> _secondSinglePropertyValidator;

    //    public GroupValidator(
    //        ISinglePropertyValidatorBuilder<T1> firstSinglePropertyValidator,
    //        ISinglePropertyValidatorBuilder<T2> secondSinglePropertyValidator)
    //    {
    //        _firstSinglePropertyValidator = firstSinglePropertyValidator;
    //        _secondSinglePropertyValidator = secondSinglePropertyValidator;
    //    }

    //    public GroupResult<TValueObject> WhenValid<TValueObject>(Func<T1, T2, TValueObject> createValueObjectFunc)
    //    {
    //        var failuresByValidatorIndex = new Dictionary<int, IValidationFailure[]>();

    //        var validators = new object[] { _firstSinglePropertyValidator, _secondSinglePropertyValidator };
    //        var v1 = _firstSinglePropertyValidator.WhenValid(w => w);
    //        var v2 = _secondSinglePropertyValidator.WhenValid(w => w);

    //        if (!v1.TryGet(out var v1Value))
    //        {
    //            failuresByValidatorIndex.AddFailure(0, v1.Failures);
    //        }

    //        if (!v2.TryGet(out var v2Value))
    //        {
    //            failuresByValidatorIndex.AddFailure(1, v2.Failures);
    //        }

    //        if (failuresByValidatorIndex.Any())
    //        {
    //            return GroupResult<TValueObject>.Failure(failuresByValidatorIndex);
    //        }

    //        var valueObject = createValueObjectFunc.Invoke(v1Value!, v2Value!);
    //        return GroupResult<TValueObject>.Success(valueObject);
    //    }

    //    public Task<GroupResult<TValueObject>> WhenValidAsync<TValueObject>(Func<T1, T2, TValueObject> createValueObjectFunc)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class FailureNotSetException : Exception
    {
        public FailureNotSetException() :
            base($"Validator failed but did not return a valid {nameof(IValidationFailure)}")
        {
        }
    }

    //public interface IGroupValidator<out T1, out T2>
    //{
    //    GroupResult<TValueObject> WhenValid<TValueObject>(Func<T1, T2, TValueObject> createValueObjectFunc);
    //    Task<GroupResult<TValueObject>> WhenValidAsync<TValueObject>(Func<T1, T2, TValueObject> createValueObjectFunc);
    //}

    //public interface IGroupValidator<T1, T2, T3>
    //{
    //    GroupResult<TValueObject> WhenValid<TValueObject>(Func<T1, T2, T3, TValueObject> func);
    //}

    //public interface IGroupValidator<T1, T2, T3, T4>
    //{
    //    SingleResult<TValueObject> WhenValid<TValueObject>(Func<T1, T2, T3, T4, TValueObject> func);
    //}

    //public interface ISinglePropertyValidatorBuilder<TInitial>
    //{
    //    ISinglePropertyValidatorBuilder<TInitial> AddValidator(IValidator<TInitial> validator);

    //    public IValidator<TInitial, TValueObject> WhenValid<TValueObject>(
    //        Func<TInitial, TValueObject> createValueObjectFunc);
    //}

    //public interface IValidator<TInitial, TValueObject>
    //{
    //    SingleResult<TValueObject> Validate();
    //}
    
    public class NotNullValidator<T> : IValidator<T>
    {
        public void Validate(T value, IValidationFailureCollection context)
        {
            if (value is null)
            {
                context.AddFailure(new ValueNullFailure());
            }
        }
    }

    public class StringLengthValidator : IValidator<string>
    {
        private readonly int _minLength;
        private readonly int _maxLength;

        public StringLengthValidator(int minLength, int maxLength)
        {
            _minLength = minLength;
            _maxLength = maxLength;
        }

        public void Validate(string value, IValidationFailureCollection context)
        {
            if (value.Length < _minLength ||
                value.Length > _maxLength)
            {
                var failure = new StringLengthFailure(_minLength, _maxLength, value.Length);
                context.AddFailure(failure);
            }
        }
    }

    public class AsyncDelayedValidator<T> : IAsyncValidator<T>
    {
        private readonly int _delayMs;

        public AsyncDelayedValidator(int delayMs)
        {
            _delayMs = delayMs;
        }

        public async Task ValidateAsync(T value, IValidationFailureCollection context)
        {
            await Task.Delay(_delayMs);
        }
    }

    public class ValueNullFailure : IValidationFailure
    {
    }

    public record StringLengthFailure(int MinLength, int MaxLength, int ActualLength) : IValidationFailure
    {
    }

    public class InvalidResultException : Exception
    {
        public object Failure { get; }

        public InvalidResultException(object failure)
        {
            Failure = failure;
        }
    }

    //public class GroupResult<T>
    //{
    //    private readonly IDictionary<int, IValidationFailure[]> _validationFailures;
    //    private readonly T? _value;

    //    public T Value
    //    {
    //        get
    //        {
    //            if (!TryGet(out var value))
    //            {
    //                // TODO: Implement proper result..
    //                throw new Exception("err");
    //            }

    //            return value!;
    //        }
    //    }

    //    private GroupResult(T value)
    //    {
    //        _validationFailures = new Dictionary<int, IValidationFailure[]>();
    //        _value = value;
    //    }

    //    private GroupResult(IDictionary<int, IValidationFailure[]> validationFailures)
    //    {
    //        _validationFailures = validationFailures;
    //        _value = default;
    //    }

    //    public bool TryGet(out T? value)
    //    {
    //        value = default;
    //        if (_validationFailures.Any())
    //        {
    //            return false;
    //        }

    //        value = _value!;
    //        return true;
    //    }

    //    public static GroupResult<T> Success(T value)
    //    {
    //        return new GroupResult<T>(value);
    //    }

    //    public static GroupResult<T> Failure(IDictionary<int, IValidationFailure[]> validationFailures)
    //    {
    //        return new GroupResult<T>(validationFailures);
    //    }
    //}

    //public class ValidationResult<T>
    //{
    //    private readonly IDictionary<string, IEnumerable<IValidationFailure>> _validationFailures;
    //    private readonly T? _value;

    //    public T Value
    //    {
    //        get
    //        {
    //            if (!TryGet(out var value))
    //            {
    //                // TODO: Implement proper result..
    //                throw new Exception("err");
    //            }

    //            return value!;
    //        }
    //    }

    //    private ValidationResult(T value)
    //    {
    //        _validationFailures = new Dictionary<string, IEnumerable<IValidationFailure>>();
    //        _value = value;
    //    }

    //    private ValidationResult(IDictionary<string, IEnumerable<IValidationFailure>> validationFailures)
    //    {
    //        _validationFailures = validationFailures;
    //        _value = default;
    //    }

    //    public bool TryGet(out T? value)
    //    {
    //        value = default;
    //        if (_validationFailures.Any())
    //        {
    //            return false;
    //        }

    //        value = _value!;
    //        return true;
    //    }

    //    public static ValidationResult<T> Success(T value)
    //    {
    //        return new ValidationResult<T>(value);
    //    }

    //    public static ValidationResult<T> Failure(IDictionary<string, IEnumerable<IValidationFailure>> validationFailures)
    //    {
    //        return new ValidationResult<T>(validationFailures);
    //    }
    //}

    public static class ValidatorExtensions
    {
        public static ISinglePropertyValidatorBuilder<T> NotNull<T>(this ISinglePropertyValidatorBuilder<T?> singlePropertyValidatorBuilder)
        {
            return singlePropertyValidatorBuilder.AddValidator(new NotNullValidator<T>());
        }

        public static ISinglePropertyValidatorBuilder<string> Length(this ISinglePropertyValidatorBuilder<string> singlePropertyValidatorBuilder, int minLength, int maxLength)
        {
            return singlePropertyValidatorBuilder.AddValidator(new StringLengthValidator(minLength, maxLength));
        }

        public static ISinglePropertyValidatorBuilder<T> ValidAsync<T>(this ISinglePropertyValidatorBuilder<T> singlePropertyValidatorBuilder, int delayMs)
        {
            return singlePropertyValidatorBuilder.AddValidator(new AsyncDelayedValidator<T>(delayMs));
        }
    }

    //public static class RequestValidator
    //{
    //    public static IRequestValidatorBuilder<T> For<T>(T request)
    //    {
    //        return new RequestValidatorBuilder<T>(request);
    //    }
    //}

    //internal class RequestValidatorBuilder<T> : IRequestValidatorBuilder<T>
    //{
    //    private readonly T _request;

    //    public RequestValidatorBuilder(T request)
    //    {
    //        _request = request;
    //    }

    //    public IRequestGroupValidator<T1, T2> Group<T1, T2>(params IRule<T, T1>[] rules)
    //    {
    //        return new RequestGroupValidator<T, T1, T2>(_request, rules.Cast<IRule<T, object>>().ToArray());

    //    }

    //    public IRequestGroupValidator<T1, T2> Group<T1, T2>(IRule<T, T1> rule1, IRule<T, T2> rule2)
    //    {
    //        throw new NotImplementedException();
    //        //return new RequestGroupValidator<T, T1, T2>(_request, new IRule<T, object>[] { rule1, rule2 });
    //    }
    //}

    //internal class RequestGroupValidator<T, T1, T2> : IRequestGroupValidator<T1, T2>
    //{
    //    private readonly T _request;
    //    private readonly IRule<T, object>[] _rules;

    //    public RequestGroupValidator(T request, IRule<T, object>[] rules)
    //    {
    //        _request = request;
    //        _rules = rules;
    //        throw new NotImplementedException();
    //    }

    //    public ValidationResult<TValueObject> WhenValid<TValueObject>(Func<T1, T2, TValueObject> createValueObjectFunc)
    //        => Validate<TValueObject>(createValueObjectFunc);

    //    public Task<ValidationResult<TValueObject>> WhenValidAsync<TValueObject>(Func<T1, T2, TValueObject> createValueObjectFunc)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    private ValidationResult<TValueObject> Validate<TValueObject>(Delegate createValueObjectFunc)
    //    {
    //        throw new NotImplementedException();
    //        //var validationFailuresByPropertyName = new Dictionary<string, List<IValidationFailure>>();
    //        //foreach (var rule in _rules)
    //        //{
    //        //    var validationFailures = rule.Validate(_request);
    //        //    foreach (var validationFailure in validationFailures)
    //        //    {
    //        //        if (!validationFailuresByPropertyName.TryGetValue(validationFailure.Key, out var failureList))
    //        //        {
    //        //            failureList = new List<IValidationFailure>();
    //        //        }

    //        //        failureList.AddRange(validationFailure.Value);
    //        //    }
    //        //}

    //        //if (validationFailuresByPropertyName.Any())
    //        //{
    //        //    return ValidationResult<TValueObject>.Failure(validationFailuresByPropertyName());
    //        //}

    //        //var valueObject = createValueObjectFunc.inv
    //        //rule1.Delegate.DynamicInvoke()
    //        //throw new NotImplementedException();
    //    }
    //}

    //public interface IRequestValidatorBuilder<T>
    //{
    //    IRequestGroupValidator<T1, T2> Group<T1, T2>(
    //        IRule<T, T1> rule1,
    //        IRule<T, T2> rule2);
    //}

    //public interface IRequestGroupValidator<T1, T2>
    //{
    //    ValidationResult<TValueObject> WhenValid<TValueObject>(Func<T1, T2, TValueObject> createValueObjectFunc);
    //    Task<ValidationResult<TValueObject>> WhenValidAsync<TValueObject>(Func<T1, T2, TValueObject> createValueObjectFunc);
    //}

    //public interface IRequestValidator<in T, TValid>
    //{
    //    ValidationResult<TValid> Validate(T value);
    //}

    //public interface ISingleRule<T, TValueObject> : IRule<T, TValueObject>
    //{
    //    // TODO: Implement?
    //    //IRule<T, TValid> WithData(string key, object data);
    //}

    //public interface IRule<T, TValueObject>
    //{
    //    IDictionary<string, IEnumerable<IValidationFailure>> Validate(T request);
    //}



    //public interface IGroupRule<T, TValueObject> : IRule<T, TValueObject>
    //{

    //}

    //public abstract class AbstractRequestValidator<T, TValid> : IRequestValidator<T, TValid>
    //{
    //    public abstract ValidationResult<TValid> Validate(T value);

    //    public ISingleRule<T, TValueObject> Rule<TProperty, TValueObject>(
    //        Expression<Func<T, TProperty>> selector,
    //        Func<TProperty, SingleResult<TValueObject>> valueObjectFunc)
    //    {
    //        throw new NotImplementedException();
    //        //var propertyName = FromExpression(selector);
    //        //var value = selector.Compile().Invoke
    //    }

    //    public IGroupRule<T, TValueObject> Rule<TProperty1, TProperty2, TValueObject>(
    //        Func<T, TProperty1> prop1Selector,
    //        Func<T, TProperty2> prop2Selector,
    //        Func<TProperty1, TProperty2, GroupResult<TValueObject>> valueObjectFunc)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    private static string FromExpression(Expression expression)
    //    {
    //        if (expression is not LambdaExpression lambdaExpression)
    //        {
    //            throw new InvalidOperationException();
    //        }

    //        var memberNames = new Stack<string>();

    //        var getMemberExp = new Func<Expression?, MemberExpression?>(toUnwrap =>
    //        {
    //            if (toUnwrap is UnaryExpression unaryExpression)
    //            {
    //                return unaryExpression.Operand as MemberExpression;
    //            }

    //            return toUnwrap as MemberExpression;
    //        });

    //        var memberExp = getMemberExp(lambdaExpression.Body);

    //        while (memberExp is not null)
    //        {
    //            memberNames.Push(memberExp.Member.Name);
    //            memberExp = getMemberExp(memberExp.Expression);
    //        }

    //        return memberNames.Peek();
    //    }
    //}
}
