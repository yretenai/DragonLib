# DragonLib

Common library for my projects.

Note: This library is not designed to be used by others, and is not guaranteed to be stable.

## Attribution

This library is contains code from the following libraries:

* [LibHac's Lz4 implementation](https://github.com/Thealexbarney/LibHac/blob/e0b482f44b5f225cfe29af0b80186bcb8895806c/src/LibHac/Util/Lz4.cs)[^1] (MIT License)
* [Tomáš Pažourek's Natural Language Sort Extension](https://github.com/tompazourek/NaturalSort.Extension/blob/7e99f4e52b2e8e16e3de542f2fce547d4abe047a/src/NaturalSort.Extension/NaturalSortComparer.cs)[^2] (MIT License)

The appropriate licenses are included in the `ATTRIBUTION` file.

[^1]: Code has been modified to exit on buffer overflow and the ability to return offsets for both the compressed and decompressed stream. This is known as unsafe Lz4 reading.
[^2]: Code has been modified to separate the sorting direction used by string and numeric comparison.
