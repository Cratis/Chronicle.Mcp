// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools.Design;

namespace Cratis.Chronicle.Mcp.Specs.Tools.Design.for_EventNameMorphology;

public class when_classifying_lifecycle : Specification
{
    [Fact] void should_classify_registered_as_creation() => EventNameMorphology.Classify("AuthorRegistered").ShouldEqual(LifecycleStage.Creation);
    [Fact] void should_classify_hired_as_creation() => EventNameMorphology.Classify("EmployeeHired").ShouldEqual(LifecycleStage.Creation);
    [Fact] void should_classify_signed_up_as_creation() => EventNameMorphology.Classify("UserSignedUp").ShouldEqual(LifecycleStage.Creation);
    [Fact] void should_classify_changed_as_mutation() => EventNameMorphology.Classify("AuthorNameChanged").ShouldEqual(LifecycleStage.Mutation);
    [Fact] void should_classify_set_as_mutation() => EventNameMorphology.Classify("EmployeeAddressSet").ShouldEqual(LifecycleStage.Mutation);
    [Fact] void should_classify_removed_as_termination() => EventNameMorphology.Classify("AuthorRemoved").ShouldEqual(LifecycleStage.Termination);
    [Fact] void should_classify_cancelled_as_termination() => EventNameMorphology.Classify("OrderCancelled").ShouldEqual(LifecycleStage.Termination);
    [Fact] void should_classify_corrected_as_correction() => EventNameMorphology.Classify("InvoiceCorrected").ShouldEqual(LifecycleStage.Correction);
    [Fact] void should_classify_redacted_as_correction() => EventNameMorphology.Classify("EventRedacted").ShouldEqual(LifecycleStage.Correction);
    [Fact] void should_classify_unknown_verb_as_activity() => EventNameMorphology.Classify("PaymentReceived").ShouldEqual(LifecycleStage.Activity);
    [Fact] void should_classify_logged_in_as_activity() => EventNameMorphology.Classify("UserLoggedIn").ShouldEqual(LifecycleStage.Activity);
    [Fact] void should_classify_name_without_verb_as_activity() => EventNameMorphology.Classify("OrderShipment").ShouldEqual(LifecycleStage.Activity);
}
