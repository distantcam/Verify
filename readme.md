<!--
GENERATED FILE - DO NOT EDIT
This file was generated by [MarkdownSnippets](https://github.com/SimonCropp/MarkdownSnippets).
Source File: /readme.source.md
To change this file edit the source file and then run MarkdownSnippets.
-->

# <img src="/src/icon.png" height="30px"> Verify

[![Build status](https://ci.appveyor.com/api/projects/status/dpqylic0be7s9vnm/branch/master?svg=true)](https://ci.appveyor.com/project/SimonCropp/Verify)
[![NuGet Status](https://img.shields.io/nuget/v/Verify.svg?cacheSeconds=86400)](https://www.nuget.org/packages/Verify/)

Verification tool to enable simple approval of complex models using [Json.net](https://www.newtonsoft.com/json).

<!-- toc -->
## Contents

  * [NuGet package](#nuget-package)
  * [Usage](#usage)
    * [Validating multiple instances](#validating-multiple-instances)
  * [Serializer settings](#serializer-settings)
    * [Default settings](#default-settings)
    * [Single quotes used](#single-quotes-used)
    * [QuoteName is false](#quotename-is-false)
    * [Empty collections are ignored](#empty-collections-are-ignored)
    * [Guids are scrubbed](#guids-are-scrubbed)
    * [Dates are scrubbed](#dates-are-scrubbed)
    * [Default Booleans are ignored](#default-booleans-are-ignored)
    * [Change defaults at the verification level](#change-defaults-at-the-verification-level)
    * [Changing settings globally](#changing-settings-globally)
    * [Scoped settings](#scoped-settings)
    * [Ignoring a type](#ignoring-a-type)
    * [Ignoring a instance](#ignoring-a-instance)
    * [Ignore member by expressions](#ignore-member-by-expressions)
    * [Ignore member by name](#ignore-member-by-name)
    * [Members that throw](#members-that-throw)
  * [Named Tuples](#named-tuples)
  * [Scrubbers](#scrubbers)
  * [File extension](#file-extension)
  * [Diff Tool](#diff-tool)
    * [Visual Studio](#visual-studio)
<!-- endtoc -->


## NuGet package

https://nuget.org/packages/ObjectApproval/


## Usage

Assuming this was verified:

<!-- snippet: before -->
<a id='snippet-before'/></a>
```cs
var person = new Person
{
    GivenNames = "John",
    FamilyName = "Smith",
    Spouse = "Jill",
    Address = new Address
    {
        Street = "1 Puddle Lane",
        Country = "USA"
    }
};

await Verify(person);
```
<sup>[snippet source](/src/Verify.Xunit.Tests/VerifyObjectSamples.cs#L48-L64) / [anchor](#snippet-before)</sup>
<!-- endsnippet -->

Then attempt to verify this:

<!-- snippet: after -->
<a id='snippet-after'/></a>
```cs
var person = new Person
{
    GivenNames = "John",
    FamilyName = "Smith",
    Spouse = "Jill",
    Address = new Address
    {
        Street = "1 Puddle Lane",
        Suburb = "Gotham",
        Country = "USA"
    }
};

await Verify(person);
```
<sup>[snippet source](/src/Verify.Xunit.Tests/VerifyObjectSamples.cs#L108-L125) / [anchor](#snippet-after)</sup>
<!-- endsnippet -->

The serialized json version of these will then be compared and you will be displayed the differences in the diff tool you have asked ApprovalTests to use. For example:

![SampleDiff](/src/SampleDiff.png)

Note that the output is technically not valid json. [Single quotes are used](#single-quotes-used) and [names are not quoted](#quotename-is-false). The reason for this is to make the resulting output easier to read and understand.


### Validating multiple instances

When validating multiple instances, an [anonymous type](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/anonymous-types) can be used for verification

<!-- snippet: anon -->
<a id='snippet-anon'/></a>
```cs
var person1 = new Person
{
    GivenNames = "John",
    FamilyName = "Smith"
};
var person2 = new Person
{
    GivenNames = "Marianne",
    FamilyName = "Aguirre"
};

await Verify(
    new
    {
        person1,
        person2
    });
```
<sup>[snippet source](/src/Verify.Xunit.Tests/VerifyObjectSamples.cs#L70-L90) / [anchor](#snippet-anon)</sup>
<!-- endsnippet -->

Results in the following:

<!-- snippet: VerifyObjectSamples.Anon.verified.txt -->
<a id='snippet-VerifyObjectSamples.Anon.verified.txt'/></a>
```txt
{
  person1: {
    GivenNames: 'John',
    FamilyName: 'Smith'
  },
  person2: {
    GivenNames: 'Marianne',
    FamilyName: 'Aguirre'
  }
}
```
<sup>[snippet source](/src/Verify.Xunit.Tests/VerifyObjectSamples.Anon.verified.txt#L1-L10) / [anchor](#snippet-VerifyObjectSamples.Anon.verified.txt)</sup>
<!-- endsnippet -->


## Serializer settings

Serialization settings can be customized at three levels:

 * Method: Will run the verification in the current test method.
 * Class: Will run for all verifications in all test methods for a test class.
 * Global: Will run for test methods on all tests.


### Default settings

The default serialization settings are:

<!-- snippet: defaultSerialization -->
<a id='snippet-defaultserialization'/></a>
```cs
var settings = new JsonSerializerSettings
{
    Formatting = Formatting.Indented,
    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    DefaultValueHandling = DefaultValueHandling.Ignore
};
```
<sup>[snippet source](/src/Verify.Xunit/Helpers/SerializationSettings.cs#L149-L158) / [anchor](#snippet-defaultserialization)</sup>
<!-- endsnippet -->


### Single quotes used

[JsonTextWriter.QuoteChar](https://www.newtonsoft.com/json/help/html/P_Newtonsoft_Json_JsonTextWriter_QuoteChar.htm) is set to single quotes `'`. The reason for this is that it makes approval files cleaner and easier to read and visualize/understand differences


### QuoteName is false

[JsonTextWriter.QuoteName](https://www.newtonsoft.com/json/help/html/P_Newtonsoft_Json_JsonTextWriter_QuoteName.htm) is set to false. The reason for this is that it makes approval files cleaner and easier to read and visualize/understand differences


### Empty collections are ignored

By default empty collections are ignored during verification.

To disable this behavior globally use:

```cs
Global.DontIgnoreEmptyCollections();
```


### Guids are scrubbed

By default guids are sanitized during verification. This is done by finding each guid and taking a counter based that that specific guid. That counter is then used replace the guid values. This allows for repeatable tests when guid values are changing.

<!-- snippet: guid -->
<a id='snippet-guid'/></a>
```cs
var guid = Guid.NewGuid();
var target = new GuidTarget
{
    Guid = guid,
    GuidNullable = guid,
    GuidString = guid.ToString(),
    OtherGuid = Guid.NewGuid(),
};

await Verify(target);
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Tests.cs#L24-L37) / [anchor](#snippet-guid)</sup>
<!-- endsnippet -->

Results in the following:

<!-- snippet: Tests.ShouldReUseGuid.verified.txt -->
<a id='snippet-Tests.ShouldReUseGuid.verified.txt'/></a>
```txt
{
  Guid: Guid_1,
  GuidNullable: Guid_1,
  GuidString: Guid_1,
  OtherGuid: Guid_2
}
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Tests.ShouldReUseGuid.verified.txt#L1-L6) / [anchor](#snippet-Tests.ShouldReUseGuid.verified.txt)</sup>
<!-- endsnippet -->

To disable this behavior globally use:

```cs
Global.SontScrubGuids();
```


### Dates are scrubbed

By default dates (`DateTime` and `DateTimeOffset`) are sanitized during verification. This is done by finding each date and taking a counter based that that specific date. That counter is then used replace the date values. This allows for repeatable tests when date values are changing.

<!-- snippet: Date -->
<a id='snippet-date'/></a>
```cs
var dateTime = DateTime.Now;
var dateTimeOffset = DateTimeOffset.Now;
var target = new DateTimeTarget
{
    DateTime = dateTime,
    DateTimeNullable = dateTime,
    DateTimeString = dateTime.ToString("F"),
    DateTimeOffset = dateTimeOffset,
    DateTimeOffsetNullable = dateTimeOffset,
    DateTimeOffsetString = dateTimeOffset.ToString("F"),
};

await Verify(target);
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Tests.cs#L441-L457) / [anchor](#snippet-date)</sup>
<!-- endsnippet -->

Results in the following:

<!-- snippet: Tests.ShouldReUseDatetime.verified.txt -->
<a id='snippet-Tests.ShouldReUseDatetime.verified.txt'/></a>
```txt
{
  DateTime: DateTime_1,
  DateTimeNullable: DateTime_1,
  DateTimeOffset: DateTimeOffset_1,
  DateTimeOffsetNullable: DateTimeOffset_1,
  DateTimeString: DateTimeOffset_2,
  DateTimeOffsetString: DateTimeOffset_2
}
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Tests.ShouldReUseDatetime.verified.txt#L1-L8) / [anchor](#snippet-Tests.ShouldReUseDatetime.verified.txt)</sup>
<!-- endsnippet -->

To disable this behavior globally use:

```cs
Global.DontScrubDateTimes();
```


### Default Booleans are ignored

By default values of `bool` and `bool?` are ignored during verification. So properties that equate to 'false' will not be written,

To disable this behavior globally use:

```cs
Global.DontIgnoreFalse();
```


### Change defaults at the verification level

`DateTime`, `DateTimeOffset`, `Guid`, `bool`, and empty collection behavior can also be controlled at the verification level: 

<!-- snippet: ChangeDefaultsPerVerification -->
<a id='snippet-changedefaultsperverification'/></a>
```cs
DontIgnoreEmptyCollections();
DontScrubGuids();
DontScrubDateTimes();
DontIgnoreFalse();
await Verify(target);
```
<sup>[snippet source](/src/Verify.Xunit.Tests/VerifyObjectSamples.cs#L16-L24) / [anchor](#snippet-changedefaultsperverification)</sup>
<!-- endsnippet -->


### Changing settings globally

To change the serialization settings for all verifications use `Global.ApplyExtraSettings()`:

<!-- snippet: ExtraSettings -->
<a id='snippet-extrasettings'/></a>
```cs
base.ApplyExtraSettings(jsonSerializerSettings =>
{
    jsonSerializerSettings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
    jsonSerializerSettings.TypeNameHandling = TypeNameHandling.All;
});
```
<sup>[snippet source](/src/Verify.Xunit.Tests/VerifyObjectSamples.cs#L95-L103) / [anchor](#snippet-extrasettings)</sup>
<!-- endsnippet -->


### Scoped settings

<!-- snippet: ScopedSerializer -->
<a id='snippet-scopedserializer'/></a>
```cs
var person = new Person
{
    GivenNames = "John",
    FamilyName = "Smith",
    Dob = new DateTime(2000, 10, 1),
};
DontScrubDateTimes();
var serializerSettings = BuildJsonSerializerSettings();
serializerSettings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
await Verify(person, jsonSerializerSettings: serializerSettings);
```
<sup>[snippet source](/src/Verify.Xunit.Tests/VerifyObjectSamples.cs#L30-L43) / [anchor](#snippet-scopedserializer)</sup>
<!-- endsnippet -->

Result:

<!-- snippet: VerifyObjectSamples.ScopedSerializer.verified.txt -->
<a id='snippet-VerifyObjectSamples.ScopedSerializer.verified.txt'/></a>
```txt
{
  GivenNames: 'John',
  FamilyName: 'Smith',
  Dob: '\/Date(970322400000+1000)\/'
}
```
<sup>[snippet source](/src/Verify.Xunit.Tests/VerifyObjectSamples.ScopedSerializer.verified.txt#L1-L5) / [anchor](#snippet-VerifyObjectSamples.ScopedSerializer.verified.txt)</sup>
<!-- endsnippet -->


### Ignoring a type

To ignore all members that match a certain type:

<!-- snippet: AddIgnoreType -->
<a id='snippet-addignoretype'/></a>
```cs
IgnoreMembersWithType<ToIgnore>();

var target = new IgnoreTypeTarget
{
    ToIgnore = new ToIgnore
    {
        Property = "Value"
    },
    ToInclude = new ToInclude
    {
        Property = "Value"
    }
};
await Verify(target);
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Tests.cs#L124-L141) / [anchor](#snippet-addignoretype)</sup>
<!-- endsnippet -->

Result:

<!-- snippet: Tests.IgnoreType.verified.txt -->
<a id='snippet-Tests.IgnoreType.verified.txt'/></a>
```txt
{
  ToInclude: {
    Property: 'Value'
  }
}
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Tests.IgnoreType.verified.txt#L1-L5) / [anchor](#snippet-Tests.IgnoreType.verified.txt)</sup>
<!-- endsnippet -->


### Ignoring a instance

To ignore instances of a type based on delegate:

<!-- snippet: AddIgnoreInstance -->
<a id='snippet-addignoreinstance'/></a>
```cs
IgnoreInstance<Instance>(x => x.Property == "Ignore");

var target = new IgnoreInstanceTarget
{
    ToIgnore = new Instance
    {
        Property = "Ignore"
    },
    ToInclude = new Instance
    {
        Property = "Include"
    },
};
await Verify(target);
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Tests.cs#L90-L107) / [anchor](#snippet-addignoreinstance)</sup>
<!-- endsnippet -->

Result:

<!-- snippet: Tests.AddIgnoreInstance.verified.txt -->
<a id='snippet-Tests.AddIgnoreInstance.verified.txt'/></a>
```txt
{
  ToInclude: {
    Property: 'Include'
  }
}
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Tests.AddIgnoreInstance.verified.txt#L1-L5) / [anchor](#snippet-Tests.AddIgnoreInstance.verified.txt)</sup>
<!-- endsnippet -->


### Ignore member by expressions

To ignore members of a certain type using an expression:

<!-- snippet: IgnoreMemberByExpression -->
<a id='snippet-ignorememberbyexpression'/></a>
```cs
IgnoreMember<IgnoreExplicitTarget>(x => x.Property);
IgnoreMember<IgnoreExplicitTarget>(x => x.Field);
IgnoreMember<IgnoreExplicitTarget>(x => x.GetOnlyProperty);
IgnoreMember<IgnoreExplicitTarget>(x => x.PropertyThatThrows);

var target = new IgnoreExplicitTarget
{
    Include = "Value",
    Field = "Value",
    Property = "Value"
};
await Verify(target);
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Tests.cs#L163-L178) / [anchor](#snippet-ignorememberbyexpression)</sup>
<!-- endsnippet -->

Result:

<!-- snippet: Tests.IgnoreMemberByExpression.verified.txt -->
<a id='snippet-Tests.IgnoreMemberByExpression.verified.txt'/></a>
```txt
{
  Include: 'Value'
}
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Tests.IgnoreMemberByExpression.verified.txt#L1-L3) / [anchor](#snippet-Tests.IgnoreMemberByExpression.verified.txt)</sup>
<!-- endsnippet -->


### Ignore member by name

To ignore members of a certain type using type and name:

<!-- snippet: IgnoreMemberByName -->
<a id='snippet-ignorememberbyname'/></a>
```cs
var type = typeof(IgnoreExplicitTarget);
IgnoreMember(type, "Property");
IgnoreMember(type, "Field");
IgnoreMember(type, "GetOnlyProperty");
IgnoreMember(type, "PropertyThatThrows");

var target = new IgnoreExplicitTarget
{
    Include = "Value",
    Field = "Value",
    Property = "Value"
};
await Verify(target);
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Tests.cs#L184-L200) / [anchor](#snippet-ignorememberbyname)</sup>
<!-- endsnippet -->

Result:

<!-- snippet: Tests.IgnoreMemberByName.verified.txt -->
<a id='snippet-Tests.IgnoreMemberByName.verified.txt'/></a>
```txt
{
  Include: 'Value'
}
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Tests.IgnoreMemberByName.verified.txt#L1-L3) / [anchor](#snippet-Tests.IgnoreMemberByName.verified.txt)</sup>
<!-- endsnippet -->


### Members that throw

Members that throw exceptions can be excluded from serialization based on the exception type or properties.

By default members that throw `NotImplementedException` or `NotSupportedException` are ignored.

Note that this is global for all members on all types.

Ignore by exception type:

<!-- snippet: IgnoreMembersThatThrow -->
<a id='snippet-ignoremembersthatthrow'/></a>
```cs
IgnoreMembersThatThrow<CustomException>();

var target = new WithCustomException();
await Verify(target);
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Tests.cs#L227-L234) / [anchor](#snippet-ignoremembersthatthrow)</sup>
<!-- endsnippet -->

Result:

<!-- snippet: Tests.CustomExceptionProp.verified.txt -->
<a id='snippet-Tests.CustomExceptionProp.verified.txt'/></a>
```txt
{}
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Tests.CustomExceptionProp.verified.txt#L1-L1) / [anchor](#snippet-Tests.CustomExceptionProp.verified.txt)</sup>
<!-- endsnippet -->

Ignore by exception type and expression:

<!-- snippet: IgnoreMembersThatThrowExpression -->
<a id='snippet-ignoremembersthatthrowexpression'/></a>
```cs
IgnoreMembersThatThrow<Exception>(
    x => x.Message == "Ignore");

var target = new WithExceptionIgnoreMessage();
await Verify(target);
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Tests.cs#L277-L285) / [anchor](#snippet-ignoremembersthatthrowexpression)</sup>
<!-- endsnippet -->

Result:

<!-- snippet: Tests.ExceptionMessageProp.verified.txt -->
<a id='snippet-Tests.ExceptionMessageProp.verified.txt'/></a>
```txt
{}
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Tests.ExceptionMessageProp.verified.txt#L1-L1) / [anchor](#snippet-Tests.ExceptionMessageProp.verified.txt)</sup>
<!-- endsnippet -->



## Named Tuples

Instances of [named tuples](https://docs.microsoft.com/en-us/dotnet/csharp/tuples#named-and-unnamed-tuples) can be verified using `VerifyTuple`.

Due to the use of [ITuple](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.ituple), this approach is only available an net472+ and netcoreapp2.2+.

Given a method that returns a named tuple:

<!-- snippet: MethodWithNamedTuple -->
<a id='snippet-methodwithnamedtuple'/></a>
```cs
static (bool Member1, string Member2, string Member3) MethodWithNamedTuple()
{
    return (true, "A", "B");
}
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Tests.cs#L67-L72) / [anchor](#snippet-methodwithnamedtuple)</sup>
<!-- endsnippet -->

Can be verified:

<!-- snippet: VerifyTuple -->
<a id='snippet-verifytuple'/></a>
```cs
await VerifyTuple(() => MethodWithNamedTuple());
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Tests.cs#L60-L64) / [anchor](#snippet-verifytuple)</sup>
<!-- endsnippet -->

Resulting in:

<!-- snippet: Tests.NamedTuple.verified.txt -->
<a id='snippet-Tests.NamedTuple.verified.txt'/></a>
```txt
{
  Member1: true,
  Member2: 'A',
  Member3: 'B'
}
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Tests.NamedTuple.verified.txt#L1-L5) / [anchor](#snippet-Tests.NamedTuple.verified.txt)</sup>
<!-- endsnippet -->


## Scrubbers

Scrubbers run on the final string prior to doing the verification action.

They can be defined at three levels:

 * Method: Will run the verification in the current test method.
 * Class: Will run for all verifications in all test methods for a test class.
 * Global: Will run for test methods on all tests.

Multiple scrubbers can bee defined at each level.

Scrubber are excited in reveres order. So the most recent added method scrubber through to earlies added global scrubber.

Global scrubbers should be defined only once at appdomain startup.

Usage:

<!-- snippet: scrubberssample.cs -->
<a id='snippet-scrubberssample.cs'/></a>
```cs
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;
using Xunit.Abstractions;

public class ScrubbersSample :
    VerifyBase
{
    [Fact]
    public Task Simple()
    {
        AddScrubber(s => s.Replace("Two", "B"));
        return VerifyText("One Two Three");
    }

    [Fact]
    public Task AfterJson()
    {
        var target = new ToBeScrubbed
        {
            RowVersion = "0x00000000000007D3"
        };

        AddScrubber(s => s.Replace("0x00000000000007D3", "TheRowVersion"));
        return Verify(target);
    }

    public ScrubbersSample(ITestOutputHelper output) :
        base(output)
    {
        AddScrubber(s => s.Replace("Three", "C"));
    }

    public static class ModuleInitializer
    {
        public static void Initialize()
        {
            Global.AddScrubber(s => s.Replace("One", "A"));
        }
    }
}
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Scrubbers/ScrubbersSample.cs#L1-L41) / [anchor](#snippet-scrubberssample.cs)</sup>
<!-- endsnippet -->

Results:

<!-- snippet: ScrubbersSample.Simple.verified.txt -->
<a id='snippet-ScrubbersSample.Simple.verified.txt'/></a>
```txt
A B C
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Scrubbers/ScrubbersSample.Simple.verified.txt#L1-L1) / [anchor](#snippet-ScrubbersSample.Simple.verified.txt)</sup>
<!-- endsnippet -->

<!-- snippet: ScrubbersSample.AfterJson.verified.txt -->
<a id='snippet-ScrubbersSample.AfterJson.verified.txt'/></a>
```txt
{
  RowVersion: 'TheRowVersion'
}
```
<sup>[snippet source](/src/Verify.Xunit.Tests/Scrubbers/ScrubbersSample.AfterJson.verified.txt#L1-L3) / [anchor](#snippet-ScrubbersSample.AfterJson.verified.txt)</sup>
<!-- endsnippet -->


## File extension

The default file extension is `.txt`. So the resulting verified file will be `TestClass.TestMethod.verified.txt`.

It can be overridden at two levels:

 * Method: Change the extension for the current test method.
 * Class: Change the extension all verifications in all test methods for a test class.

Usage:

<!-- snippet: ExtensionSample.cs -->
<a id='snippet-ExtensionSample.cs'/></a>
```cs
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;
using Xunit.Abstractions;

public class ExtensionSample :
    VerifyBase
{
    [Fact]
    public async Task AtMethod()
    {
        UseExtensionForText(".xml");
        await VerifyText(@"<note>
<to>Joe</to>
<from>Kim</from>
<heading>Reminder</heading>
</note>");
    }

    [Fact]
    public async Task InheritedFromClass()
    {
        await VerifyText(@"{
    ""fruit"": ""Apple"",
    ""size"": ""Large"",
    ""color"": ""Red""
}");
    }

    public ExtensionSample(ITestOutputHelper output) :
        base(output)
    {
        UseExtensionForText(".json");
    }
}
```
<sup>[snippet source](/src/Verify.Xunit.Tests/ExtensionSample.cs#L1-L35) / [anchor](#snippet-ExtensionSample.cs)</sup>
<!-- endsnippet -->

Result in two files:

<!-- snippet: ExtensionSample.InheritedFromClass.verified.json -->
<a id='snippet-ExtensionSample.InheritedFromClass.verified.json'/></a>
```json
{
    "fruit": "Apple",
    "size": "Large",
    "color": "Red"
}
```
<sup>[snippet source](/src/Verify.Xunit.Tests/ExtensionSample.InheritedFromClass.verified.json#L1-L5) / [anchor](#snippet-ExtensionSample.InheritedFromClass.verified.json)</sup>
<!-- endsnippet -->

<!-- snippet: ExtensionSample.AtMethod.verified.xml -->
<a id='snippet-ExtensionSample.AtMethod.verified.xml'/></a>
```xml
<note>
<to>Joe</to>
<from>Kim</from>
<heading>Reminder</heading>
</note>
```
<sup>[snippet source](/src/Verify.Xunit.Tests/ExtensionSample.AtMethod.verified.xml#L1-L5) / [anchor](#snippet-ExtensionSample.AtMethod.verified.xml)</sup>
<!-- endsnippet -->


## Diff Tool

Controlled via environment variables.

 * `VerifyDiffProcess`: The process name. Short name if the tool exists in the current path, otherwise the full path.
 * `VerifyDiffArguments`: The argument syntax to pass to the process. Must contain the strings `{receivedPath}` and `{verifiedPath}`.


### Visual Studio

```
setx VerifyDiffProcess "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe"
setx VerifyDiffArguments "/diff {receivedPath} {verifiedPath}"
```


## Release Notes

See [closed milestones](../../milestones?state=closed).


## Icon

[Helmet](https://thenounproject.com/term/helmet/9554/) designed by [Leonidas Ikonomou](https://thenounproject.com/alterego) from [The Noun Project](https://thenounproject.com).
