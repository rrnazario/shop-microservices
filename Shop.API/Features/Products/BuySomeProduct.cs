using MassTransit;

namespace Shop.API.Features.Products;

public record CreateOrder(Guid CorrelationId) : CorrelatedBy<Guid>;
public record ProcessOrder(Guid OrderId, Guid ProcessingId);
public record OrderProcessed(Guid OrderId, Guid ProcessingId);
public record OrderCancelled(Guid OrderId, string Reason);

public class ProcessOrderConsumer : IConsumer<ProcessOrder>
{
    public async Task Consume(ConsumeContext<ProcessOrder> context)
      => await context.RespondAsync(new OrderProcessed(context.Message.OrderId, context.Message.ProcessingId));
}
public class BuyingState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string? CurrentState { get; set; }
    public Guid? ProcessingId { get; set; }
    public Guid? RequestId { get; set; }
    public Uri? ResponseAddress { get; set; }
    public Guid OrderId { get; set; }
}
public class BuyingStateMachine : MassTransitStateMachine<BuyingState>
{
    public State? Created { get; set; }

    public State? Cancelled { get; set; }

    public Event<CreateOrder>? OrderSubmitted { get; set; }

    public Request<BuyingState, ProcessOrder, OrderProcessed>? ProcessOrder { get; set; }

    public BuyingStateMachine()
    {
        InstanceState(m => m.CurrentState);

        Event(() => OrderSubmitted);

        Request(() => ProcessOrder, order => order.ProcessingId, config => { config.Timeout = TimeSpan.Zero; });

        Initially(
            When(OrderSubmitted)
                .Then(context =>
                {
                    context.Saga.CorrelationId = context.Message.CorrelationId;
                    context.Saga.ProcessingId = Guid.NewGuid();
                    context.Saga.OrderId = Guid.NewGuid();
                    context.Saga.RequestId = context.RequestId;
                    context.Saga.ResponseAddress = context.ResponseAddress!;
                })
                .Request(ProcessOrder, context => new ProcessOrder(context.Saga.OrderId, context.Saga.ProcessingId!.Value))
                .TransitionTo(ProcessOrder!.Pending));

        During(ProcessOrder.Pending,
            When(ProcessOrder.Completed)
                .TransitionTo(Created)
                .ThenAsync(async context =>
                {
                    var endpoint = await context.GetSendEndpoint(context.Saga.ResponseAddress!);
                    await endpoint.Send(context.Saga, r => r.RequestId = context.Saga.RequestId);
                }),
            When(ProcessOrder.Faulted)
                .TransitionTo(Cancelled)
                .ThenAsync(async context =>
                {
                    var endpoint = await context.GetSendEndpoint(context.Saga.ResponseAddress!);
                    await endpoint.Send(new OrderCancelled(context.Saga.OrderId, "Faulted"), r => r.RequestId = context.Saga.RequestId);
                }),
            When(ProcessOrder.TimeoutExpired)
                .TransitionTo(Cancelled)
                .ThenAsync(async context =>
                {
                    var endpoint = await context.GetSendEndpoint(context.Saga.ResponseAddress!);
                    await endpoint.Send(new OrderCancelled(context.Saga.OrderId, "Time-out"), r => r.RequestId = context.Saga.RequestId);
                }));
    }
}
