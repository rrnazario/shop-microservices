using MassTransit;

namespace Shop.Common.StateMachines;

public record StartBuy(Guid CorrelationId, int Amount) : CorrelatedBy<Guid>;

public record ProcessBuy(Guid ProductId, Guid ProcessingId, int Amount);

public record BuyProcessed(Guid ProductId);

public record BuyCancelled(Guid ProductId, string Reason);

public class BuyProductState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public Guid? RequestId { get; set; }
    public int CurrentState { get; set; }
    public Guid? ProcessingId { get; set; }
    public Guid ProductId { get; set; }
    public int Amount { get; set; }

    public Uri? ResponseAddress { get; set; }
}

public class BuyProductStateMachine
    : MassTransitStateMachine<BuyProductState>
{
    public State Started { get; set; }
    public State Accepted { get; set; }
    public State Failed { get; set; }

    public Event<StartBuy> BuyStarted { get; set; }

    public Request<BuyProductState, ProcessBuy, BuyProcessed> ProcessBuyRequest { get; set; }

    public BuyProductStateMachine()
    {
        InstanceState(m => m.CurrentState, Started, Accepted, Failed);

        Event(() => BuyStarted);

        Request(() => ProcessBuyRequest, order => order.ProcessingId, config => { config.Timeout = TimeSpan.Zero; });

        Initially(
            When(BuyStarted)
                .Then(ctx =>
                {
                    ctx.Saga.ProcessingId = Guid.CreateVersion7();
                    ctx.Saga.RequestId = ctx.RequestId;

                    ctx.Saga.ProductId = ctx.Message.CorrelationId;
                    ctx.Saga.Amount = ctx.Message.Amount;
                    ctx.Saga.ResponseAddress = ctx.ResponseAddress!;
                })
                .Request(
                    ProcessBuyRequest,
                    context => new ProcessBuy(context.Saga.ProductId, (Guid)context.Saga.ProcessingId!,
                        context.Saga.Amount))
                .TransitionTo(ProcessBuyRequest!.Pending));

        During(ProcessBuyRequest.Pending,
            When(ProcessBuyRequest.Completed)
                .TransitionTo(Accepted)
                .ThenAsync(async context =>
                {
                    var endpoint =
                        await context.GetResponseEndpoint<BuyProcessed>(context.Saga.ResponseAddress,
                            context.Saga.RequestId);

                    await endpoint.Send(new BuyProcessed(context.Saga.ProductId));
                })
                .Finalize(),
            When(ProcessBuyRequest.Faulted)
                .TransitionTo(Failed)
                .ThenAsync(async context =>
                {
                    var endpoint =
                        await context.GetResponseEndpoint<BuyCancelled>(context.Saga.ResponseAddress,
                            context.Saga.RequestId);

                    var message = string.Join("\n", context.Message.Exceptions.Select(e => e.Message));

                    await endpoint.Send(new BuyCancelled(context.Saga.ProductId, message),
                        r => r.RequestId = context.Saga.RequestId);
                })
                .Finalize(),
            When(ProcessBuyRequest.TimeoutExpired)
                .TransitionTo(Failed)
                .ThenAsync(async context =>
                {
                    var endpoint =
                        await context.GetResponseEndpoint<BuyCancelled>(context.Saga.ResponseAddress,
                            context.Saga.RequestId);

                    await endpoint.Send(new BuyCancelled(context.Saga.ProductId, "Timed out"),
                        r => r.RequestId = context.Saga.RequestId);
                })
                .Finalize(),
            Ignore(BuyStarted)
        );

        SetCompletedWhenFinalized();
    }
}