## Properties and Indexers in C#

_Properties_ are first-class members of _classes_, _structs_ and _interfaces_ in C#, just
like _fields_ and _methods_.

Properties are accessed by name, and do not use a function-like syntax when they are accessed.
Consider this code:

```C#
var aString = "Hello World";
var len = aString.Length;
Assert (len == 11);
```

In the second line, `Length` is a read-only property of the `System.String` type.  When 
`aString.Length` is accessed, the _getter_ associated with the `Length` property is run, 
and an integer is returned.

Traditionally (i.e., 20+ years ago when the language first shipped), a type would define its
properties this way:

```C#
public class SomeClass {
    public SomeClass() {...}    //default constructor
    // other code

    private bool _hasBeenAccessed = false;      //private backing field
    public bool HasBeenAccessed {               //property declaration
        get { return _hasBeenAccessed; }        //getter implementation
        set { _hasBeenAccessed = value; }       //setter implementation
    }
}
```

In the example, the word `value` is a keyword.  It represents the value to which the property
has been assigned.  The types of the _getter_ and _setter_ always agree (i.e., when you set
this property, you set it with a bool, when you read/get it, the result is a bool).

As long as the "always the same type" rule is met, the getters and setters can have any code
the programmer wants.  Generally, it's best if they have few side-effects and do not require
a lot of resources to execute.

### Auto-Properties

Though getters and setters can have any code, the pattern above (a private backing field,
and minimalist getters and setters) was used in the vast majority of code.  As a result, a
simpler syntax was devised.  That code would now be written (using _auto-properties_) as:

```C#
public class SomeClass {
    public SomeClass() {...}    //default constructor
    // other code

    public bool HasBeenAccessed { get; set; } = false;
}
```

That simpler code compiles to the same code as what's shown above (with the exception
that the private backing field will be named with a name only the compiler knows).  The
initialization code (the `= false;`) is optional.  If there is no initializer, the 
semi-colon is not needed (semi-colons rarely appear after closing braces in C#).

### Using Lambda Syntax

Once the `=>` syntax of Lambdas became widely known, it turned up in other parts of the
language.  These include minimal function definitions and simple property implementations.
For example, this is valid:

```C#
public class SomeClass {
    public SomeClass() {...}    //default constructor
    // other code

    private bool _hasBeenAccessed = false;      //private backing field
    public bool HasBeenAccessed {               //property declaration
        get => _hasBeenAccessed;                //getter implementation
        set => _hasBeenAccessed = value;        //setter implementation
    }
}
```
Of course, you won't see anything quite this simple in real code - an auto-property
would be used instead.  However, it's an example of using `=>` with properties.

### Private, Protected, Public and Internal

A property has an 
[`Access Modifier`](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/access-modifiers) 
like most other type members, and the modifiers generally have the same meaning:

| Modifier | Meaning |
|---|---|
| **private** | Can be accessed only by code from within the defining type (the default)
| **protected** | Can be accessed within the defining type and by sub-classes
| **public** | Can be accessed by any code
| **internal** | Can be accessed by any code in the _same assembly_ as the defining type

For completeness there are two other, much more rarely used, access modifiers:

| Modifier | Meaning |
|---|---|
| **protected internal** | Effectively `protected` OR `internal`
| **private protected** | Effectively `protected` AND `internal`

### Property Access Modifier Rules

Since properties have both _getters_ and _setters_, you can have different access rules
for both accessors.  If, for example, you want public access to the getter, but access
to the setter restricted to members of the class, you can say:

```C#
public bool HasBeenAccessed { get; private set; } = false;
```
The rule here is to give the property the more liberal access, and then restrict the 
accessor that needs it.  This makes sense with the `HasBeenAccessed` property; it may 
be that various code within the `SomeClass` class sets the value to true to indicate
that the instance has been accessed.  It may make no sense to set it from outside the
class.

If the only place that a property will be written to is in the type's constructor(s),
Then a _setter_ can be elided - _Constructors can call an auto-property's setter even
if it's not declared_.

### Get, Set and Init

The normal case for properties (like the `HasBeenAccessed` we've been working with) is
that the property is both settable and gettable (perhaps with different access modifiers).

It's possible to have _Read-Only_ properties (the property has a getter, but no setter),
and _Write-Only_ properties (with a setter but no getter - this is a rarer situation).

Finally, the `set` keyword in an auto-property can be replaced with `init`.  This indicates 
that a property is settable, but only at initialization time.  An attempt to set the
value of this property after initialization will result in a compiler error.  This provides
a way to make an object (or some properties of an object) _immutable_.

For example, if we change the definition of `HasBeenAccessed` to:

```C#
public bool HasBeenAccessed { get; init; } = false;
```

then this is possible:

```C#
var myObject = new SomeClass { HasBeenAccessed = true };
```

But, this will cause an error:
```C#
myObject.HasBeenAccessed = false;
```

## _Indexers_ (or Indexed Properties)

C# supports a feature called 
[_Indexers_](https://learn.microsoft.com/en-us/dotnet/csharp/indexers).  Indexers allow objects 
(typically objects that contain collections) to act as if they were `Arrays` (or other 
indexable collections - for example, `Dictionaries`).  The `List<T>` class has an indexer that
uses an integer as the index.  It is declared like:

```C#
// external declaration of the List<T> indexer:
public T this[int index] { get; set; }
```

Similarly, the `Dictionary<TKey, TValue>` class has an indexer declared as:

```C#
// external declaration of the Dictionary<TKey, TValue> indexer:
public TValue this[TKey key] { get; set; }
```

> ---
> **Notes**:  
> * In both cases, the getter and setter will be non-trivial, the declaration only shows that
>   the indexer is read/write.
> * Note the use of the keyword `this` in the declarations; this emphasizes that the Indexer's
>   nature is that it is treating the entire object (aka `this`) as the source of the data.
>  ---

Those two definitions allow code like:

```C#
var myList = new List<int>{ 1, 2, 3, 4, 5};
var item3 = myList[3];
Assert (item3 == 4);

//and

var myDict = new Dictionary<char, string>{ {'a', "A" }, {'b', "B" }, {'c', "C" }, };
var itemB = myDict['b'];
Assert (itemB == "B");
```

The implementation of an indexer looks like what you would expect from a property named
`this`, with a parameter.  Here's an example:

```C#
public int this[string key]
{
    get { return storage.Find(key); }
    set { storage.SetAt(key, value); }
}
```

A single class can have multiple indexers, each with a different parameter signature.  For
example a dictionary-like object might have an indexer that looked like:

```C#
// a possible int-parametered indexer on a PseudoDictionary<TKey, TValue> (Note, does not exist)
public KeyValuePair<TKey,TValue> this[int index] { get; set; }
```

It's also possible to have an indexer that has more than a single parameter.  This allows your
objects to pretend to be multi-dimensional arrays.  For example, if you wanted to create a
`SparseMatrix` type, you could use this.