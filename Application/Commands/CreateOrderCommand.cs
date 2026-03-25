using MediatR;

namespace Application.Commands
{
    //
    public record CreateOrderCommand(decimal Amount) : IRequest<Guid>;
}
