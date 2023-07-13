## The `IDisposable` Interface and the `using` Keyword

The .NET `IDisposable` interface provides a deterministic finalization mechanism in a
non-deterministic Garbage Collected system.  The `using` keyword makes using `IDisposable`
easier.

### Background: Garbage Collection and Finalization

In a .NET program, instances of [_Reference Types_](./0-TypeSystem.md) are created on 
the _Managed Heap_.  If there are no other active references to an object on the managed
heap, it becomes _eligible for garbage collection_.  Collection does not happen immediately,
instead, it is deferred until it's convenient for the system.  Most other garbage 
collected systems behave in the same way.

A .NET class can create a method known as a _Finalizer_ (also known as a _destructor_ in
C#) that can clean up resources used by the object as it is about to be garbage collected.
However, there are no guarantees as to when a Finalizer might run (i.e., it is
non-deterministic).  It may run immediately or it may wait until your program is exiting.

> ---
> **Note**:  _About Finalizers_
>
> ---
> Finalizers should be created in **_very rare_** circumstances.  They are difficult to
> write, very hard to test, and when they crash (which happens often), difficult to 
> debug.  Because they run just as an object was scheduled to be garbage collected, they
> defer the collection even further.  That deferral can gum up the GC and slow down a 
> program.
>
> If you do create a finalizer, make sure your `Dispose` method un-schedules the 
> finalization of your objects.  See the note on 
> [`GC.SuppressFinalization`](#preventing-finalization-from-a-dispose-method) below.
>
> Finalizers are also known as _destructors_ in the C# language.  The name was chosen 20
> years ago to make the concept easier for C++ programmers.  _Finalizers are not like
> C++ destructors at all._
>
> ---

### Deterministic Finalization - `IDisposable`

Garbage collection manages the memory used by reference type objects very well.  However,
there are non-memory resources that are not managed by the garbage collector.  These 
resources include things like file handles, network sockets and database connectors.

Many times, these resources are more constrained than memory and should be more
aggressively managed than memory is.  .NET (and C#) provide a way for a class to opt in
to more aggressive finalization and to advertise that it does: 

```C#
namespace System;
public interface IDisposable{
    public Dispose();
}
```

When a class implements the `IDisposable` interface, consumers should call `Dispose` when
they are finished with instances of that class.  It's up to the implementing class to 
decide what should be disposed.  It's up to the calling class to make sure that the 
`Dispose` method gets called.

> ---
> **Remember**: Finalization is built-in (but fragile); Dispose is code _you_ write 
> and _you_ call
>
> ---

### The `using` Keyword - Exception-proof Dispose

C# provides the `try`/`finally` construct to provide an exception-proof guarantee
that cleanup can happen.  Because calling `Dispose` is so common, and getting a
a guarantee in the face of exceptions is important, the language includes the `using` 
keyword.

This code:

```C#
using (var x = new DisposableClass()){
    x.DoSomethingInteresting();
}
```

is equivalent to:

```C#
{
    var x = new DisposableClass();
    try {
        x.DoSomethingInteresting();
    }
    finally {
        x.Dispose();
    }
}
```

Recent versions of C# have made so that this can be collapsed to the following
code (although the programmer has less control over scope in this situation):

```C#
using var x = new DisposableClass();
x.DoSomethingInteresting();
```

### Preventing Finalization from a Dispose Method

The .NET Framework garbage collector looks to see if an object requires finalization
before it starts a garbage collection cycle. There are two basic rules whether an 
object needs Finalization:

* Does the class have a Finalizer (aka a destructor)?
* Has the object run `GC.SuppressFinalization(this);`?

If the object's class has a Finalizer and the object hasn't suppressed finalization, 
the GC will temporarily prevent the object from being collected and schedule the object 
for finalization some time in the future (after the garbage collection it just missed).

Generally, once a class's `Dispose` method runs, there's no need for Finalization;
all the resources the finalizer might release should have been released in Dispose.
As a result, the Dispose method should call `GC.SuppressFinalization(this);` within
the Dispose method.

### What Should Be Disposed

There are two categories of things that should be cleaned up in a `Dispose` method:

* Native resources directly managed by instances of this type
* References to objects (in _fields_ or _properties_) of a Disposable type (i.e.,
  references to objects whose class implements `IDisposable`)

In the first case (native resources), your type is very likely a good candidate
for writing a Finalizer.  Be very careful how you do that.

In the second case, these objects do not need finalization (and, in fact, cannot
be safely referenced from within a Finalizer).  They do need to be disposed though.

### Microsoft's Dispose Pattern

Microsoft has published advice and basic models of what a Dispose method should look
like in different situations, naming it the 
[Dispose Pattern](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/dispose-pattern)

The pattern is based on the assumption that for most classes, the body of the `Dispose`
method is the same (or very close) to the body of the Finalizer; the only major
difference being the call to `GC.SuppressFinalization(this);` in the Dispose method.

The _Dispose Pattern_ works with the expectation of inheritance.  Reference the article
on the Dispose Pattern for details, but the rough design is:

* Create an overload of `Dispose` that is protected and takes a single `bool` parameter
  In base classes, mark it `virtual` and in subclasses, mark it `override`
* Your normal `Dispose` method (with no arguments) should _not_ be virtual and 
  should merely call the overload passing `true` as the parameter and then suppress
  finalization:

  ```C#
  public void Dispose() {
    Dispose(true);
    GC.SuppressFinalization(this);
  }
  ```
* If you have a Finalizer, you should also call the Dispose overload, but instead pass
  `false`:

  ```C#
  ~MyFinalizableClass() {
    Dispose(false);
  }
  ```
This way, all of the complex cleanup logic is in a single place; the overloaded 
`Dispose(bool)` method.  It typically ends up looking like:

```C#
protected virtual Dispose (bool disposing){
    if (disposing){
        // call Dispose on disposable properties
    }
    // cleanup any native resources this class manages
}
```

Remember to read the Microsoft article on the
[Dispose Pattern](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/dispose-pattern)


> ---
> **Note**: `Dispose` methods should be _idempotent_; callers should be able to call
> `Dispose` more than once with no side-effects. A `Dispose` implementation may want
> to track whether or not it's been called before to prevent an expensive (or unsafe)
> cleanup from running twice.
>
> The rare exception to this rule is a class whose instances are unusable after cleanup
>
> ---