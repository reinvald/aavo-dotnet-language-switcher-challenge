## C# Enumerated Types

C# has had enumerated types and the `enum` keyword since it first shipped.  They are modeled
on the original `C` language `enum` capability.

In C#, an `enum` is an integral type that maps integers (or other integral types) to
symbolic names.  Consider this code:

```C#
public enum Answer {
    No,
    Yes,
    Maybe,
}
```

By default, enums are based on `int`, so every `Answer` is a positive 32-bit integer.  Also
by default, the first value in the list is considered the `0-value` and is assigned 0 as a 
value.  Subsequent elements get values one more than the previous value (so `No` is `0`, `Yes` 
is `1` and `Maybe` is `2`).

You can change the type of integer the enumerated type is based on by using an 
*inheritance-like* syntax.  To used a signed 8-bit quantity for our `Answer` type, declare
it this way:

```C#
public enum Answer : sbyte { /* item definitions */ }
```

In general, in a 32 or 64 bit world, there isn't much benefit to non-`int` enums unless you
intend on packing many of them into type definition.  Most enums you see will use the 
default `int`.

You can change the mapping of numeric values to symbols by assigning each entry a value:

```C#
public enum Answer {
    No = 0,
    Yes = 100,
    Maybe = 200,
}
```

If the `Maybe` entry was not assigned `200` in the example above, it would have been 
assigned one more than `Yes`, i.e., `101`.

If you don't have a symbol assigned to `0`, zero remains the default value of the type
(as it does for every value-type).  It's best to alway have an explicit zero-valued member.
You will often see `None` or `Default` as the first element in an `enum` definition.

The values of an enum-valued variable are not constrained the the values listed in the
definition of the `enum`.  Using the original (0, 1, 2) definition of `Answer`, this is 
perfectly good code:

```C#
var goodAnswer = (Answer)2;
var badAnswer = (Answer)22;
```
If you look at those two variables in the debugger, you will see `goodAnswer` as `Maybe`,
while `badAnswer` will be `22`.  A better way of expressing the `Maybe` value is:

```C#
var goodAnswer = Answer.Maybe;
```

An enumerated type definition can be a top-level type, or it can be a member of another
type.  For example, if I made the `Answer` type a member of a class named `MyClass`:

```C#
public class MyClass {
    // various members of MyClass
    public enum Answer {
        No,
        Yes,
        Maybe
    }
}
```

In this case, you could do the assignment above as:

```C#
var goodAnswer = MyClass.Answer.Maybe;
```

### The `[Flags]` Attribute and Enums

Enumerated types can be used to express flag values.  Enum types to be used this way
are marked with the `[Flags]` [Attribute](./5-Attributes.md).

Consider a scheduling system where you want track what is to happen on what day, but 
a single thing can be scheduled on multiple days.  You could create a type like:

```C#
[Flags]
public enum Days
{
    None      = 0b_0000_0000,  // decimal: 0,  hex: 0x0
    Monday    = 0b_0000_0001,  // decimal: 1,  hex: 0x1
    Tuesday   = 0b_0000_0010,  // decimal: 2,  hex: 0x2
    Wednesday = 0b_0000_0100,  // decimal: 4,  hex: 0x4
    Thursday  = 0b_0000_1000,  // decimal: 8,  hex: 0x8
    Friday    = 0b_0001_0000,  // decimal: 16, hex: 0x10
    Saturday  = 0b_0010_0000,  // decimal: 32, hex: 0x20
    Sunday    = 0b_0100_0000,  // decimal: 64, hex: 0x40
    Weekend   = Saturday | Sunday,
    Weekday   = Monday | Tuesday | Wednesday | Thursday | Friday
}
```
In this case, if you were to look at the integer value of `Days.Weekday`, you would see
`0b1_1111` (or `31` decimal).

The `[Flags]` attribute is known by the compiler and the runtime.  Flags enabled
enums are treated differently.  For example, if you look at these variables in the
debugger:

```C#
var twoDays = Days.Monday | Days.Friday;
var asString = twoDays.ToString();
```
You will see `twoDays` rendered as `Monday | Friday` and the value of `asString`
as `"Monday, Friday"`.

> ---
> **Note**: Integer Formats and Separators
> 
> ---
> Integers can be written using decimal (base-10) numbering, hexadecimal (base-16)
> numbering or binary (base-2) numbering.  These three variables all have the same
> value:
> ```C#
> var asDecimal = 1234;             //decimal
> var asHex = 0x4D2;                //hexadecimal (0x-prefix)
> var asBinary = 0b10011010010;     //binary (0b-prefix)
> ```
> An underscore can be used as a separator in integers for readability.  The 
> underscores are ignored by the compiler when determining value.  So, for example
> ```C#
> var bigNumber = 12_345_678;       // == 12,245,678
> var binary = 0b100_1101_0010;     // it's still 1234 decimal
> var bigHex = 0x1234_5678_9ABC;
> var oneCrore = 1_00_00_000;       // Works with Indian Lakh/Crore notation
> ```
> ---


### Enumerated Types are Effectively Sealed, but...

You can't add any member methods to an `enum`, nor anything else other than enum values.
However, you can add behavior to a particular enumerated type using 
[Extension Methods](./2-AboutLinq.md#c-30-fundamentals-needed-by-linq). You can also
add behavior to all enumerated types using a generic extension method.

### All Enumerated Typed Inherit from System.Enum

As mentioned in the article on [the Type System](./0-TypeSystem.md), all enumerated types
inherit from `System.Enum`.  System.Enum is an abstract reference type, but each `enum`
type is a value type.

Inheriting from `System.Enum` provides `enum` types with a set of services.  Here is a 
selection of the methods available from `System.Enum`

| Method(s) | Notes |
|--- |---
| `CompareTo`, `Equals` | Standard `IComparable` and `IEquatable` implementations
| `GetName` | Gets the name of an enum value as a string
| `GetNames` | Gets all of the possible names for an enum type as an array of strings
| `GetValues` | Gets all of the possible values for an enum type as an array of the underlying type
| `HasFlag` | Tests whether the bit pattern match the bits in the enum instance
| `IsDefined` | Tests whether the value passed in is a valid defined value for the enum type
| `ToString` | Converts an enum value to a string
| `Parse`, `TryParse` | Converts a string to an enum value (if possible)

