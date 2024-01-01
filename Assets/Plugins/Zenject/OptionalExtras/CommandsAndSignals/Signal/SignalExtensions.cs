using System;
using ModestTree;

namespace Zenject
{
    public static class SignalExtensions
    {
        public static ConditionCopyNonLazyBinder BindSignal<TSignal>(this DiContainer container)
            where TSignal : ISignal
        {
            return container.BindSignal<TSignal>(null);
        }

        public static ConditionCopyNonLazyBinder BindSignal<TSignal>(this DiContainer container, object identifier)
            where TSignal : ISignal
        {
            return container.Bind<TSignal>().WithId(identifier).AsSingle();
        }

        public static ConditionCopyNonLazyBinder BindTrigger<TTrigger>(this DiContainer container)
            where TTrigger : ITrigger
        {
            return container.BindTrigger<TTrigger>(null);
        }

        public static ConditionCopyNonLazyBinder BindTrigger<TTrigger>(this DiContainer container, object identifier)
            where TTrigger : ITrigger
        {
            Type concreteSignalType = typeof(TTrigger).DeclaringType;

            Assert.IsNotNull(concreteSignalType);
            Assert.That(concreteSignalType.DerivesFrom<ISignal>());

            container.Bind(concreteSignalType.BaseType())
                .To(concreteSignalType)
                .AsSingle()
                .When(ctx => ctx.ObjectType != null && ctx.ObjectType.DerivesFromOrEqual<TTrigger>() && ctx.ConcreteIdentifier == identifier);

            return container.Bind<TTrigger>().WithId(identifier).AsSingle();
        }
    }
}
