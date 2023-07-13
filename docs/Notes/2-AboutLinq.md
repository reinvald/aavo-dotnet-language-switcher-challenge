## LINQ: Language INtegrated Query technology

LINQ is a technology that came to the .NET development languages in the second half of the 2000s.
It relies on several innovations that came to C# (and other .NET languages) at the same time:

### C# 3.0 Fundamentals Needed by LINQ

* **The `var` keyword:**  
  In C#, you can use `var` to assign a variable the type of the right-hand side of an assignment statement.
  For example, in `var s = "SomeString";`, the variable `s` will be typed as string.  Using `var` here results
  in the same code being generated as would `string s = "SomeString";`

* **Anonymously Typed Objects:**  
  Fully-typed objects can be created without specifying a type for the object.  Consider: 
  ```C#
  var myObject = new { Name = "Bob the Builder", Age = 27 };
  ```
  That says that `myObject` is a reference to an instance of an anonymous class with two properties, one, 
  a string named `Name` and the other, an integer named `Age`.  All anonymously typed objects with those
  two properties _in the same Assembly_ are considered the same type.  Note that `var` is required here since
  the type name is anonymous.

* **Extension Methods**  
  Extension methods are a way to extend the call signature of an existing type without modifying the code of 
  the type.  Extension methods do not have access to the protected or private members of the type, but they can
  be very useful in extending the utility of the type.  Extension methods are static methods of another type
  where the first parameter is an instance of the type to be extended, annotated with a `this` keyword.
  Consider this example:
  ```C#
  namespace My.Extensions;

  public static partial class MyExtensions {
    public static string ToTitleCase(this string inputString){
        if (string.IsNullOrWhiteSpace(inputString) || inputString.Length < 2){
            return inputString;
        }
        return inputString.Substring(0,1).ToUpper() + inputString.Substring(1).ToLower();
    }
  }
  ```
  Now any string will have `.ToTitleCase()` appear as if it's one of its member functions.  Extension methods
  are particularly useful when they extend generic types, for example consider this trivial method:
  ```C#
  namespace My.Extensions;

  public static partial class MyExtensions {
    public static int MyCount(this IEnumerable<T> collection){
        int count = 0;
        foreach (var item in collection){
            ++count;
        }
        return count;
    }
  }
  ```
  Now any collection of anything (nearly all collections of type `T` implement `IEnumerable<T>`) can check its
  count.

* **Iterator Blocks and `yield return`**  
  
  The `yield return` statement predates LINQ by a release, but it is essential to basic LINQ functionality.
  A method that contains a `yield return` statement is considered an iterator block.  When an iterator block
  is compiled, it builds a state machine out of the block's code.  Consider this code:
  ```C#
  public static IEnumerable<int> Range (int start, int end)
  {
    if (start >= end)
    {
        throw new ArgumentOutOfRangeException($"Start: {start} must be less than End: {end}");
    }
    for (var i = start; i < end; ++i)
    {
        yield return i;
    }
  }
  ```
  It appears that `Range` will return a collection of `ints` from `start` to `end`.  For example, a call to:
  ```C#
  var range = Range(3, 6);
  ```
  would set `range` to something that would look like `"[ 3, 4, 5 ]"` in `JSON`.  However, that's not what
  `range` is.  Instead, it is an object capable of producing that range when it is iterated over (remember
  that about the only useful thing you can do with an `IEnumerable<T>` is iterate over it).  The actual 
  production of the collection is deferred until it is needed.

  It does this by making use of a compiler-generated state-machine within the iterator block. 
  Whenever `yield return i;` is executed, the value of `i` is returned to the caller as the 
  next item in the `IEnumerable<int>` collection.
  The next time `Range()` is called, execution starts immediately after the `yield return i;` statement

  To see this in action, paste the `Range()` code above into a debuggable environment.  Then use this code to 
  run it:

  ```C# 
  var range = Range(3, 6);
  foreach (var item in range){
    Debug.WriteLine($"Item is {item}");
  }
  ```
  Then put a breakpoint on the opening brace of the `for` loop in the `Range` function.  Run the code.
  When the breakpoint is hit, step through the code line by line.  When you get to `yield return i;`, 
  control will return to the calling `foreach` statement.  When the `foreach` loops around again,
  you can see control jump back into the `for` loop within `Range` until the next number is ready to return.

  The act of enumerating a deferred collection like this is sometimes called _materializing_
  the collection.

* **Lambdas**  
  The .NET languages have always supported `delegates` (effectively strongly-typed, type-safe function 
  pointers).  In C# version 2 (circa 2005), they began to support _anonymous methods_, albeit with an 
  awkward syntax.  Lambdas give anonymous methods a much easier to use, easier to remember syntax.  Consider
  the code for this extension method:
  ```C#
  namespace My.Extensions;

  public delegate bool Predicate<T> (T item);

  public static partial class MyExtensions {
    public static IEnumerable<T> Where<T> (this IEnumerable<T> collection, Predicate<T> predicate){
        foreach (var item in collection){
            if (predicate(item)){
                yield return item;
            }
        }
    }
  }
  ```

  You can call this extension method this way (using the `Range` method from the previous section):

  ```C#
  var numbers = Range(0,10); //creates a collection 0, 1, 2, ... 9
  var filtered = numbers.Where(i => i > 5);
  foreach (var item in filtered){
      Debug.WriteLine($"Filtered item is {item}");
  }
  ```
  In that code, the expression `i => i > 5` is a lambda expression, creating the method body to use as
  the `Predicate` delegate passed to the `Where` function.  It can be read as:  

  _given an integer `i`, return `true` if `i` is less than `5`_  

  When you run the code above, the result will involve only the integer sequence `[ 6, 7, 8, 9 ]` - with
  the `0..9` coming from `Range` and the `0..5` thrown away by the `Where` function.  If you step through
  this code, you will see that the `Range` and `Where` iterator blocks are visited on each pass through
  the outer `foreach (var item in filtered)` loop.

  [Lambda syntax](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions)
  provides for the ability for to have multiple input parameters and "method bodies" more complicated than
  what can fit on a single line.  However, the syntax shown above is the most common form.

### The Basis of LINQ

The code above showed a `Where` extension method.  We are going to change the call signature of Where 
slightly, by using the standard 
[`Func<>`](https://learn.microsoft.com/en-us/dotnet/api/system.func-2?view=net-7.0) delegate. `Func<>` is
a generic delegate definition that takes any number of input types (up to 16) and returns some type.
In the generic parameter list, the return type is always the last thing in the list.  As a result, instead
of using the `Predicate<T>` delegate defined above, `Where` will be changed to look like:

```C#
public static IEnumerable<T> Where<T> (this IEnumerable<T> collection, Func<T, bool> predicate) {...}
```
In the code above, `Func<T, bool> predicate` declares a parameter that is a delegate to a 
function that takes a T and returns a bool (just like the `Predicate` delegate declared above)

The change from `Predicate` to `Func<T, bool>` doesn't affect the `Where` function; the behavior of `Where` here will be the same as the previous example.

Now consider a `Select` extension method that looks like this:

```C#
public static IEnumerable<TResult> Select<TSource,TResult>(this IEnumerable<TSource> source, 
                                                           Func<TSource,TResult> selector)
{
    foreach (TSource element in source)
    {
        yield return selector(element);
    }
}
```
Again, this is an extension method on `IEnumerable<T>`.  However, instead of filtering out the incoming
collection, this performs a _projection_, transforming each incoming `TSource` element into a `TResult` 
and eventually returning a deferred collection of type `IEnumerable<TResult>`.  Consider:

```C#
var result = Range(0, 10)
             .Where(i => i > 5)
             .Select(i => new {Item = i, Doubled = i * 2});
```
This code will return a collection of four instances (one for each of 6 through 9) of an anonymous class
with two integer properties, one named `Item` and the other named `Doubled`.  Everything in the collection
is deferred, and when `result` is enumerated, all of `Range`, `Where` and `Select` will be 
visited in turn, for each of the integers that `Range` produces.

It's easy to imagine other extension methods like `Sum`, `Count`, `OrderBy`, `GroupBy` and others that perform
other database-like operations.

LINQ at its base is this set of extension methods, but it's much more as well.

### Language Integration

LINQ integrates with the C# and VB languages directly via it's _Query Comprehension Syntax_ (aka _Query
Syntax_).  The code above can be replaced with:

```C#
var result = from item in Range(0,10)
             where item > 5
             select new {Item = item, Doubled = item * 2};
```
Keywords for other key LINQ operations (like `sum`, `count`, `orderby`, and `groupby`) exist as well. As long
as the type (in this case `IEnumerable<int>`) has the appropriate member methods or extension methods, LINQ
is happy to do its magic.

The language integration in LINQ is based on pattern, not interfaces or other common 
mechanisms.  If a type (like `IEnumerable<T>)` implements an appropriate method like 
`Where` or `Select` (or has an appropriate extension method), then the statement above
_should just work_.

The C# compiler compiles query comprehension code into fluent-style method calls under the covers.  There is
no difference in how the two syntaxes are executed.  Whether to use one or the other is a matter of preference,
though, for example, `Join` operations are easier to express in the query syntax.

### Materializing Results

Up until now, there has been emphasis on the fact that LINQ calls result in deferred-evaluation instances of 
`IEnumerable<T>`.  We have been forcing the collections to be realized buy using a `foreach` 
statement.  There are simple, standard ways to _materialize_ deferred evaluation collections. 
Consider this extension method
```C#
public static List<T> ToList(this IEnumerable<T> source){
    var list = new List<T>();
    foreach (var item in list){
        list.Add(item);
    }
    return list;
}
```

Now our code can look like:

```C#
List<int> result = Range(0, 10)
                   .Where(i => i > 5)
                   .Select(i => new {Item = i, Doubled = i * 2})
                   .ToList();
```
And the `result` variable is no longer a deferred `IEnumerable<int>`; instead, it's a full-on materialized
`List<int>`.

LINQ includes a `ToList` extension method much like the one above. There is also a `ToArray()` 
function as well as their awaitable `async` counterparts (`ToListAsync()` and 
`ToArrayAsync()`).

## Moving to Database Access - `IQueryable<T>`

Everything that's been done so far in this document uses an `IEnumerable<T>` collection to access data in 
memory. In addition, all the lambdas shown are used to initialize `Func<>` style delegates.  As touched on
above, LINQ is based on patterns and structure, not on a particular set of interfaces or other static 
mechanisms.

There's another type that works well with LINQ: `IQueryable<T>`.  Implementations of IQueryable are done by
_LINQ Providers_.  One such provider is _Entity Framework_.  There are other LINQ providers as well - in
particular, [_LINQ to SQL_](https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/sql/linq/), a 
pre-cursor to Entity Framework that shipped with the original LINQ release.

### Expression Trees and `IQueryable`

LINQ based on IQueryables uses 
[_Expression Trees_](https://learn.microsoft.com/en-US/dotnet/csharp/programming-guide/concepts/expression-trees/)
rather than Delegates in its LINQ implementation.  Expression trees are sometimes just called _Expressions_,
though that's a somewhat ambiguous term.

Delegates are type-safe function pointers - they represent code that can be executed.  For example, this
makes sense:

```C#
Func<int, bool> integerPredicate = i => i > 5;
var shouldBeTrue = integerPredicate (12);
```
An expression tree is not code, it's an object that describes code.  Consider

```C#
Expression<Func<int, bool>> predicateExpression = i => i > 5;
```

That Expression Tree (`predicateExpression`) can't be invoked the way a delegate can.  Instead, it
is an object that represents the code _given an integer, return true if it's greater than 5, and
false otherwise_.

When you write a complex LINQ statement (using either syntax) that works with IQueryables, the compiler
produces a complex Expression Tree that represents what that statement means as code.  The LINQ 
provider's role in this is that when the LINQ statement (or statements) are _materialized_ (with, say,
a call to `await ToListAsync()`), then expression tree is realized in the target language for the 
provider, send to an appropriate data store, executed, returned to the program and converted to 
the desired form (say, a `List<T>`).

An example of this can be found early in this exercise.  This code:

```C#
var picnics = await _context.Picnics.ToListAsync();
```
is executed.  The `_context.Picnics` is a `DbSet<Picnics>` property of an Entity Framework 
`DbContext`.  It represents the entire contents of the `Picnic` table in the database.  Between
when the call is first made and when the `Task<List<Picnic>>` that this call returns completes 
(i.e., when the `await` finishes), the following happens:

1. The `DbContext` (`_context`) looks at the `IQueryable` this statement represents and translates
   it into SQL Server script that look like:
   ```sql
   SELECT 
      [p].[Id], 
      [p].[HasFood], 
      [p].[HasMusic], 
      [p].[LocationId], 
      [p].[PicnicName], 
      [p].[StartTime]
   FROM [Picnic] AS [p]
   ```
2. That SQL script is passed to the database server, evaluated, and returned back to the DbContext 
   as a SQL result set, which is then converted to a `List<Picnic>`

That's what _materializing_ an Entity Framework LINQ query does.  Like an `IEnumerable` query, 
everything is deferred until the query is converted to a result (typically with `ToList`, `ToArray`
or their async cousins).  With either `IEnumerable` or `IQueryable` queries, you can compose 
your query from a set of `IEnumerable` or `IQueryable` statements (for example, creating two
`IQueryable` and then _Joining_ them in a separate statement).  It's only when you materialize
the result that the round trip to the database server happens.

### `SaveChanges` and `SaveChangesAsync`

An Entity Framework `IQueryable` can be set to either _track changes_ or not.  By default, they
are set to track changes.

When an Entity Framework application accesses data from a database server, changes the values of
that data, and then calls 
[`SaveChanges`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext.savechanges) 
(or `SaveChangesAsync`) on the `DbContext`, the change
tracking mechanism gathers all of the changes, translates them into appropriate `INSERT`,  
`UPDATE` or `DELETE` statements and sends them off to the database for execution.

Calling `SaveChanges` (or its async friend) is the database-write equivalent of materializing
an EntityFramework query.
