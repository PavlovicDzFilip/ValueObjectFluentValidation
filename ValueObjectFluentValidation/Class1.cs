using System;
using System.Xml.XPath;

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
        public static IValidatorBuilder<T> For<T>(T value)
        {
            return new ValidationBuilder<T>(value);
        }
    }

    internal class ValidationBuilder<T> :IValidatorBuilder<T>
    {
        private readonly T _value;
        private readonly List<IPropertyValidator<T>> _validators;

        public ValidationBuilder(T value)
        {
            _value = value;
            _validators = new List<IPropertyValidator<T>>();
        }

        public IValidatorBuilder<T> AddValidator(IPropertyValidator<T> propertyValidator)
        {
            _validators.Add(propertyValidator);
            return this;
        }

        public Result<TValueObject> WhenValid<TValueObject>(Func<T, TValueObject> createValueObjectFunc)
        {
            var validationErrors = Validate();
            if (validationErrors.Any())
            {
                return Result<TValueObject>.Failure(validationErrors);
            }

            var valueObject = createValueObjectFunc(_value);
            return Result<TValueObject>.Success(valueObject);
        }

        private IValidationFailure[] Validate()
        {
            foreach (var validator in _validators)
            {
                var isValid = validator.TryValidate(_value, out var failure);
                if (!isValid)
                {
                    return new[] {failure!};
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

    public interface IPropertyValidator<in T>
    {
        bool TryValidate(T value, out IValidationFailure? failure);
    }

    public class NotNullValidator : IPropertyValidator<string?>
    {
        public bool TryValidate(string? value, out IValidationFailure? failure)
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
        public T Value { get; }

        private Result(T value)
        {
            Value = value;
        }

        private Result(IValidationFailure[] failures)
        {
            throw new NotImplementedException();
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
        public static IValidatorBuilder<string> NotNull(this IValidatorBuilder<string> validatorBuilder)
        {
            return validatorBuilder.AddValidator(new NotNullValidator());
        }

        public static IValidatorBuilder<string> Length(this IValidatorBuilder<string> validatorBuilder, int minLength, int maxLength)
        {
            return validatorBuilder.AddValidator(new NotNullValidator());
        }

        
    }
}
