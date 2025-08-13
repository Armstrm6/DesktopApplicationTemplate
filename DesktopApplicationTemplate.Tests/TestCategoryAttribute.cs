using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace DesktopApplicationTemplate.Tests;

[TraitDiscoverer("DesktopApplicationTemplate.Tests.TestCategoryDiscoverer", "DesktopApplicationTemplate.Tests")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class TestCategoryAttribute : Attribute, ITraitAttribute
{
    public TestCategoryAttribute(string category)
    {
        Category = category;
    }
    public string Category { get; }
}

public sealed class TestCategoryDiscoverer : ITraitDiscoverer
{
    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        var category = traitAttribute.GetNamedArgument<string>("Category");
        yield return new KeyValuePair<string, string>("Category", category);
    }
}
