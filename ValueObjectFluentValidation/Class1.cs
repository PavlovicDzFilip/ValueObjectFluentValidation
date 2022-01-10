using System.Collections.ObjectModel;
using System.Linq.Expressions;
using ValueObjectFluentValidation.Group;
using ValueObjectFluentValidation.Single;
using ValueObjectFluentValidation.Util;

namespace ValueObjectFluentValidation
{
    public interface IRequestValidationFailureCollection
    {
        void AddFailure(string propertyName, IValidationFailureCollection failures);
        IReadOnlyDictionary<string, IValidationFailureCollection> AsDictionary();
        void AddFailures(IRequestValidationFailureCollection failureCollection);
    }
    internal class RequestValidationFailureCollection : IRequestValidationFailureCollection
    {
        private readonly Dictionary<string, IValidationFailureCollection> _failuresByPropertyName = new();
        public void AddFailure(string propertyName, IValidationFailureCollection failures)
        {
            if (!_failuresByPropertyName.TryGetValue(propertyName, out var existingFailures))
            {
                existingFailures = new ValidationFailureCollection();
                _failuresByPropertyName.Add(propertyName, existingFailures);
            }

            foreach (var failure in failures)
            {
                existingFailures.AddFailure(failure);
            }
        }

        public IReadOnlyDictionary<string, IValidationFailureCollection> AsDictionary()
            => new ReadOnlyDictionary<string, IValidationFailureCollection>(_failuresByPropertyName);

        public void AddFailures(IRequestValidationFailureCollection failureCollection)
        {
            foreach (var pair in failureCollection.AsDictionary())
            {
                AddFailure(pair.Key, pair.Value);
            }
        }
    }

    public class ValidationResult<T> : Result<T, IRequestValidationFailureCollection>
    {
        public ValidationResult(T success) : base(success)
        {
        }

        public ValidationResult(IRequestValidationFailureCollection failure) : base(failure)
        {
        }
    }

    public static class RequestValidator
    {
        public static IRequestValidatorBuilder<T> For<T>(T request)
        {
            return new RequestValidatorBuilder<T>(request);
        }
    }

    internal class RequestValidatorBuilder<TRequest> : IRequestValidatorBuilder<TRequest>
    {
        private readonly TRequest _request;

        public RequestValidatorBuilder(TRequest request)
        {
            _request = request;
        }

        public IRequestGroupValidator<T1, T2> Group<T1, T2>(IRule<TRequest, T1> rule1, IRule<TRequest, T2> rule2)
        {
            var functions = new[]
            {
                FromRule(rule1),
                FromRule(rule2)
            };

            return new RequestGroupValidator<TRequest, T1, T2>(_request, functions);
        }

        private Func<TRequest, ValidationResult<object>> FromRule<TValueObject>(IRule<TRequest, TValueObject> rule)
        {
            Func<TRequest, ValidationResult<object>> func = request =>
            {
                var result = rule.Validate(request);
                if (result.TryGet(out var obj))
                {
                    return new ValidationResult<object>(obj!);
                }

                return new ValidationResult<object>(result.Failure);
            };

            return func;
        }
    }

    internal class RequestGroupValidator<T, T1, T2> : IRequestGroupValidator<T1, T2>
    {
        private readonly T _request;
        private readonly Func<T, ValidationResult<object>>[] _rules;

        public RequestGroupValidator(T request, Func<T, ValidationResult<object>>[] rules)
        {
            _request = request;
            _rules = rules;
        }

        public ValidationResult<TValueObject> WhenValid<TValueObject>(Func<T1, T2, TValueObject> createValueObjectFunc)
            => Validate<TValueObject>(createValueObjectFunc);

        public Task<ValidationResult<TValueObject>> WhenValidAsync<TValueObject>(Func<T1, T2, TValueObject> createValueObjectFunc)
        {
            throw new NotImplementedException();
        }

        private ValidationResult<TValueObject> Validate<TValueObject>(Delegate createValueObjectFunc)
        {
            var ruleValues = new object[_rules.Length];
            var i = 0;
            var allFailures = new RequestValidationFailureCollection();
            foreach (var rule in _rules)
            {
                var result = rule.Invoke(_request);
                if (result.TryGet(out var valueObject))
                {
                    ruleValues[i] = valueObject!;
                }
                else
                {
                    allFailures.AddFailures(result.Failure);
                }

                i++;
            }

            if (allFailures.AsDictionary().Any())
            {
                return new ValidationResult<TValueObject>(allFailures);
            }

            var validRequest = createValueObjectFunc.DynamicInvoke(ruleValues);
            return new ValidationResult<TValueObject>((TValueObject)validRequest!);
        }
    }

    public interface IRequestValidatorBuilder<T>
    {
        IRequestGroupValidator<T1, T2> Group<T1, T2>(
            IRule<T, T1> rule1,
            IRule<T, T2> rule2);
    }

    public interface IRequestGroupValidator<T1, T2>
    {
        ValidationResult<TValueObject> WhenValid<TValueObject>(Func<T1, T2, TValueObject> createValueObjectFunc);
        Task<ValidationResult<TValueObject>> WhenValidAsync<TValueObject>(Func<T1, T2, TValueObject> createValueObjectFunc);
    }

    public interface IRequestValidator<in T, TValid>
    {
        ValidationResult<TValid> TryCreate(T value);
    }

    public interface IRule<in T, TValueObject>
    {
        ValidationResult<TValueObject> Validate(T request);
    }

    internal class Rule<T, TValueObject> :
        IRule<T, TValueObject>
    {
        private readonly Func<object[], ValidationResult<TValueObject>> _valueObjectFunc;
        private readonly LambdaExpression[] _selectors;

        public Rule(Func<object[], ValidationResult<TValueObject>> valueObjectFunc, params LambdaExpression[] selectors)
        {
            _valueObjectFunc = valueObjectFunc;
            _selectors = selectors;
        }

        public ValidationResult<TValueObject> Validate(T request)
        {
            var propertyValues = _selectors
                .Select(selector => selector.Compile())
                .Select(lambda => lambda.DynamicInvoke(request))
                .ToArray();

            var result = _valueObjectFunc.Invoke(propertyValues!);
            return result;
        }
    }

    public abstract class AbstractRequestValidator<T, TValid> : IRequestValidator<T, TValid>
    {
        public abstract ValidationResult<TValid> TryCreate(T value);

        public IRule<T, TValueObject> Rule<TProperty, TValueObject>(
            Expression<Func<T, TProperty>> selector,
            Func<TProperty, SingleResult<TValueObject>> valueObjectFunc)
        {
            Func<object[], ValidationResult<TValueObject>> func = @params =>
                {
                    var param = (TProperty)@params[0];
                    var result = valueObjectFunc.Invoke(param);
                    if (result.TryGet(out var valueObject))
                    {
                        return new ValidationResult<TValueObject>(valueObject!);
                    }

                    var failure = new RequestValidationFailureCollection();
                    failure.AddFailure(selector.FromExpression(), result.Failure);
                    return new ValidationResult<TValueObject>(failure);
                };

            return new Rule<T, TValueObject>(func, selector);
        }

        public IRule<T, TValueObject> Rule<TProperty1, TProperty2, TValueObject>(
            Expression<Func<T, TProperty1>> prop1Selector,
            Expression<Func<T, TProperty2>> prop2Selector,
            Func<TProperty1, TProperty2, GroupResult<TValueObject>> valueObjectFunc)
        {
            Func<object[], ValidationResult<TValueObject>> func = @params =>
            {
                var result = (GroupResult<TValueObject>)valueObjectFunc.DynamicInvoke(@params)!;
                if (result.TryGet(out var valueObject))
                {
                    return new ValidationResult<TValueObject>(valueObject!);
                }

                var failure = new RequestValidationFailureCollection();
                var propertyNames = new LambdaExpression[] { prop1Selector, prop2Selector }
                    .Select(p => p.FromExpression()).ToArray();

                foreach (var pair in result.Failure.AsDictionary())
                {
                    failure.AddFailure(propertyNames[pair.Key], pair.Value);
                }

                return new ValidationResult<TValueObject>(failure);
            };

            return new Rule<T, TValueObject>(func, prop1Selector, prop2Selector);
        }
    }

    internal static class ExpressionExtensions
    {
        public static string FromExpression(this Expression expression)
        {
            if (expression is not LambdaExpression lambdaExpression)
            {
                throw new InvalidOperationException();
            }

            var memberNames = new Stack<string>();

            var getMemberExp = new Func<Expression?, MemberExpression?>(toUnwrap =>
            {
                if (toUnwrap is UnaryExpression unaryExpression)
                {
                    return unaryExpression.Operand as MemberExpression;
                }

                return toUnwrap as MemberExpression;
            });

            var memberExp = getMemberExp(lambdaExpression.Body);

            while (memberExp is not null)
            {
                memberNames.Push(memberExp.Member.Name);
                memberExp = getMemberExp(memberExp.Expression);
            }

            return memberNames.Peek();
        }
    }
}
