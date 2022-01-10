using System.Reflection.Metadata;

namespace ValueObjectFluentValidationTests
{
    public class Test { }

    //public class CreateApplication
    //{
    //    public record Command(string? Name, string? Street, string? Number, int Tmp, string NonNullStr);
    //    public record CommandWithAddress(string? Name, AddressDto Address);
    //    public record AddressDto(string Street, string Number);

    //    public record ValidCommand(ApplicationName Name);

    //    public class RequestValidatorTransformator : AbstractRequestValidator<Command, ValidCommand>
    //    {
    //        public override SingleResult<ValidCommand> Validate(Command command)
    //        {
    //            var r3 = Rule(x => x.NonNullStr, SingleResult<string>.Success);
    //            return RequestValidator.For(command)
    //                .Group(
    //                    Rule(cmd => cmd.Name, ApplicationName.TryCreate),
    //                    Rule(cmd => cmd.Street, cmd => cmd.Number, Address.TryCreate))
    //                .WhenValid((name, address) => new ValidCommand(name));
    //        }
    //    }

    //    public class AddressDtoValidator : AbstractRequestValidator<AddressDto, Address>
    //    {
    //        public override SingleResult<Address> Validate(AddressDto value)
    //        {
    //            return RequestValidator
    //                .For(value)
    //                .Group(
    //                    Rule(cmd => (cmd.Street, cmd.Number), tuple => Address.TryCreate(tuple.Street, tuple.Number)),
    //                    Rule(cmd => cmd.Number, x => SingleResult<string>.Failure(new ValueNullFailure()))
    //                )
    //                .WhenValid((a, x) => a);
    //        }
    //    }

    //    public class Handler
    //    {
    //        void Handle(ValidCommand command) { }
    //    }
    //}
}