Bencodex codec for .NET
=======================

[![Build Status][]][build]

This library implements [Bencodex] serialization format which extends
[Bencoding].

[Bencodex]: https://github.com/planetarium/bencodex
[Bencoding]: http://www.bittorrent.org/beps/bep_0003.html#bencoding
[build]: https://github.com/planetarium/bencodex.net/actions
[Build Status]: https://github.com/planetarium/bencodex.net/workflows/build/badge.svg?event=push


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

Distributed under [LGPL 2.1] or later.

[LGPL 2.1]: https://www.gnu.org/licenses/lgpl-2.1.html
