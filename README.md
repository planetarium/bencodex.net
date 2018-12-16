Bencodex codec for .NET
=======================

This library implements [Bencodex] serialization format which extends
[Bencoding].

[Bencodex]: https://github.com/planetarium/bencodex
[Bencoding]: http://www.bittorrent.org/beps/bep_0003.html#bencoding


Usage
-----

It currently provides only the most basic encoder and decoder.  See also
these methods:

 -  `Bencodex.Codec.Encode(Bencodex.Types.IValue, System.IO.Stream)`
 -  `Bencodex.Codec.Encode(Bencodex.Types.IValue)`
 -  `Bencodex.Codec.Decode(System.IO.Stream)`
 -  `Bencodex.Codec.Decode(System.Byte[])`

It will provide type-extensible higher-level APIs as well in the future.


License
-------

Distributed under [LGPLv3] or later.

[LGPLv3]: https://www.gnu.org/licenses/lgpl-3.0.html
