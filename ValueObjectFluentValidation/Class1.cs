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
            throw new NotImplementedException();
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

    public class FailureNotSetException : Exception
    {
        public FailureNotSetException() :
            base($"Validator failed but did not return a valid {nameof(IValidationFailure)}")
        {
        }
    }

    public interface IGroupValidator<T1, T2>
    {
        Result<TValueObject> WhenValid<TValueObject>(Func<T1, T2, TValueObject> func);
    }

    public interface IGroupValidator<T1, T2, T3>
    {
        Result<TValueObject> WhenValid<TValueObject>(Func<T1, T2, T3, TValueObject> func);
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
            while (_validators.TryDequeue(out var validator))
            {
                var validationResult = validator.Validate(Value);
                if (!validationResult.IsValid)
                {
                    return Result<TValueObject>.Failure(validationResult.Error!);
                }
            }

            var valueObject = createValueObjectFunc(Value);
            return Result<TValueObject>.Success(valueObject);
        }

        public async Task<Result<TValueObject>> WhenValidAsync<TValueObject>(Func<T, TValueObject> createValueObjectFunc)
        {
            while (_validators.TryDequeue(out var validator))
            {
                var validationResult = await validator.ValidateAsync(Value);
                if (!validationResult.IsValid)
                {
                    return Result<TValueObject>.Failure(validationResult.Error!);
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
        PropertyValidationResult Validate(T value);
    }

    public interface IAsyncPropertyValidator<in T>
    {
        Task<PropertyValidationResult> ValidateAsync(T value);
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

        public PropertyValidationResult Validate(T value)
        {
            if (_propertyValidator is not null)
            {
                return _propertyValidator.Validate(value);
            }

            if (_asyncPropertyValidator is not null)
            {
                return _asyncPropertyValidator
                    .ValidateAsync(value)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
            }


            throw new NotImplementedException();
        }

        public Task<PropertyValidationResult> ValidateAsync(T value)
        {
            if (_asyncPropertyValidator is not null)
            {
                return _asyncPropertyValidator.ValidateAsync(value);
            }

            if (_propertyValidator is not null)
            {
                var syncValidationResult = _propertyValidator.Validate(value);
                return Task.FromResult(syncValidationResult);
            }

            throw new NotImplementedException();
        }
    }

    public record PropertyValidationResult
    {
        public bool IsValid { get; }
        public IValidationFailure? Error { get; }

        private PropertyValidationResult(bool isValid, IValidationFailure? failure)
        {
            IsValid = isValid;
            Error = failure;
        }

        public static PropertyValidationResult Success()
            => new(true, null);

        public static PropertyValidationResult Failure(IValidationFailure failure)
            => new(
                false,
                failure ?? throw new ArgumentNullException(nameof(failure)));
    }

    public class NotNullValidator<T> : IPropertyValidator<T>
    {
        public bool TryValidate(T value, out IValidationFailure? failure)
        {
            failure = default;
            if (value is null)
            {
                failure = new ValueNullFailure();
                return false;
            }

            return true;
        }

        public PropertyValidationResult Validate(T value)
        {
            if (value is null)
            {
                return PropertyValidationResult.Failure(new ValueNullFailure());
            }

            return PropertyValidationResult.Success();
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

        public PropertyValidationResult Validate(string value)
        {
            if (value.Length < _minLength ||
                value.Length > _maxLength)
            {
                var failure = new StringLengthFailure(_minLength, _maxLength, value.Length);
                return PropertyValidationResult.Failure(failure);
            }

            return PropertyValidationResult.Success();
        }
    }

    public class AsyncDelayedValidator<T> : IAsyncPropertyValidator<T>
    {
        private readonly int _delayMs;

        public AsyncDelayedValidator(int delayMs)
        {
            _delayMs = delayMs;
        }

        public async Task<PropertyValidationResult> ValidateAsync(T value)
        {
            await Task.Delay(_delayMs);
            return PropertyValidationResult.Success();
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

        public IValidationFailure? Error { get; }

        public Result(T value)
        {
            _value = value;
            Error = null;
        }

        public Result(IValidationFailure validationError)
        {
            Error = validationError ?? throw new ArgumentNullException(nameof(validationError));
            _value = default;
        }

        public bool TryGet(out T? value)
        {
            value = default;
            if (Error is not null)
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

        public static Result<T> Failure(IValidationFailure failure)
        {
            return new Result<T>(failure);
        }
    }

    public class GroupResult<T>
    {
        private readonly IDictionary<string, IEnumerable<IValidationFailure>> _validationErrors;

        public GroupResult(T value)
        {
        }

        public GroupResult(IDictionary<string, IEnumerable<IValidationFailure>> validationErrors)
        {
            _validationErrors = validationErrors;
        }
    }

    public static class ValidatorExtensions
    {
        public static IValidatorBuilder<T> NotNull<T>(this IValidatorBuilder<T> validatorBuilder)
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
