namespace ValueObjectFluentValidation
{
    public class Class1
    {
        public void Hello()
        {
            Console.WriteLine("Zoki Nikola");
        }
    }

    public class Validator
    {
        //public static IValidatorBuilder<T, T> For<T>(T value)
        //{
        //    return new ValidationBuilder<T>(value);
        //}

        public static IValidatorBuilderInitial<T> For<T>(T name)
        {
            throw new NotImplementedException();
        }

        public static IGroupValidator<T1, T2> Group<T1, T2>(
            IValidatorBuilder<T1> firstValidator,
            IValidatorBuilder<T2> secondValidator)
        {
            throw new NotImplementedException();
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
        private readonly T _value;
        private readonly List<IPropertyValidator<T>> _validators;

        public ValidationBuilder()
        {
            _validators = new List<IPropertyValidator<T>>();
        }

        public IValidatorBuilder<T> AddValidator(IPropertyValidator<T> propertyValidator)
        {
            _validators.Add(propertyValidator);
            return this;
        }

        public Result<TValueObject> WhenValid<TValueObject>(
            Func<T, TValueObject> createValueObjectFunc)
        {
            throw new NotImplementedException();
        }

        //public Result<TValueObject> WhenValid<TValueObject>(Func<TCurrent, TValueObject> createValueObjectFunc)
        //{
        //    var validationErrors = Validate();
        //    if (validationErrors.Any())
        //    {
        //        return Result<TValueObject>.Failure(validationErrors);
        //    }

        //    var valueObject = createValueObjectFunc(_value);
        //    return Result<TValueObject>.Success(valueObject);
        //}

        private IValidationFailure[] Validate()
        {
            foreach (var validator in _validators)
            {
                var isValid = validator.TryValidate(_value, out var failure);
                if (!isValid)
                {
                    return new[] { failure! };
                }
            }

            return Array.Empty<IValidationFailure>();
        }
    }

    public interface IValidationFailure
    {

    }

    public interface IValidatorBuilder<T>
    {
        IValidatorBuilder<T> AddValidator(IPropertyValidator<T> propertyValidator);

        public Result<TValueObject> WhenValid<TValueObject>(
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

    public interface IPropertyValidator<T>
    {
        bool TryValidate(T value, out IValidationFailure? failure);
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
    }

    public class ValueNullFailure : IValidationFailure
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

        public IEnumerable<IValidationFailure> Errors { get; }

        public Result(T value)
        {
            _value = value;
            Errors = Array.Empty<IValidationFailure>();
        }

        public Result(IEnumerable<IValidationFailure> validationErrors)
        {
            if (validationErrors == null || !validationErrors.Any())
            {
                throw new ArgumentNullException(nameof(validationErrors));
            }

            Errors = validationErrors;
            _value = default;
        }

        public bool TryGet(out T? value)
        {
            value = default;
            if (Errors.Any())
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

    public class GroupResult<T> : Result<T>
    {
        private readonly IDictionary<string, IEnumerable<IValidationFailure>> _validationErrors;

        public GroupResult(T value) : base(value)
        {
        }

        public GroupResult(IDictionary<string, IEnumerable<IValidationFailure>> validationErrors) :
            base(validationErrors.SelectMany(error => error.Value))
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
            return validatorBuilder.AddValidator(new NotNullValidator<string>());
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
