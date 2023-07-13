## C# Generic Types and Functions

Generic types in computer languages provide a way to apply the same algorithm to multiple
types.  The classic example is a generic collection, like `List<T>` in C#.

The `List<T>` type provides the semantics of a list (adding items, removing items, enumerating
items) without restriction on the type of items in the list.  A `List<int>` and a 
`List<string>` each provide the same list semantics, but only accept `int` or `string` 
arguments respectively.

Generic types have been around about 40 years (they were included in the original Ada
specification at the end of the 1970s).  C++ gained templates in the 1990s.  Both Java and 
the .NET languages got generic types in the mid-2000s.

### C++ Templates

Genericity in C++ is achieved by the compiler.  Everything about the generic type must be
known at compile time.  In nearly every implementation, this means that the source for
the generic type (like `List<T>`) must be available as a header file (a _.H File_) when
a consumer program instantiates a generic type.

When a program instantiates a generic type, the full type is compiled.  The output of a
compiler will have `List<int>` and `List<string>` and `List<Whatever>` as separate,
unrelated types.  Tools further down the toolchain (typically the linker) look at the
output from the compilation to do a binary comparison of compiled types.  That comparison
is looking for identical code, so that the can eliminate redundant
implementations.  This result in four things:

* C++ compilers are very resource intensive (although recent toolsets produce
  efficient binary images)
* C++ generics are very flexible (there are many specialization options including
  partial specialization).  Rules like SFINAE (*Specialization Failure is not an Error*)
  are impossible in other languages.
* The only way to distribute code for a generic type is to distribute _the code_ for
  the type; there's no binary distribution channel possible
* Fixes to library code must be distributed as source and be recompiled into each
  application that used that library

Some have called C++ templates the most comprehensive and capable macro language ever 
devised.

### Java Generics

Java generics use *Type Erasure* to implement their generic types.  When a programmer
uses a `List<string>`, the compiler manages the casting required to make sure that only
strings go into the list, and that retrievals from the list return only strings.
However, that all happens in the compiler, to the JVM, a `List<string>` and a `List<int>`
are the same type, a list of objects - the individual types are *erased* between
the compiler and the back end.  This has a few consequences:

* Full type information is not available at runtime via reflection
* Types like `int` must be boxed when *input* to generic objects and *unboxed* when 
  returned from generics

Java allows constraints on generic type parameters using the `super` and `extends`
keywords.

### .NET Generics

C# and VB added generic types in their version 2.0 release in the fall of 2005.
In .NET languages (including C#), generic types (like `List<T>`) are first class
elements in the language; this is valid C#:

```C#
var genericListType = typeof(List<>);
```
A program can use that `genericListType` variable at runtime to create a `List<int>` 
via *reification*; the process in which a generic type and a type parameter are
united and *crowned* as a full-on type (`List<T>`).

If you look into the assembly assembly that defines a generic type, 
you will see full metadata for the generic type.  If you look in the 
`System.Collections.Generics` assembly, you will see metadata that defines `List<T>`
and all of the other generic collections.  When your code consumes/references those
assemblies, that metadata is used to get access to the entire type definition. 

### Constraining Type Parameters

Collections like `List<T>` are easily implemented; they work with instances of the 
generic type parameter without knowing anything about the type.  However, if the
code in the generic class (or generic function) needs to work with instances of 
the type parameter, the programmer needs to inform the compiler about what needs to be
done.

Consider a function that takes two instances of a class, compares them.  If the first
instance is greater than the second, it returns that instance.  If not, it creates 
a new class instance and returns that instead.  Something like:

```C#
// NOTE: this code will not compile
public T CompareAndReturn<T> (T first, T second)
{
    if (first.CompareTo(second) > 0){
        return first;
    }
    return new T();
}
```
As noted, this code will not compile.  The reason is that the compiler only knows that `T`
is a type, not that instances of `T` have a `CompareTo` method, nor that `T` has a default
constructor.

To fix this, we can *constrain* `T` so that it must be a type that implements `IComparable<T>`
and that has a default constructor.  We do that by adding constraints on the method
declaration:

```C#
public T CompareAndReturn<T> (T first, T second) where T : IComparable<T>, new()
```
Since `T` must implement `IComparable<T>`, the compiler knows that instances of `T`
will have an `int CompareTo(T other)` method.  Because of the `new()` constraint, the 
compiler knows that `T` has a default constructor.

**Constraint List**

| Constraint | Explanation |
|---|---
| `struct` | `T` must be a non-nullable value type. Cannot be combined with `new()`
| `class` | `T` must be a reference type (typically a class). If using nullable reference types, `T` must be a non-nullable reference type
| `class?` | `T` must be a reference type (nullable or not)
| `new()` | `T` must have a public default constructor (not needed for `struct`)
| *\<baseClassName>* | `T` must inherit (directly or indirectly) from that *base class*. If using nullable reference types, `T` must be non-nullable
| *\<baseClassName>?* | `T` must inherit (directly or indirectly) from that *base class* (nullable or not)
| *\<interfaceName>* | `T` must implement that *interface*.  If using nullable reference types, `T` must be non-nullable
| *\<interfaceName>*? | `T` must implement that *interface* (nullable or not).
| `where T : U` | If a generic class or method has two (or more) type parameters, you can specify that one must inherit from another

There are also `unmanaged` and `default` constraints used in unusual circumstances.

If you want to constrain a generic parameter to an enumerated type (an `enum`), use
a combination of `where T : struct, Enum`.  The capitalized `Enum` references 
`System.Enum`; the base class of all enumerated types.  `System.Enum` is an abstract
class (i.e., a reference type).  All actual `enum`s are value types.  By combining the
the constraints, you eliminate `System.Enum` from the set of possible `T`s.

If there is more than one generic parameter, each can be constrained separately:

```C#
class Base { }
class Test<T, U>
    where U : struct
    where T : Base, new()
{ }
```

The generic type name (which has been `T` or `U` in each example so far) has no meaning
at all.  If you look at the meta data for a generic type (like `Dictionary<TKey, TValue>`), 
the type names (`TKey` and `TValue`) are reduced to positional indicators.

Generic things (types or methods) differ depending on the number of parameters (e.g., you
can *overload* a class name by changing the number of generic parameters).  As an 
example, these are all different classes:

```C# 
public class SomeClass {}
public class SomeClass<T> {}
public class SomeClass<T, U>{}
```


### C# Generic Type Inference

The C# compiler is capable of inferring the generic parameter type of a function if the
function has a matching parameter for each generic parameter type.  Consider the
generic function we described above:

```C#
public T CompareAndReturn<T> (T first, T second) 
        where T : IComparable<T>, new() 
{/*code*/}
```

This is a perfectly good invocation of that function:

```C#
var result = CompareAndReturn(12, 15);
```

In this case, the compiler realizes that the two parameters are literal `int`s, so the 
function must be `CompareAndReturn<int>` (and, `int` implements `IComparable<int>` and
has a default constructor (as do all value type instances)).  The variable `result` will
be of type `int`, since `CompareAndReturn` returns a `T` and `T` is `int` in this case.

If a generic function has more than one type parameter, each of those type parameters 
must show up in the the function parameter list for type inference to work; partial 
type inference is impossible.

### .NET Generics are Fully Reflection Ready

Consider this code.  It's taken from a test project.  It's part of the process of
enumerating a set of JSON files, using the file name as both a *Table Name* and a 
*Type Name* and then using
the contained JSON as data for the table.  This method creates a `List<T>` where `T` 
is a POCO class that matches the database table.  It then populates and returns the 
strongly typed list to the caller.  It is used to populate a set of database tables 
in preparation for running some tests:

```C#
public static IEnumerable GetJsonDataAsListOfT(string typeName, string jsonData)
{
    // Get the .NET type corresponding to the string: typeName
    var theType = Type.GetType(typeName, true, true);

    // Get the generic type: List<T>
    var listOfTType = typeof(List<>);

    // Create/Reify the full type: List<[typeName]>
    var listOfTheTypeType = listOfTType.MakeGenericType(theType);

    //at this point, we have the type of List<T>
    //now get a methodInfo for the JsonConvert.Deserialize<T> function
    var jsonConvertType = typeof(JsonConvert);
    //there are multiple overloads of JsonConvert.Deserialize, get the generic one
    var jsonConvertMethods = jsonConvertType.GetMethods(BindingFlags.Public | BindingFlags.Static);
    var deserializeObjectMethod =
        jsonConvertMethods.FirstOrDefault(m => m.Name == "DeserializeObject" && m.IsGenericMethod);
    if (deserializeObjectMethod == null)
    {
        return null;
    }

    // create this function: static JsonConvert.DeserializeObject<[typeName]>()
    var deserializeObjectGenericMethod = 
                deserializeObjectMethod.MakeGenericMethod(listOfTheTypeType);

    //and invoke that function
    var result = deserializeObjectGenericMethod.Invoke(null, new object[] {jsonData});
    //at this point, result is an object, but we know it is a collection, so...
    return (IEnumerable) result;
}
```

Consider the case of the `Picnic` table.  If this function was called with two strings,
one `Picnic` (matching the `Picnic` type), and the other a string with JSON data that 
contained the contents of the `Picnic` table:

```JSON
[
  {
    "id": 1,
    "picnicName": "Picnic At Oakwood",
    "locationId": 1,
    "startTime": "2023-03-04T14:00:00",
    "hasMusic": true,
    "hasFood": true,
    "location": null,
    "teddyBears": []
  },
  {
    "id": 3,
    "picnicName": "100 Acre Festival",
    "locationId": 2,
    "startTime": "2023-06-21T14:30:00",
    "hasMusic": true,
    "hasFood": true,
    "location": null,
    "teddyBears": []
  }
]
```

Then 
```C#
var picnicList = GetJsonDataAsListOfT("Picnic", picnicJsonDataAsString);
```
would result in `picnicList` being a variable of type `IEnumerable`, where each item to
be enumerated would be a strongly typed `Picnic` instance.