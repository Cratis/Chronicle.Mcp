// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools.Design;

namespace Cratis.Chronicle.Mcp.Specs.Tools.Design.for_EventSchemaParser;

public class when_parsing : Specification
{
    const string Schema = """{"type":"object","required":["name",7],"properties":{"name":{"type":"string","description":"The name"},"id":{"type":"string","format":"guid"},"uid":{"type":"string","format":"uuid"},"at":{"type":"string","format":"date-time"},"day":{"type":"string","format":"date"},"clock":{"type":"string","format":"time"},"small":{"type":"integer","format":"int32"},"big":{"type":"integer","format":"int64"},"count":{"type":"integer"},"price":{"type":"number"},"flag":{"type":"boolean"},"nullable":{"type":["null","integer"]},"onlyNull":{"type":["null"]},"numericType":{"type":5},"tags":{"type":"array","items":{"type":"string"}},"untyped":{"type":"array"},"badItems":{"type":"array","items":3},"nestedIds":{"type":"array","items":{"type":"string","format":"guid"}},"address":{"$ref":"#/definitions/Address"},"emptyRef":{"$ref":""},"slashRef":{"$ref":"///"},"anything":{}}}""";

    IReadOnlyList<EventSchemaProperty> _result;

    void Establish() { }

    void Because() => _result = EventSchemaParser.Parse(Schema);

    string ClrOf(string name) => _result.Single(property => property.Name == name).ClrType;

    [Fact] void should_parse_all_properties() => _result.Count.ShouldEqual(22);
    [Fact] void should_flag_required() => _result.Single(property => property.Name == "name").Required.ShouldBeTrue();
    [Fact] void should_not_flag_unlisted_as_required() => _result.Single(property => property.Name == "id").Required.ShouldBeFalse();
    [Fact] void should_read_description() => _result.Single(property => property.Name == "name").Description.ShouldEqual("The name");
    [Fact] void should_read_format() => _result.Single(property => property.Name == "id").Format.ShouldEqual("guid");
    [Fact] void should_map_string() => ClrOf("name").ShouldEqual("string");
    [Fact] void should_map_guid() => ClrOf("id").ShouldEqual("Guid");
    [Fact] void should_map_uuid() => ClrOf("uid").ShouldEqual("Guid");
    [Fact] void should_map_date_time() => ClrOf("at").ShouldEqual("DateTimeOffset");
    [Fact] void should_map_date() => ClrOf("day").ShouldEqual("DateOnly");
    [Fact] void should_map_time() => ClrOf("clock").ShouldEqual("TimeOnly");
    [Fact] void should_map_int32() => ClrOf("small").ShouldEqual("int");
    [Fact] void should_map_int64() => ClrOf("big").ShouldEqual("long");
    [Fact] void should_map_integer() => ClrOf("count").ShouldEqual("int");
    [Fact] void should_map_number() => ClrOf("price").ShouldEqual("double");
    [Fact] void should_map_boolean() => ClrOf("flag").ShouldEqual("bool");
    [Fact] void should_skip_null_in_type_array() => ClrOf("nullable").ShouldEqual("int");
    [Fact] void should_default_null_only_type_to_object() => ClrOf("onlyNull").ShouldEqual("object");
    [Fact] void should_default_non_string_type_to_object() => ClrOf("numericType").ShouldEqual("object");
    [Fact] void should_map_array_of_strings() => ClrOf("tags").ShouldEqual("IEnumerable<string>");
    [Fact] void should_map_untyped_array_to_object_elements() => ClrOf("untyped").ShouldEqual("IEnumerable<object>");
    [Fact] void should_map_non_object_items_to_object_elements() => ClrOf("badItems").ShouldEqual("IEnumerable<object>");
    [Fact] void should_map_array_element_format() => ClrOf("nestedIds").ShouldEqual("IEnumerable<Guid>");
    [Fact] void should_use_reference_name() => ClrOf("address").ShouldEqual("Address");
    [Fact] void should_default_empty_reference_to_object() => ClrOf("emptyRef").ShouldEqual("object");
    [Fact] void should_default_segmentless_reference_to_object() => ClrOf("slashRef").ShouldEqual("object");
    [Fact] void should_default_empty_definition_to_object() => ClrOf("anything").ShouldEqual("object");
}
