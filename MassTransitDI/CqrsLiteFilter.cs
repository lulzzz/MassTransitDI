using CQRSlite.Domain;
using CQRSlite.Events;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MassTransitDI
{
    public class CqrsLiteFilter : IFilter<ConsumeContext>
    {
        private IServiceProvider _container;

        public CqrsLiteFilter(IServiceProvider container)
        {
            _container = container;
        }

        public async Task Send(ConsumeContext context, IPipe<ConsumeContext> next)
        {
            try
            {
                var scopeFactory = _container.GetRequiredService<IServiceScopeFactory>();

                using (var serviceScope = scopeFactory.CreateScope())
                {
                    var services = serviceScope.ServiceProvider;

                    var publisher = services.GetRequiredService<IEventPublisher>() as MassTransitEventPublisher;

                    //  Inject ConsumeContext into IEventPublisher implementation here
                    publisher.SetContext(context);

                    await next.Send(context).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("cqrslite");
        }
    }

    public class CqrsLiteSpecification : IPipeSpecification<ConsumeContext>
    {
        private IServiceProvider _container;

        public CqrsLiteSpecification(IServiceProvider container)
        {
            _container = container;
        }

        public void Apply(IPipeBuilder<ConsumeContext> builder)
        {
            builder.AddFilter(new CqrsLiteFilter(_container));
        }

        public IEnumerable<ValidationResult> Validate()
        {
            yield break;
        }
    }

    public static class CqrsLiteMiddlewareConfiguratorExtensions
    {
        public static void UseCqrsLite(this IConsumePipeConfigurator configurator, IServiceProvider container)
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var specification = new CqrsLiteSpecification(container);

            configurator.AddPipeSpecification(specification);
        }
    }
}
