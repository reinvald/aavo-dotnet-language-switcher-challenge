## Packaging: Assemblies, Namespaces, Classes & Partial

### Assemblies

Executable .NET code is packaged in _Assemblies_.  An assembly consists of one
or more _Modules_ (in reality, nearly every assembly consists of exactly one
module).  Modules are EXEs or DLLs based on the Windows Portable Executable (PE)
file format.

The executable code in a .NET Assembly/Module is an assembly language for a 
stack-based abstract machine.  The language is known as _Intermediate Language_
(IL) or sometimes _Common Intermediate Language_ (CIL, or even MSIL in some
contexts).  At execution time, a _Just In Time_ compiler (a JIT) will compile 
the IL into code that is executable on the target machine.  This is similar to
how most modern JVMs treat Java Byte Code.

PE files include information about their target architecture (for example x86).
One of the options with .NET applications is an _Any CPU_ target architecture.
In the Any CPU case, no target-specific code is included in the assembly, and
the underlying framework makes sure that the JITted code will run on any
supported processor and OS combination.

Assemblies also include all of the _Metadata_ needed to describe the code the 
assembly contains.  Assembly metadata is used by programmer tooling (for things
like _Intellisense_), by the compiler, by packaging tools and at runtime.

## Namespaces

In .NET, namespaces are used to segregate type names.  As an example, all of the types used
in traditional input/output operations are in the `System.IO` namespace.

Namespaces have no relationship to how types are package in assemblies.  An
assembly may contain code _belonging to_ several namespaces.  Code for a single 
namespace may span multiple assemblies.

Traditionally, namespaces were declared using normal (`{}`) brace-enforced 
scoping.  For example:

```C#
namespace MyNamespace
{
	class SomeClass : SuperClass 
	{
		// code for SomeClass
	}

	class SomeOtherClass 
	{
	    // code for SomeOtherClass
	}
}
```

Modern C# has made it easier to use `namespace` declarations for namespaces
whose scope spans an entire file (which is the normal case).  Using a modern
compiler, the code above can be condensed to:

```C#
namespace MyNamespace

class SomeClass : SuperClass 
{
	// code for SomeClass
}

class SomeOtherClass 
{
    // code for SomeOtherClass
}
```

### Using Directives

The `using` keyword can be used in a using directive.  It _brings in_ the names
in a namespace into the current programming environment.  For example, there's 
a static method named `Combine` in the `Path` class in the `System.IO` namespace.
It can be accessed by:

```C#
    System.IO.Path.Combine(parameters)
```

or, you can add a `using` directive at the top of the file:

```C#
using System.IO;
```

And call the method simply using `Path.Combine(parameters)`

You can also use using directives to provide an _alias_ to a fully qualified type
name.

Finally, there is another, completely unrelated use of the keyword `using`. _Using
statements_ can be used with _Disposable_ types to provide a scope that provides 
exception-proof deterministic finalization of a disposable object.

## Packaging and Types (Classes, Structs, ...) and "Partial"

A single C# source file can declare one or more classes (or other types like 
interfaces, structs, delegates).

C# also does not require that a single type (class, struct or interface) need be 
defined in a single source file.  The same class may be _partially_ defined 
in any number of source files using the `partial` keyword.  Because of the 
nature of type identity, all of the parts of a class must be defined in the 
same namespace and the same assembly.

For example, these two source file snippets could be in different source files:

```C#
public partial class MyClass() {
    public MyClass(){
	    //constructor code
	}

	public void DoSomework(){
	    //code for the DoSomework method
	}
}
```

and

```C#
public partial class MyClass() {
	public void DoSomeOtherWork(){
	    //code for the DoSomeOtherWork method
	}
}
```

after compilation, this would be completely equivalent to:

```C#
public class MyClass() {
    public MyClass(){
	    //constructor code
	}

	public void DoSomework(){
	    //code for the DoSomework method
	}

	public void DoSomeOtherWork(){
	    //code for the DoSomeOtherWork method
	}
}
```

### Where you see "partial classes"

The primary use of the `partial` keyword with classes is when code is created
by a _scaffolding_ tool or a _designer_.  Often, the tool expects that
it is the only source of changes to the file it created.  In this case, using
`partial` with a class allows the tool to have complete control over that code.
However, with `partial` part of the class can be defined in a second file that
the programmer can control.

There are lots of rules about what can be done with classes marked _partial_; but
they all boil down to: "in the end, you can only do what makes sense when the
class fragments are combined into a single class".

Note that everyone calls classes programmed this way _Partial Classes_, but there
really is no such thing as a _partial class_, just a class that is described
using multiple source files, enabled by the `partial` keyword.

## Partial Methods

The `partial` keyword can be used with methods as well to create _partial methods_.
Partial methods are not commonly used, but they can be very practical.

Consider a case where a designer scaffolds a complex operation for a class.
Programmers may want to hook the start and/or the end of that complex operation.
In this case, the scaffolded method might look like:

```C#
public void SomeComplexOperation(){
    // set up variables
	StartingComplexOperation (operationData);
	// code for the complex operation
	ComplexOperationComplete (operationData):
}
```

Those two methods could be declared as partial methods, with no body 
(/implementation):

```C#
private partial void StartingComplexOperation (OperationData operationData);
private partial void ComplexOperationComplete (OperationData operationData):
```

If the methods are not implemented, then the call sites that reference them 
end up being `noops` (i.e., they disappear at compile time)

However, if there is an implementation of those methods in the other side
of the partial class (the code under the programmer's control), then they
will be called.