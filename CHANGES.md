Changelog
=========

Version 0.4.0
-------------

To be released.

 -  Removed `Bencodex.Types.CommonVariables` static class.


Version 0.3.0
-------------

Released on October 5, 2021.

 -  The package became to have an assembly for .NET Standard 2.1 besides
    an existing assembly for .NET Standard 2.0.  The new assembly purposes
    to support [nullable reference types].  [[#24]]
 -  `IValue` interface became to inherit `IEquatable<IValue>`.  [[#19]]
 -  `Bencodex.Types.Null` became a read-only struct.  [[#37]]
 -  Added `Bencodex.Types.Null.Value` read-only field.  [[#20], [#37]]
 -  `Bencodex.Types.Binary`'s internal representation became
    `ImmutableArray<byte>` instead of `byte[]`.  [[#39]]
    - `Binary` became to implement `IEquatable<ImmutableArray<byte>>`.
    - `Binary` became to implement `IComparer<ImmutableArray<byte>>`.
    - Added `Binary(ImmutableArray<byte>)` constructor.
    - `Binary.Value` property became obsolete.
    - Added `Binary.ByteArray` property.
    - Added `Binary.ToByteArray()` method.
 -  `Bencodex.Types.Dictionary` became a read-only struct.  [[#24]]
 -  `Bencodex.Types.Dictionary(IEnumerable<KeyValuePair<IKey, IValue>>)`
    constructor now has no default value for the parameter.  [[#24]]
 -  `Bencodex.Types.Dictionary.SetItem()` became to have more overloads.  Added
    overloads, which is listed below, return `Bencodex.Types.Dictionary` instead
    of `IImmutableDictionary<IKey, IValue>`.  Note that existing
    `SetItem(IKey, IValue)` method which implements
    `IImmutableDictionary<IKey, IValue>` is still remained as it had been.
    [[#7], [#40]]
     -  (`IKey`, `string`)  [[#7]]
     -  (`IKey`, `ImmutableArray<byte>`)  [[#40]]
     -  (`IKey`, `byte[]`)  [[#7]]
     -  (`IKey`, `long`)  [[#7]]
     -  (`IKey`, `ulong`)  [[#7]]
     -  (`IKey`, `bool`)  [[#7]]
     -  (`IKey`, `IEnumerable<IValue>`)  [[#7]]
     -  (`string`, `IValue`)  [[#7]]
     -  (`string`, `string`)  [[#7]]
     -  (`string`, `ImmutableArray<byte>`)  [[#40]]
     -  (`string`, `byte[]`)  [[#7]]
     -  (`string`, `long`)  [[#7]]
     -  (`string`, `ulong`)  [[#7]]
     -  (`string`, `bool`)  [[#7]]
     -  (`string`, `IEnumerable<IValue>`)  [[#7]]
     -  (`ImmutableArray<byte>`, `IValue`)  [[#7]]
     -  (`ImmutableArray<byte>`, `string`)  [[#7]]
     -  (`ImmutableArray<byte>`, `ImmutableArray<byte>`)  [[#40]]
     -  (`ImmutableArray<byte>`, `byte[]`)  [[#7]]
     -  (`ImmutableArray<byte>`, `long`)  [[#7]]
     -  (`ImmutableArray<byte>`, `ulong`)  [[#7]]
     -  (`ImmutableArray<byte>`, `bool`)  [[#7]]
     -  (`ImmutableArray<byte>`, `IEnumerable<IValue>`)  [[#7]]
     -  (`byte[]`, `IValue`)  [[#7]]
     -  (`byte[]`, `string`)  [[#7]]
     -  (`byte[]`, `ImmutableArray<byte>`)  [[#40]]
     -  (`byte[]`, `byte[]`)  [[#7]]
     -  (`byte[]`, `long`)  [[#7]]
     -  (`byte[]`, `ulong`)  [[#7]]
     -  (`byte[]`, `bool`)  [[#7]]
     -  (`byte[]`, `IEnumerable<IValue>`)  [[#7]]
 -  `Bencodex.Types.Dictionary.Add()` became to have more overloads.  Added
    overloads, which is listed below, return `Bencodex.Types.Dictionary` instead
    of `IImmutableDictionary<IKey, IValue>`.  Note that existing
    `Add(IKey, IValue)` method which implements
    `IImmutableDictionary<IKey, IValue>` is still remained as it had been.
    [[#7], [#40]]
      -  (`string`, `IValue`)  [[#7]]
      -  (`string`, `string`)  [[#7]]
      -  (`string`, `ImmutableArray<byte>`)  [[#40]]
      -  (`string`, `byte[]`)  [[#7]]
      -  (`string`, `long`)  [[#7]]
      -  (`string`, `ulong`)  [[#7]]
      -  (`string`, `bool`)  [[#7]]
      -  (`string`, `IEnumerable<IValue>`)  [[#7]]
      -  (`ImmutableArray<byte>`, `IValue`)  [[#40]]
      -  (`ImmutableArray<byte>`, `string`)  [[#40]]
      -  (`ImmutableArray<byte>`, `ImmutableArray<byte>`)  [[#40]]
      -  (`ImmutableArray<byte>`, `byte[]`)  [[#40]]
      -  (`ImmutableArray<byte>`, `long`)  [[#40]]
      -  (`ImmutableArray<byte>`, `ulong`)  [[#40]]
      -  (`ImmutableArray<byte>`, `bool`)  [[#40]]
      -  (`ImmutableArray<byte>`, `IEnumerable<IValue>`)  [[#40]]
      -  (`byte[]`, `IValue`)  [[#7]]
      -  (`byte[]`, `string`)  [[#7]]
      -  (`byte[]`, `byte[]`)  [[#7]]
      -  (`byte[]`, `ImmutableArray<byte>`)  [[#40]]
      -  (`byte[]`, `long`)  [[#7]]
      -  (`byte[]`, `ulong`)  [[#7]]
      -  (`byte[]`, `bool`)  [[#7]]
      -  (`byte[]`, `IEnumerable<IValue>`)  [[#7]]
 -  Added `Bencodex.Types.Dictionary[string]` indexer. [[#7]]
 -  Added `Bencodex.Types.Dictionary[ImmutableArray<byte>]` indexer. [[#40]]
 -  Added `Bencodex.Types.Dictionary[byte[]]` indexer. [[#7]]
 -  Added `Bencodex.Types.Dictionary.Empty` static property.  [[#7]]
 -  Added `Bencodex.Types.Dictionary.GetValue<T>(string)` method. [[#7]]
 -  Added `Bencodex.Types.Dictionary.GetValue<T>(ImmutableArray<byte>)` method. [[#40]]
 -  Added `Bencodex.Types.Dictionary.GetValue<T>(byte[])` method. [[#11]]
 -  `Bencodex.Types.Dictionary.TryGetKey()` became to fill its `out` parameter
    with an empty `Binary` value when it returns `false`.  [[#24]]
 -  `Bencodex.Types.Dictionary.TryGetValue()` became to fill its `out` parameter
    with a `Bencodex.Types.Null` value when it returns `false`.  [[#24]]
 -  Added `IValue.Inspection` property to get a JSON-like human-readable
    representation for the sake of debugging.  [[#12], [#13]]
 -  `ToString()` method of `IValue` subclasses became to return its `Inspection`
    with a prefix of the qualified class name.  [[#12], [#13]]
 -  `Binary` became a read-only struct.  [[#14]]
 -  Added `Binary(string, System.Text.Encoding)` constructor.  [[#14]]
 -  Fixed a bug that changing on an array returned by `Binary.Value` property
    had changed the `Binary` as well.  `Binary.Value` property became to
    return always a new copy of its internal array.  [[#14]]
 -  Added overloads to `Bencodex.Types.Dictionary.ContainsKey()`
    for the sake of convenience.  [[#15], [#40]]
     -  `Bencodex.Types.Dictionary.ContainsKey(string)`  [[#15]]
     -  `Bencodex.Types.Dictionary.ContainsKey(ImmutableArray<byte>)`  [[#40]]
     -  `Bencodex.Types.Dictionary.ContainsKey(byte[])`  [[#15]]
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
 -  Removed `Bencodex.Misc.ByteChunkQueue` class.  [[#28]]
 -  `Codec.Decode()` method was entirely rewritten to optimize.
    [[#28]]
 -  Optimized `Bencodex.Types.Binary.GetHashCode()` method. Now the hash code is
    calculated using the modified [FNV], and cached after it is once calculated.
    [[#28]]
 -  Added `IValue.EncodeToStream(Stream)` method.  [[#32]]
 -  `Bencodex.Types.Dictionary` became not to immediately realize the inner
    hash table, but do it when it needs (e.g., when to look up a key) instead.
    Note that this change does not cause any API changes, but just purposes
    faster instantiation.  [[#33], [#34]]
 -  `Bencodex.Misc.ByteArrayComparer` now implements
    `IComparer<ImmutableArray<byte>>` besides `IComparer<byte[]>`.  [[#39]]

[#7]: https://github.com/planetarium/bencodex.net/pull/7
[#11]: https://github.com/planetarium/bencodex.net/pull/11
[#12]: https://github.com/planetarium/bencodex.net/issues/12
[#13]: https://github.com/planetarium/bencodex.net/pull/13
[#14]: https://github.com/planetarium/bencodex.net/pull/14
[#15]: https://github.com/planetarium/bencodex.net/pull/15
[#19]: https://github.com/planetarium/bencodex.net/issues/19
[#20]: https://github.com/planetarium/bencodex.net/issues/20
[#23]: https://github.com/planetarium/bencodex.net/pull/23
[#24]: https://github.com/planetarium/bencodex.net/pull/24
[#25]: https://github.com/planetarium/bencodex.net/pull/25
[#26]: https://github.com/planetarium/bencodex.net/pull/26
[#28]: https://github.com/planetarium/bencodex.net/pull/28
[#32]: https://github.com/planetarium/bencodex.net/pull/32
[#33]: https://github.com/planetarium/bencodex.net/pull/33
[#34]: https://github.com/planetarium/bencodex.net/pull/34
[#37]: https://github.com/planetarium/bencodex.net/pull/37
[#39]: https://github.com/planetarium/bencodex.net/pull/39
[#40]: https://github.com/planetarium/bencodex.net/pull/40
[#44]: https://github.com/planetarium/bencodex.net/pull/44
[nullable reference types]: https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references
[RTL]: https://en.wikipedia.org/wiki/Right-to-left
[FNV]: https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function


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
