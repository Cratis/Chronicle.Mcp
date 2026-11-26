// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Events;
using Cratis.Chronicle.Contracts.Observation;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

public class when_consumer_identity_is_uncertain : a_namespace_audit
{
    const string ReactionSource =
        "module Library\n" +
        "  feature Registration\n" +
        "    slice Automation Follow\n" +
        "      event Registered\n" +
        "        name String\n" +
        "      event Audited\n" +
        "        name String\n" +
        "      reaction Follow\n" +
        "        when Registered\n" +
        "          name\n" +
        "          produces Audited\n" +
        "            name = name\n";

    void Establish()
    {
        Argument("modelSource", ReactionSource);
        Argument("applicationName", Store);
    }

    [Theory]
    [InlineData("reason", "UnmappedConstruct", "event-log", ObserverOwner.Client, true)]
    [InlineData("reason", "PlatformConsumer", "event-log", ObserverOwner.Kernel, false)]
    [InlineData("kind", "ConsumerOnOtherSequence", "other-log", ObserverOwner.Client, false)]
    public async Task should_distinguish_consumers_that_do_not_prove_the_subscription(string field, string finding, string sequence, ObserverOwner owner, bool ambiguous)
    {
        ObserverInformation[] observers = ambiguous
            ? [Observer("First.Follow", sequence, owner), Observer("Second.Follow", sequence, owner)]
            : [Observer("App.Follow", sequence, owner)];
        _observers.GetObservers(Arg.Any<AllObserversRequest>(), Arg.Any<CallContext>()).Returns(observers);
        await Audit(TestContext.Current.CancellationToken);
        Body.GetProperty("findings").EnumerateArray().Any(item => item.TryGetProperty(field, out var value) && value.GetString() == finding).ShouldBeTrue();
    }

    static ObserverInformation Observer(string id, string sequence, ObserverOwner owner) => new()
    {
        Id = id,
        Type = ObserverType.Reactor,
        Owner = owner,
        EventSequenceId = sequence,
        EventTypes = [new EventType { Id = "Registered", Generation = 1 }]
    };
}
