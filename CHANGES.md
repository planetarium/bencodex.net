Changelog
=========

Version 0.3.0
-------------

To be released.

 -  `Bencodex.Types.Dictionary.SetItem()` became
    to have more overloads.  [[#7]]
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
 -  Added `Bencodex.Types.Dictionary.GetValue<T>(byte[])` method. [[#11]]

[#7]: https://github.com/planetarium/bencodex.net/pull/7


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
