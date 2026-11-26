// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Events;
using Cratis.Chronicle.Contracts.Observation;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

public class when_consumer_identity_is_uncertain : a_namespace_audit
{
    const string ReactionSource = """
        module Library
          feature Registration
            slice Automation Follow
              event Registered
                name String
              event Audited
                name String
              reaction Follow
                when Registered
                  name
                  produces Audited
                    name = name
        """;

    void Establish()
    {
        Argument("modelSource", ReactionSource);
        Argument("applicationName", Store);
    }

    [Theory]
    [InlineData("UnmappedConstruct", "event-log", ObserverOwner.Client, true)]
    [InlineData("PlatformConsumer", "event-log", ObserverOwner.Kernel, false)]
    public async Task should_not_guess_a_unique_application_consumer(string reason, string sequence, ObserverOwner owner, bool ambiguous)
    {
        ObserverInformation[] observers = ambiguous
            ? [Observer("First.Follow", sequence, owner), Observer("Second.Follow", sequence, owner)]
            : [Observer("Platform.Follow", sequence, owner)];
        _observers.GetObservers(Arg.Any<AllObserversRequest>(), Arg.Any<CallContext>()).Returns(observers);
        await Audit();
        Body.GetProperty("findings").EnumerateArray().Any(item => item.TryGetProperty("reason", out var value) && value.GetString() == reason).ShouldBeTrue();
    }

    [Fact]
    public async Task should_distinguish_a_consumer_on_a_different_sequence()
    {
        _observers.GetObservers(Arg.Any<AllObserversRequest>(), Arg.Any<CallContext>()).Returns([Observer("App.Follow", "other-log", ObserverOwner.Client)]);
        await Audit();
        Body.GetProperty("findings").EnumerateArray().Any(item => item.GetProperty("kind").GetString() == "ConsumerOnOtherSequence").ShouldBeTrue();
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
