Changelog
=========

Version 0.3.0
-------------

To be released.

 -  The package became to have an assembly for .NET Standard 2.1 besides
    an existing assembly for .NET Standard 2.0.  The new assembly purposes
    to support [nullable reference types].  [[#24]]
 -  `Bencodex.Types.Dictionary` became a readonly struct.  [[#24]]
 -  `Bencodex.Types.Dictionary(IEnumerable<KeyValuePair<IKey, IValue>>)`
    constructor now has no default value for the parameter.  [[#24]]
 -  `Bencodex.Types.Dictionary.SetItem()` became to have more overloads.  [[#7]]
     -  Added overloads, which is listed below,
        return `Bencodex.Types.Dictionary` instead of
        `IImmutableDictionary<IKey, IValue>`. Note that existing
        `SetItem(IKey, IValue)` method which implements
        `IImmutableDictionary<IKey, IValue>` is still remained as it had been.
     -  (`IKey`, `string`)
     -  (`IKey`, `byte[]`)
     -  (`IKey`, `long`)
     -  (`IKey`, `ulong`)
     -  (`IKey`, `bool`)
     -  (`IKey`, `IEnumerable<IValue>`)
     -  (`string`, `IValue`)
     -  (`string`, `string`)
     -  (`string`, `byte[]`)
     -  (`string`, `long`)
     -  (`string`, `ulong`)
     -  (`string`, `bool`)
     -  (`string`, `IEnumerable<IValue>`)
     -  (`byte[]`, `IValue`)
     -  (`byte[]`, `string`)
     -  (`byte[]`, `byte[]`)
     -  (`byte[]`, `long`)
     -  (`byte[]`, `ulong`)
     -  (`byte[]`, `bool`)
     -  (`byte[]`, `IEnumerable<IValue>`)
 -  `Bencodex.Types.Dictionary.Add()` became to have more overloads.  [[#7]]
      -  Added overloads, which is listed below,
         return `Bencodex.Types.Dictionary` instead of
         `IImmutableDictionary<IKey, IValue>`. Note that existing
         `Add(IKey, IValue)` method which implements
         `IImmutableDictionary<IKey, IValue>` is still remained as it had been.
      -  (`string`, `IValue`)
      -  (`string`, `string`)
      -  (`string`, `byte[]`)
      -  (`string`, `long`)
      -  (`string`, `ulong`)
      -  (`string`, `bool`)
      -  (`string`, `IEnumerable<IValue>`)
      -  (`byte[]`, `IValue`)
      -  (`byte[]`, `string`)
      -  (`byte[]`, `byte[]`)
      -  (`byte[]`, `long`)
      -  (`byte[]`, `ulong`)
      -  (`byte[]`, `bool`)
      -  (`byte[]`, `IEnumerable<IValue>`)
 -  Added `Bencodex.Types.Dictionary[string]` indexer. [[#7]]
 -  Added `Bencodex.Types.Dictionary[byte[]]` indexer. [[#7]]
 -  Added `Bencodex.Types.Dictionary.Empty` static property.  [[#7]]
 -  Added `Bencodex.Types.Dictionary.GetValue<T>(byte[])` method. [[#11]]
 -  `Bencodex.Types.Dictionary.TryGetKey()` became to fill its `out` parameter
    with an empty `Binary` value when it returns `false`.  [[#24]]
 -  `Bencodex.Types.Dictionary.TryGetValue()` became to fill its `out` parameter
    with a `Bencodex.Types.Null` value when it returns `false`.  [[#24]]
 -  Added `IValue.Inspection` property to get a JSON-like human-readable
    representation for the sake of debugging.  [[#12], [#13]]
 -  `ToString()` method of `IValue` subclasses became to return its `Inspection`
    with a prefix of the qualified class name.  [[#12], [#13]]
 -  `Binary` became a readonly struct.  [[#14]]
 -  Added `Binary(string, System.Text.Encoding)` constructor.  [[#14]]
 -  Fixed a bug that changing on an array returned by `Binary.Value` property
    had changed the `Binary` as well.  `Binary.Value` property became to
    return always a new copy of its internal array.  [[#14]]
 -  Added overloads to `Bencodex.Types.Dictionary.ContainsKey()`
    for the sake of convenience.  [[#15]]
     -  `Bencodex.Types.Dictionary.ContainsKey(string)`
     -  `Bencodex.Types.Dictionary.ContainsKey(byte[])`
 -  `Bencodex.Types.Integer(string value)` constructor was replaced by
    `Bencodex.Types.Integer(string value, IFormatProvider? provider = null)`
    which is still compatible in source code level.  [[#23]]
 -  Fixed encoding and decoding bugs that had been occurred on some locales
    writing [RTL] scripts, e.g., Arabic (`ar`).  [[#23]]
 -  Added `Bencodex.Types.List[int]` indexer.  [[#25]]
 -  Added `Bencodex.Types.List.Count` property.  [[#25]]
 -  `Bencodex.Types.List.Add()` became to have more overloads,
     which return `Bencodex.Types.List` so that it is convenient to chain
     method calls.  [[#26]]
     -  `Add(IValue)`
     -  `Add(string)`
     -  `Add(byte[])`
     -  `Add(bool)`
     -  `Add(BigInteger)`
 -  Added `Bencodex.Types.List.Empty` static property.  [[#26]]

[#7]: https://github.com/planetarium/bencodex.net/pull/7
[#11]: https://github.com/planetarium/bencodex.net/pull/11
[#12]: https://github.com/planetarium/bencodex.net/issues/12
[#13]: https://github.com/planetarium/bencodex.net/pull/13
[#14]: https://github.com/planetarium/bencodex.net/pull/14
[#15]: https://github.com/planetarium/bencodex.net/pull/15
[#23]: https://github.com/planetarium/bencodex.net/pull/23
[#24]: https://github.com/planetarium/bencodex.net/pull/24
[#25]: https://github.com/planetarium/bencodex.net/pull/25
[#26]: https://github.com/planetarium/bencodex.net/pull/26
[nullable reference types]: https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references
[RTL]: https://en.wikipedia.org/wiki/Right-to-left


Version 0.2.0
-------------

Released on October 30, 2019.

 -  Relicensed under [LGPL 2.1] or later.
 -  Added more implicit conversions.  [[#1]]
     -  `Integer` ↔ `short`
     -  `Integer` ↔ `ushort`
     -  `Integer` → `int`
     -  `Integer` ↔ `uint`
     -  `Integer` ↔ `long`
     -  `Integer` ↔ `ulong`
     -  `Binary` → `bytes`
     -  `Boolean` ↔ `bool`

[LGPL 2.1]: https://www.gnu.org/licenses/lgpl-2.1.html
[#1]: https://github.com/planetarium/bencodex.net/pull/1

Version 0.1.0
-------------

Released on December 17, 2018.

 -  Implemented the [specification version 1][bencodex-1.0].

[bencodex-1.0]: https://github.com/planetarium/bencodex/tree/1.0
