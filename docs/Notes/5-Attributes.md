## Attributes in C# and .NET

In C#, _Attributes_ are used to inject metadata into [_Assemblies_](1-Packaging.md#assemblies).
Consider this code:

```C#
[Table("TeddyBear")]
[Index("Name", Name = "IX_TeddyBearName", IsUnique = true)]
public partial class TeddyBear
{
    [Key]
    public int Id { get; set; }
    // Rest of class
}
```

The `[Table]` and `[Index]` are attributes on the `TeddyBear` class, while the `[Key]`
is an attribute on the `Id` property.  Attributes can be applied to many features in C#:

* Assemblies
* Modules
* Classes (reference types)
* Structs (value types)
* Interfaces
* Enums
* Delegates
* Events
* Constructors
* Fields
* Properties
* Methods
* Parameters
* Generic Parameters
* Return Values

### Attributed Are Instances of Specialized Classes

An _Attribute_ is implemented using a standard .NET class.  Every Attribute class

* Inherits from `System.Attribute`
* Has a `[AttributeTargetAttribute] that describes what kinds of things the attribute 
  might target (from that list above)
* By convention, attribute class names end in _"Attribute"_ (e.g.,
  `AttributeNameAttribute`).  If a class name ends in _Attribute_, then the
  attribute can be used with a name that does not include the the trailing _Attribute_.

Consider this code that defines a _Special_ attribute:

```C#
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class SpecialAttribute : System.Attribute{
    // class definition goes here
}
```

It inherits from `System.Attribute`.  It can be used on either _Classes_ or 
_Properties_.  It is named `SpecialAttribute`, but it can be used like this
to annotate a class:

```C#
[Special]
public class MyClass {
    // class definition goes here
}
```

### Attributes Revealed via Reflection

The .NET Framework (and .NET Core) provide the means to _Reflect_ over assemblies, types
and their components.  _Reflection_ (as provided by the `System.Reflection` namespace)
lets programs _look in the mirror_ to see how they are constructed.

For example, this code:

```C#
var teddyBearType = typeof(TeddyBear);
var classAttributes = teddyBearType.GetCustomAttributes(false);
```

Will result in the `classAttributes` variable holding a reference to an array that 
contains both a `TableAttribute` and an `IndexAttribute` instance.

Similarly, if that code were to continue like:

```C#
var idProperty = teddyBearType.GetProperty("Id");
var idAttributes = Attribute.GetCustomAttributes(idProperty);
```

Then the `idAttributes` variable would reference an array with one member, an
instance of `KeyAttribute`.

In almost every case, attributes are used by some framework (the .NET Framework,
ASP.NET, WCF) to annotate some program element and let that annotation be
used by the framework (after discovery via reflection) to make the program
behave differently.

### Attribute Declarations are Effectively Constructor Invocations

Going back to the declaration of the `TeddyBear` class:

```C#
[Table("TeddyBear")]
[Index("Name", Name = "IX_TeddyBearName", IsUnique = true)]
public partial class TeddyBear
```

The `[Table("TeddyBear")]` declaration requires that the `TableAttribute` class
have a constructor that takes a string.  When a program reflects over the 
`TeddyBear` class and gets the class's custom attributes, an instance of a 
`TableAttribute` is created by using that constructor and and the string 
`"TeddyBear"`.

The `[Index]` declaration also requires that the `IndexAttribute` class
has a constructor that takes a string.  In addition, it must have two properties
* A string-valued property named `Name`, and
* A boolean property named `IsUnique`.

This construct:
```C#
[Index("Name", Name = "IX_TeddyBearName", IsUnique = true)]
```

will result in a call to that constructor, and once the object is constructed, 
set the properties whenever the custom attributes of the `TeddyBear` class are
evaluated.

All this information is stored in the metadata of the `TeddyBear` class in its
assembly; it is not stored in code.  As a result, there's no other way for
anything in an attribute class to be evaluated (i.e., other than at attribute
enumeration time).

Also, since attribute declarations have no real code associated with them, there
are restrictions on what can be used in attribute constructor expressions.
Constructor parameters and property values much be things that are known at 
compilation time:
* String literal (or literal expressions)
* Literal expressions for numeric types or booleans
* Enum values
* Constant type-valued expressions (`typeof(MyType)`)

**_And Finally..._**

If an attribute declaration includes empty parentheses -- or no parentheses at 
all -- then it will call the default constructor for the class.  Consider this
declaration of the `TeddyBear` class's `Id` property:

```C#
[Key]
public int Id { get; set; }
```

It is completely equivalent to:

```C#
[Key()]
public int Id { get; set; }
```

When `Id`'s custom attributes are enumerated, either of those declarations will
result in a `KeyAttribute` instance being constructed by the class's default
constructor.
