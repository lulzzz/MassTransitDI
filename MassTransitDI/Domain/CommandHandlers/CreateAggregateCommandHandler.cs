using CQRSlite.Domain;
using MassTransit;
using MassTransitDI.Domain.Aggregate;
using MassTransitDI.Domain.Commands;
using System;
using System.Threading.Tasks;

namespace MassTransitDI.Domain.CommandHandlers
{
    public class CreateAggregateCommandHandler : IConsumer<CreateRecord>
    {
        private ISession _session;

        public CreateAggregateCommandHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task Consume(ConsumeContext<CreateRecord> context)
        {
            var record = new Record(context.Message.Id, context.Message.Index);

            await _session.Add(record);

            await _session.Commit();
        }
    }
}
