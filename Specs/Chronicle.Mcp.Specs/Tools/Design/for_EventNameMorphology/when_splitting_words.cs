// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools.Design;

namespace Cratis.Chronicle.Mcp.Specs.Tools.Design.for_EventNameMorphology;

public class when_splitting_words : Specification
{
    [Fact] void should_split_pascal_case() => EventNameMorphology.SplitWords("AuthorRegistered").ShouldEqual("Author", "Registered");
    [Fact] void should_split_camel_case() => EventNameMorphology.SplitWords("authorRegistered").ShouldEqual("author", "Registered");
    [Fact] void should_keep_acronyms_together() => EventNameMorphology.SplitWords("PIIRemoved").ShouldEqual("PII", "Removed");
    [Fact] void should_split_kebab_case() => EventNameMorphology.SplitWords("author-registered").ShouldEqual("author", "registered");
    [Fact] void should_split_snake_case() => EventNameMorphology.SplitWords("author_registered").ShouldEqual("author", "registered");
    [Fact] void should_split_multi_word_entity() => EventNameMorphology.SplitWords("OrderLineAdded").ShouldEqual("Order", "Line", "Added");
}
