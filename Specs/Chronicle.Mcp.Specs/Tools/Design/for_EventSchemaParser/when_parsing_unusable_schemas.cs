// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools.Design;

namespace Cratis.Chronicle.Mcp.Specs.Tools.Design.for_EventSchemaParser;

public class when_parsing_unusable_schemas : Specification
{
    [Fact] void should_return_empty_for_null() => EventSchemaParser.Parse(null).ShouldBeEmpty();
    [Fact] void should_return_empty_for_whitespace() => EventSchemaParser.Parse("  ").ShouldBeEmpty();
    [Fact] void should_return_empty_for_invalid_json() => EventSchemaParser.Parse("{ nope").ShouldBeEmpty();
    [Fact] void should_return_empty_for_non_object_root() => EventSchemaParser.Parse("[1]").ShouldBeEmpty();
    [Fact] void should_return_empty_without_properties() => EventSchemaParser.Parse("{\"type\":\"object\"}").ShouldBeEmpty();
    [Fact] void should_return_empty_when_properties_is_not_an_object() => EventSchemaParser.Parse("{\"properties\":[]}").ShouldBeEmpty();
    [Fact] void should_treat_non_array_required_as_empty() => EventSchemaParser.Parse("{\"required\":\"a\",\"properties\":{\"a\":{\"type\":\"string\"}}}")[0].Required.ShouldBeFalse();
}
