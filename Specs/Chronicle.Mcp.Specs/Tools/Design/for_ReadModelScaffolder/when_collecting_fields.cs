// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Events;
using Cratis.Chronicle.Contracts.EventTypes;
using Cratis.Chronicle.Mcp.Tools.Design;

namespace Cratis.Chronicle.Mcp.Specs.Tools.Design.for_ReadModelScaffolder;

public class when_collecting_fields : Specification
{
    List<string> _notes;
    IReadOnlyList<ScaffoldedField> _result;

    static EventTypeDetailsResponse Registration(string id, string schema) => new()
    {
        Type = new EventType { Id = id },
        Schema = schema
    };

    void Establish() => _notes = [];

    void Because() => _result = ReadModelScaffolder.CollectFields(
        [
            Registration("AuthorRegistered", "{\"properties\":{\"id\":{\"type\":\"string\"},\"name\":{\"type\":\"string\"},\"age\":{\"type\":\"integer\"}}}"),
            Registration("AuthorRenamed", "{\"properties\":{\"Name\":{\"type\":\"string\"},\"age\":{\"type\":\"string\"},\"email\":{\"type\":\"string\"}}}")
        ],
        _notes);

    [Fact] void should_skip_id_and_duplicates() => _result.Select(field => field.Name).ShouldEqual("Name", "Age", "Email");
    [Fact] void should_keep_first_seen_type() => _result.Single(field => field.Name == "Age").ClrType.ShouldEqual("int");
    [Fact] void should_record_first_source_event() => _result.Single(field => field.Name == "Name").SourceEventType.ShouldEqual("AuthorRegistered");
    [Fact] void should_record_source_of_later_field() => _result.Single(field => field.Name == "Email").SourceEventType.ShouldEqual("AuthorRenamed");
    [Fact] void should_note_only_the_type_conflict() => _notes.Count.ShouldEqual(1);
    [Fact] void should_describe_the_conflict() => _notes[0].ShouldEqual("Property 'Age' appears as int (from AuthorRegistered) and string (from AuthorRenamed); kept the first. Review the type.");
}
