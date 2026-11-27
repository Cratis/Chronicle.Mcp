// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using Cratis.Chronicle.Mcp.Tools.Design.Conformance;
using Cratis.Screenplay.Diagnostics;
using Cratis.Screenplay.Semantics;
using Cratis.Screenplay.Workspaces;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_CatalogModelCompilation;

public class when_assembling_scoped_imports : Specification
{
    const string Entry = "application.play";
    const string Registration = "parts/registration.play";
    const string Register = "parts/details/register.play";
    const string Application = "domain Library\nmodule Library\n  feature Registration\n    import \"parts/registration.play\"\n";
    const string Slice = "slice StateChange Register\n  command Register\n    id Uuid identifier\n    name String\n    produces Registered\n      for id\n      name = name\n  event Registered\n    name String\n";

    [Fact]
    void should_keep_the_feature_placement_across_a_nested_relative_glob()
    {
        var (model, _) = CompileWith("details/*.play");

        model.ShouldNotBeNull();
        var feature = model.Model.Application.Modules.Single().Features.Single();
        feature.Name.ShouldEqual("Registration");
        var registered = feature.Slices.Single().Events.Single();
        registered.Name.ShouldEqual("Registered");
    }

    [Theory]
    [InlineData("/parts/details/register.play")]
    [InlineData("../parts/details/register.play")]
    [InlineData("./details/register.play")]
    [InlineData("details/not-published*.play")]
    void should_refuse_disallowed_patterns_even_if_normalization_would_match(string pattern) =>
        CompileWith(pattern).Model.ShouldBeNull();

    static (SemanticCompilation? Model, IEnumerable<Diagnostic> Diagnostics) CompileWith(string pattern) =>
        CatalogModelCompilation.Compile(Entry, [Document(Entry, Application), Document(Registration, $"import \"{pattern}\"\n"), Document(Register, Slice)], null, "Library");

    static WorkspaceDocument Document(string path, string source) => ConformanceModelSources.Document(PortablePlayPath.Parse(path), Encoding.UTF8.GetBytes(source));
}
