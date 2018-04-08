using MassTransit;
using System;

namespace MassTransitDI.Domain.Commands
{
    public interface CreateRecord : CorrelatedBy<Guid>
    {
        Guid Id { get; }
        int Index { get; }
    }
}
