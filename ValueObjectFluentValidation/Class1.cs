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

        public static IGroupValidator<T1, T2> Group<T1,T2>(
            IValidatorBuilder<T1> firstValidator, 
            IValidatorBuilder<T2> secondValidator)
        {
            throw new NotImplementedException();
        }
    }

    public interface IGroupValidator<T1, T2>
    {
        IWhenValid<TValueObject> WhenValid<TValueObject>(Func<T1, T2, TValueObject> func);
    }


    internal class ValidatorBuilderInitial<T> : ValidationBuilder<T>, IValidatorBuilderInitial<T>
    {
        public IValidatorBuilder<TTransformed> Transform<TTransformed>(Func<T, TTransformed> transformFunc)
        {
            return this;
        }

        public ValidatorBuilderInitial()
        {
        }
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

        public IWhenValid<TValueObject> WhenValid<TValueObject>(
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

        public IWhenValid<TValueObject> WhenValid<TValueObject>(
            Func<T, TValueObject> createValueObjectFunc);
    }

    public interface IWhenValid<TValueObject>
    {
        Result<TValueObject> Validate();
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

    public class RequestValidator
    {
        public static IRequestValidatorBuilder<T> For<T>(T request)
        {
            throw new NotImplementedException();
        }
    }

    public interface IRequestValidatorBuilder<T>
    {
        IRequestValidatorBuilder<T> RuleFor<TProperty, TValueObject>(
            Func<T, TProperty> func,
            Func<TProperty, Result<TValueObject>> valueObjectFunc)
        where TProperty: struct
        {

        }

        IRequestValidatorBuilder<T> RuleFor<TValueObject>(
            Func<T, string?> func,
            Func<string?, Result<TValueObject>> valueObjectFunc)
        {

        }

        IWhenValid<TValidRequest> WhenValid<T1, T2, TValidRequest>(Func<T1, T2, TValidRequest> func);
    }

    public static IGroupValidator<T1, T2> Group<T1, T2>(
        IValidatorBuilder<T1> firstValidator,
        IValidatorBuilder<T2> secondValidator)
    {
        throw new NotImplementedException();
    }
}
