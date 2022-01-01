namespace ValueObjectFluentValidation
{
    public class Validator
    {
        public static IValidatorBuilderInitial<T> For<T>(T value)
        {
            return new ValidatorBuilderInitial<T>(value);
        }

        public static IGroupValidator<T1, T2> Group<T1, T2>(
            IValidatorBuilder<T1> firstValidator,
            IValidatorBuilder<T2> secondValidator)
        {
            return new GroupValidator<T1, T2>(firstValidator, secondValidator);
        }

        private class ValidatorBuilderInitial<T> :
            ValidationBuilder<T>,
            IValidatorBuilderInitial<T>
        {
            public ValidatorBuilderInitial(T value) : base(value)
            {
            }

            public IValidatorBuilder<TTransformed> Transform<TTransformed>(Func<T, TTransformed> transformFunc)
            {
                var transformed = transformFunc(Value);
                return new ValidationBuilder<TTransformed>(transformed);
            }
        }
    }

    internal class GroupValidator<T1, T2> : IGroupValidator<T1, T2>
    {
        private readonly IValidatorBuilder<T1> _firstValidator;
        private readonly IValidatorBuilder<T2> _secondValidator;

        public GroupValidator(
            IValidatorBuilder<T1> firstValidator,
            IValidatorBuilder<T2> secondValidator)
        {
            _firstValidator = firstValidator;
            _secondValidator = secondValidator;
        }

        public GroupResult<TValueObject> WhenValid<TValueObject>(Func<T1, T2, TValueObject> createValueObjectFunc)
        {
            var failuresByValidatorIndex = new Dictionary<int, IValidationFailure[]>();

            var validators = new object[] { _firstValidator, _secondValidator };
            var v1 = _firstValidator.WhenValid(w => w);
            var v2 = _secondValidator.WhenValid(w => w);

            if (!v1.TryGet(out var v1Value))
            {
                failuresByValidatorIndex.Add(0, v1.Failures);
            }

            if (!v2.TryGet(out var v2Value))
            {
                failuresByValidatorIndex.Add(1, v2.Failures);
            }

            if (failuresByValidatorIndex.Any())
            {
                return GroupResult<TValueObject>.Failure(failuresByValidatorIndex);
            }

            var valueObject = createValueObjectFunc.Invoke(v1Value!, v2Value!);
            return GroupResult<TValueObject>.Success(valueObject);
        }

        public Task<GroupResult<TValueObject>> WhenValidAsync<TValueObject>(Func<T1, T2, TValueObject> createValueObjectFunc)
        {
            throw new NotImplementedException();
        }
    }

    public class FailureNotSetException : Exception
    {
        public FailureNotSetException() :
            base($"Validator failed but did not return a valid {nameof(IValidationFailure)}")
        {
        }
    }

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
        Result<TValueObject> WhenValid<TValueObject>(Func<T1, T2, T3, T4, TValueObject> func);
    }

    internal class ValidationBuilder<T> : IValidatorBuilder<T>
    {
        private protected readonly T Value;
        private readonly Queue<IPropertyValidatorInternal<T>> _validators;

        public ValidationBuilder(T value)
        {
            Value = value;
            _validators = new Queue<IPropertyValidatorInternal<T>>();
        }

        public IValidatorBuilder<T> AddValidator(IPropertyValidator<T> propertyValidator)
        {
            _validators.Enqueue(new PropertyValidatorWrapper<T>(propertyValidator));
            return this;
        }

        public IValidatorBuilder<T> AddValidator(IAsyncPropertyValidator<T> propertyValidator)
        {
            _validators.Enqueue(new PropertyValidatorWrapper<T>(propertyValidator));
            return this;
        }

        public Result<TValueObject> WhenValid<TValueObject>(
            Func<T, TValueObject> createValueObjectFunc)
        {
            var context = new ValidationContext();

            while (_validators.TryDequeue(out var validator))
            {
                validator.Validate(Value, context);
                // TODO: Introduce cascade policy, and stop reading public Failures array
                if (context.Failures.Any())
                {
                    return Result<TValueObject>.Failure(context.Failures.ToArray());
                }
            }

            var valueObject = createValueObjectFunc(Value);
            return Result<TValueObject>.Success(valueObject);
        }

        public async Task<Result<TValueObject>> WhenValidAsync<TValueObject>(Func<T, TValueObject> createValueObjectFunc)
        {
            var context = new ValidationContext();

            while (_validators.TryDequeue(out var validator))
            {
                await validator.ValidateAsync(Value, context);
                // TODO: Introduce cascade policy, and stop reading public Failures array
                if (context.Failures.Any())
                {
                    return Result<TValueObject>.Failure(context.Failures.ToArray());
                }
            }

            var valueObject = createValueObjectFunc(Value);
            return Result<TValueObject>.Success(valueObject);
        }
    }

    public interface IValidationFailure
    {

    }

    public interface IValidatorBuilder<out T>
    {
        IValidatorBuilder<T> AddValidator(IPropertyValidator<T> propertyValidator);
        IValidatorBuilder<T> AddValidator(IAsyncPropertyValidator<T> propertyValidator);

        public Result<TValueObject> WhenValid<TValueObject>(
            Func<T, TValueObject> createValueObjectFunc);

        public Task<Result<TValueObject>> WhenValidAsync<TValueObject>(
            Func<T, TValueObject> createValueObjectFunc);
    }

    //public interface IValidatorBuilder<TInitial>
    //{
    //    IValidatorBuilder<TInitial> AddValidator(IPropertyValidator<TInitial> propertyValidator);

    //    public IValidator<TInitial, TValueObject> WhenValid<TValueObject>(
    //        Func<TInitial, TValueObject> createValueObjectFunc);
    //}

    public interface IValidator<TInitial, TValueObject>
    {
        Result<TValueObject> Validate();
    }

    public interface IValidatorBuilderInitial<T> : IValidatorBuilder<T>
    {
        IValidatorBuilder<TTransformed> Transform<TTransformed>(Func<T, TTransformed> transformFunc);
    }

    public interface IPropertyValidator<in T>
    {
        void Validate(T value, IValidationContext context);
    }

    public interface IAsyncPropertyValidator<in T>
    {
        Task ValidateAsync(T value, IValidationContext context);
    }

    internal interface IPropertyValidatorInternal<in T> :
        IPropertyValidator<T>,
        IAsyncPropertyValidator<T>
    {
    }

    internal record PropertyValidatorWrapper<T> : IPropertyValidatorInternal<T>
    {
        private readonly IAsyncPropertyValidator<T>? _asyncPropertyValidator;
        private readonly IPropertyValidator<T>? _propertyValidator;

        public PropertyValidatorWrapper(IPropertyValidator<T> propertyValidator)
        {
            _propertyValidator = propertyValidator;
        }

        public PropertyValidatorWrapper(IAsyncPropertyValidator<T> asyncPropertyValidator)
        {
            _asyncPropertyValidator = asyncPropertyValidator;
        }

        public void Validate(T value, IValidationContext context)
        {
            if (_propertyValidator is not null)
            {
                _propertyValidator.Validate(value, context);
                return;
            }

            if (_asyncPropertyValidator is not null)
            {
                _asyncPropertyValidator
                    .ValidateAsync(value, context)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
                return;
            }

            throw new NotImplementedException();
        }

        public async Task ValidateAsync(T value, IValidationContext context)
        {
            if (_propertyValidator is not null)
            {
                _propertyValidator.Validate(value, context);
                return;
            }

            if (_asyncPropertyValidator is not null)
            {
                await _asyncPropertyValidator.ValidateAsync(value, context);
                return;
            }

            throw new NotImplementedException();
        }
    }

    public interface IValidationContext
    {
        void AddFailure(IValidationFailure failure);
    }

    internal class ValidationContext : IValidationContext
    {
        public readonly ICollection<IValidationFailure> Failures = new List<IValidationFailure>();

        public void AddFailure(IValidationFailure failure)
            => Failures.Add(failure);
    }

    public class NotNullValidator<T> : IPropertyValidator<T>
    {
        public void Validate(T value, IValidationContext context)
        {
            if (value is null)
            {
                context.AddFailure(new ValueNullFailure());
            }
        }
    }

    public class StringLengthValidator : IPropertyValidator<string>
    {
        private readonly int _minLength;
        private readonly int _maxLength;

        public StringLengthValidator(int minLength, int maxLength)
        {
            _minLength = minLength;
            _maxLength = maxLength;
        }

        public void Validate(string value, IValidationContext context)
        {
            if (value.Length < _minLength ||
                value.Length > _maxLength)
            {
                var failure = new StringLengthFailure(_minLength, _maxLength, value.Length);
                context.AddFailure(failure);
            }
        }
    }

    public class AsyncDelayedValidator<T> : IAsyncPropertyValidator<T>
    {
        private readonly int _delayMs;

        public AsyncDelayedValidator(int delayMs)
        {
            _delayMs = delayMs;
        }

        public async Task ValidateAsync(T value, IValidationContext context)
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

    public class Result<T>
    {
        public IValidationFailure[] Failures { get; }
        private readonly T? _value;
        public T Value
        {
            get
            {
                if (!TryGet(out var value))
                {
                    // TODO: Implement proper result..
                    throw new Exception("err");
                }

                return value!;
            }
        }

        private Result(T value)
        {
            _value = value;
            Failures = Array.Empty<IValidationFailure>();
        }

        private Result(IValidationFailure[] failures)
        {
            failures = failures ?? throw new ArgumentNullException(nameof(failures));
            if (!failures.Any()) throw new ArgumentException("Failures collection is empty");

            Failures = failures;
            _value = default;
        }

        public bool TryGet(out T? value)
        {
            value = default;
            if (Failures.Any())
            {
                return false;
            }

            value = _value!;
            return true;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(value);
        }

        public static Result<T> Failure(IValidationFailure[] failures)
        {
            return new Result<T>(failures);
        }
    }

    public class GroupResult<T>
    {
        private readonly IDictionary<int, IValidationFailure[]> _validationFailures;
        private readonly T? _value;

        public T Value
        {
            get
            {
                if (!TryGet(out var value))
                {
                    // TODO: Implement proper result..
                    throw new Exception("err");
                }

                return value!;
            }
        }

        private GroupResult(T value)
        {
            _validationFailures = new Dictionary<int, IValidationFailure[]>();
            _value = value;
        }

        private GroupResult(IDictionary<int, IValidationFailure[]> validationFailures)
        {
            _validationFailures = validationFailures;
            _value = default;
        }

        public bool TryGet(out T? value)
        {
            value = default;
            if (_validationFailures.Any())
            {
                return false;
            }

            value = _value!;
            return true;
        }

        public static GroupResult<T> Success(T value)
        {
            return new GroupResult<T>(value);
        }

        public static GroupResult<T> Failure(IDictionary<int, IValidationFailure[]> validationFailures)
        {
            return new GroupResult<T>(validationFailures);
        }
    }

    public static class ValidatorExtensions
    {
        public static IValidatorBuilder<T> NotNull<T>(this IValidatorBuilder<T?> validatorBuilder)
        {
            return validatorBuilder.AddValidator(new NotNullValidator<T>());
        }

        public static IValidatorBuilder<string> Length(this IValidatorBuilder<string> validatorBuilder, int minLength, int maxLength)
        {
            return validatorBuilder.AddValidator(new StringLengthValidator(minLength, maxLength));
        }

        public static IValidatorBuilder<T> ValidAsync<T>(this IValidatorBuilder<T> validatorBuilder, int delayMs)
        {
            return validatorBuilder.AddValidator(new AsyncDelayedValidator<T>(delayMs));
        }
    }

    public static class RequestValidator
    {
        public static IRequestValidatorBuilder<T> For<T>(T request)
        {
            throw new NotImplementedException();
        }
    }

    public interface IRequestValidatorBuilder<T>
    {
        Result<TValidRequest> WhenValid<T1, T2, TValidRequest>(Func<T1, T2, TValidRequest> func);

        IGroupValidator<T1, T2> Group<T1, T2>(
            IRule<T, T1> rule1,
            IRule<T, T2> rule2);

        IGroupValidator<T1, T2, T3> Group<T1, T2, T3>(
            IRule<T, T1> rule1,
            IRule<T, T2> rule2,
            IRule<T, T3> rule3);

        IGroupValidator<T1, T2, T3, T4> Group<T1, T2, T3, T4>(
            IRule<T, T1> rule1,
            IRule<T, T2> rule2,
            IRule<T, T3> rule3,
            IRule<T, T4> rule4);
    }

    public interface IRequestValidator<T, TValid>
    {
        Result<TValid> Validate(T value);
    }

    public interface ISingleRule<T, TValid> : IRule<T, TValid>
    {
        IRule<T, TValid> WithData(string key, object data);
    }

    public interface IRule<T, TValid>
    {

    }

    public interface IGroupRule<T, TValid> : IRule<T, TValid>
    {

    }

    public abstract class AbstractRequestValidator<T, TValid> : IRequestValidator<T, TValid>
    {
        public abstract Result<TValid> Validate(T value);

        public ISingleRule<T, TValueObject> Rule<TProperty, TValueObject>(
            Func<T, TProperty> func,
            Func<TProperty, Result<TValueObject>> valueObjectFunc)
        {
            throw new NotImplementedException();
        }

        public IGroupRule<T, TValueObject> Rule<TProperty1, TProperty2, TValueObject>(
            Func<T, TProperty1> prop1Selector,
            Func<T, TProperty2> prop2Selector,
            Func<TProperty1, TProperty2, Result<TValueObject>> valueObjectFunc)
        {
            throw new NotImplementedException();
        }
    }
}
