// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools.Design;

namespace Cratis.Chronicle.Mcp.Specs.Tools.Design.for_EventNameMorphology;

public class when_inferring_entity : Specification
{
    [Fact] void should_infer_from_simple_creation_event() => EventNameMorphology.InferEntity("AuthorRegistered").ShouldEqual("Author");
    [Fact] void should_infer_multi_word_entity() => EventNameMorphology.InferEntity("OrderLineAdded").ShouldEqual("OrderLine");
    [Fact] void should_infer_entity_when_verb_has_particle() => EventNameMorphology.InferEntity("UserLoggedIn").ShouldEqual("User");
    [Fact] void should_infer_entity_from_irregular_verb() => EventNameMorphology.InferEntity("InvoiceSent").ShouldEqual("Invoice");
    [Fact] void should_use_full_name_when_no_verb_is_found() => EventNameMorphology.InferEntity("OrderShipment").ShouldEqual("OrderShipment");
    [Fact] void should_use_full_name_when_name_is_only_a_verb() => EventNameMorphology.InferEntity("Registered").ShouldEqual("Registered");
    [Fact] void should_capitalize_camel_case_names() => EventNameMorphology.InferEntity("authorRegistered").ShouldEqual("Author");
}
