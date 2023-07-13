## .NET Delegates

A `Delegate` is a type-safe function point in the .NET ecosystem. 

Although it is first class
type in languages like C#, each declared delegate is a sub-class of the System.Delegate class 
under the covers.  When you use a delegate, you are using an instance of the delegate class
that you (or someone else created).

Delegates bring the concept of 
[_First Class Functions_](https://en.wikipedia.org/wiki/First-class_function)
to C# and the other .NET languages.  You can pass around references to functions like
other objects within the language.

Consider this delegate:

```C#
public delegate double MathOperation(double firstParameter, double secondParameter);
```

It would match all of these methods:

```C#
public static double AddOp(double firstParameter, double secondParameter){/*code*/}
public static double SubOp(double firstParameter, double secondParameter){/*code*/}
public static double MultOp(double firstParameter, double secondParameter){/*code*/}
public static double DivOp(double firstParameter, double secondParameter){/*code*/}
```

So you could do something like:

```C#
public static void TestDelegates()
{
    var mathOps = new List<MathOperation> { AddOp, SubOp, MultOp, DivOp };
    foreach (var mathOp in mathOps)
    {
        Console.WriteLine($"{mathOp.Method}: {mathOp(2.0, 3.0)}");
    }
}
```

The output from this would look like:
```
Double AddOp(Double, Double): 5
Double SubOp(Double, Double): -1
Double MultOp(Double, Double): 6
Double DivOp(Double, Double): 0.6666666666666666
```

### Invoking a Delegate

All delegate types get an `Invoke` method whose signature matches the delegate signature.
In C#, you can also invoke a delegate by name, just like a function call (as was done in 
the example above).  In that example, the `Console.WriteLine` statement could be changed 
to the following completely equivalent code

```C#
Console.WriteLine($"{mathOp.Method}: {mathOp.Invoke(2.0, 3.0)}");
```

### The `Func` and `Action` Generic Delegates

In addition to the delegates your code may create, .NET provides two general-purpose
generic delegates: [`Func`](https://learn.microsoft.com/en-us/dotnet/api/system.func-1) 
and [`Action`](https://learn.microsoft.com/en-us/dotnet/api/system.action).

They are each a set of overloaded generic delegate definitions that represent methods
with up to 16 parameters.  `Func` represents methods that return a value (the last 
type in the list of generic parameters represents the return type).  `Action` represents
`void` methods - also withu up to 16 parameters.

The `MathOperation` delegate above could have been removed, and the `mathOps` variable 
in the sample code could have been changed to:

```C#
var mathOps = new List<Func<double, double, double>> { AddOp, SubOp, MultOp, DivOp };
```
In that code, the first two `double` types in the list of generic type parameters represent
the argument types while the third one represents the return type.  It uses the
[`Func<T1,T2,TResult> Delegate`](https://learn.microsoft.com/en-us/dotnet/api/system.func-3),
defined as:

```C#
public delegate TResult Func<in T1,in T2,out TResult>(T1 arg1, T2 arg2);
```

> ---
> **Note**: Equivalent Delegate Types are Still Separate Types  
>
> ---
> In the code above, `MathOperation` and `Func<in T1,in T2,out TResult>` both represent
> delegates to methods that have two `double` parameters and that return a `double`.
> However, they are separate types.  If you have a method that takes a `MathOperation`
> as a parameter, you cannot pass it a delegate declared as a 
> `Func<double, double, double>`
>
> In general, this isn't a problem (particularly when you are using _Lambdas_).  However, 
> it sometimes pops up with the 
> [`System.Predicate`](https://learn.microsoft.com/en-us/dotnet/api/system.predicate-1)
> delegate:
>
> ```C#
> public delegate bool Predicate<in T>(T obj);
> ```
> Some system code requires a `Predicate` for filtering.  In LINQ, the `Where` extension
> method uses a `Func<T, bool>`
>
> ---

> ---
> **Question**: What Do the `in` and `out` Keyword Signify in the `Func` Definition?
>
> ---
> As noted above, a two-parameter `Func` definition looks like: 
> ```C#
> public delegate TResult Func<in T1,in T2,out TResult>(T1 arg1, T2 arg2);
> ```
> The `in` and `out` keywords describe 
> [_Covariance_ and _Contravariance_](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/covariance-contravariance/).
> The two `in` keywords signifies that the parameters are _contravariant_, while the `out` 
> keyword signifies that the return type is _covariant_.  A discussion of covariance and
> contravariance is beyond the realm of this article.  In general, it's not something you
> need to think about - _It Just Works_ most of the time
>
> ---

### Using a Delegate to Dispatch Work to the Threadpool and Other Async Operations

When work is dispatched to a thread (whether in the threadpool or not), it is always
described using a delegate, i.e., you give the thread (or threadpool) a delegate to a
function and say _"please do this in the background"_.

When you create a new `System.Threading.Thread` object, the constructor takes either
a `ThreadStart` delegate or a `ParameterizedThreadStart` delegate.  If you dispatch
work to the ThreadPool using 
[`QueueUserWorkItem`](https://learn.microsoft.com/en-us/dotnet/api/system.threading.threadpool.queueuserworkitem)
you pass a either a `WaitCallback` delegate or an `Action<T>` delegate.  If you do
the same with 
[`Task.Run`](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.run)
you can pass one of a variety of `Action` and `Func` delegates.

You can also dispatch work to the ThreadPool directly from a delegate instance.  The
mechanism uses ghe _BeginOperation_/_EndOperation_ pattern that was originally
used to implement asynchrony in .NET operations.  All delegates have `BeginInvoke`
and `EndInvoke` methods (their call signatures depend on the signature of the delegate).
Calling `BeginInvoke` on a delegate dispatches the work described by the delegate to the
thread pool while `EndInvoke` should be called when the work completes.  This was the
most common way of running background tasks in the early 2000s.  It's not used very
much anymore.

> ---
> **Question**: What is the ThreadPool?  
>
> ---
> .NET processes include a singleton instance of the 
> [`ThreadPool` class](https://learn.microsoft.com/en-us/dotnet/api/system.threading.threadpool)
>
> The thread pool manages a set of threads that are normally idle.  When you dispatch work
> to the thread pool, one of the threads wakes up, does the work, and when the work is 
> completed, the thread is returned to the pool to wait for more work.  The thread pool 
> manages the number of threads in the pool.  In general, the thread pool is just something
> that is there and that works.
>
> In a web application, each request is handed to a thread pool thread to get its business 
> done.
>
> One of the only issues that you need to think about when using the threadpool is that the 
> work dispatched to the thread pool should all required _similar_ work times.  If you are 
> dispatching work that completes _very_ quickly and work that completes _very_ slowly, 
> then the the slow work could cause thread starvation; though this extreme case rarely occurs.
>
> In general, do not dispatch long-lived work to the thread pool, consider creating a thread
> and doing the work on that dedicated thread.
>
> ---

### Anonymous Methods and Lambdas

C# has included anonymous methods since its initial release.  They were rarely used until
the advent of LINQ and lamdas.  Anonymous methods use the `delegate` keyword in their 
definition.  Here's an example of a traditional C# anonymous method (using the 
`MathOperation` delegate defined above):

```C#
MathOperation power = delegate (double theBase, double theExponent)
{
    return Math.Pow(theBase, theExponent);
};
Assert.Equal (16.0, power(2.0, 4.0));
```

A similar syntax is still common in VB.  It also looks a lot like JavaScript
anonymous functions.

_Lambdas_ provide a much easier and clearer path to an anonymous method. Here's the
same code using Lambda syntax:

```C#
MathOperation power = (theBase, theExponent) => Math.Pow(theBase, theExponent);
```

Lambda syntax is concise, and makes use of type inference:

* **The Parameters**:  
  Parameters are required.  If there are no parameters then an empty list of parameters
  in the form of an pair of parentheses: `()` must be provided.  If the types of the 
  parameters can be inferred (typically from the type of the delegate), then no 
  parameter type information need be included.  If there is only a single parameter,
  then the parentheses are optional, for example:
  ```C#
  Func<double, double> squareOf = x => x * x;
  ```
* **The Lambda Operator**:  
  The [_Lambda Operator_](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-operator) 
  (`=>`) is what makes a lambda expression a lambda expression
* **The Body**:  
  The body of a lambda can take two forms. The simple form (like what's shown in the two
  examples above) is just a single line.  If the expression can be evaluated in a single line
  of code (like `power` or `squared`), then you just need that expression (whether the 
  lambda represents a `Func` (that returns something) or an `Action` (that is void)). If
  more than one line is needed, the body of a lambda can include a block of code within
  normal `{`squiggly brackets`}`, with a `return;` statement.

The _Lambda Operator_ can also be used in other contexts:

* *Property Setters and Getters*:  
  If you have a property that is more complex than can be handled with an 
  [_auto-property_](./3-Properties.md#auto-properties). but that is still simple enough
  to be expressed in a single line of code, you can use the lambda operator to define it.
* *Methods*:  
  Similarly, if you have a simple method, you can define it with a lambda operator.
* *LINQ Expressions*:  
  [LINQ Expressions](./2-AboutLinq.md#expression-trees-and-iqueryable) are mostly
  indistinguishable from delegate lambda expressions.  The difference depends on the
  context in which they are used.  LINQ expressions must use the lambda syntax 
  (not the anonymous method syntax).

### Multicast Delegates

You can create a 
[Multicast Delegate](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/how-to-combine-delegates-multicast-delegates)
by combining two or more delegates.  When you `Invoke` a multicast delegate, each of the
delegates is Invoked.

Only `void` delegates (i.e. `Actions`) should be combined into a multicast delegate (if
you are invoking three functions, and they each return a separate value, what should
the multicast version return?).  All delegates in a multicast delegate must be of the 
same type.

For example, in the following `combined` is a multicast delegate.  Invoking it will
invoke each of `add` and `subtract` with the same arguments:
```C#
public static void Test2()
{
    var add = new Action<double, double>((x, y) => Console.WriteLine(x + y));
    var subtract = new Action<double, double>((x, y) => Console.WriteLine(x - y));
    var combined = add + subtract;

    add(1.0, 2.0);
    subtract(42.0, 2.0);
    combined(6.5, 1.5);
}
```
When this is run, the output looks like:
```
3
40
8
5
```
In general, it is unlikely you will ever use Multicast Delegates directly.  However,
they are the basis of .NET `event`s, as described in the next section;

## Events and Event Handling

.NET [_Events_](https://learn.microsoft.com/en-us/dotnet/csharp/events-overview) are 
a simple, light-weight _publish and subscribe_ mechanism (i.e., they
implement the _Observer_ pattern).  They are used throughout the framework code, but
particularly in client-side UI code.

* Event sources _publish_ events by including event declarations in their public
  interface.  The event declaration includes a `delegate` type that describes the
  signature needed by the event sink handler
* Event sources may have zero, one or many subscribers
* Event sinks subscribe to events by adding a delegate to an event handler to the 
  multicast delegate maintained by the event. (They unsubscribe by removing that 
  delegate).  Subscription is done using an implementation of the 
  [`incrementing assignment` operator](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/arithmetic-operators#operator-precedence-and-associativity)
  (`+=`).  Unsubscribes use (`-=`)
* Event sources _raise_ events by invoking their associated multicast delegate, 
  in effect, invoking each of the subscribed event handlers.
* Event handler signatures (described by handler delegates) are nearly identical.
  The first parameter is always an `object` named `sender` (which is a reference
  to the event source).  The second parameter is an object of type `EventArgs` or
  a subclass of `EventArgs`.  The properties of the subclasses are used to pass
  any required arguments from the event source to the individual event handlers.
* The result of this design is that there is almost no coupling between event sources
  and event handlers; they need only agree on the type of `EventArgs`.

_**On the Publisher/Event Source Side**_  

Consider _Windows Forms_.  All Windows Forms controls implement a `Click` event:

```C#
public event EventHandler? Click;
```
Where `EventHandler` is a delegate defined as:
```C#
public delegate void EventHandler(object? sender, EventArgs e);
```
There is no useful information about a Click event other than the fact that the
corresponding control was clicked.  As a result, the event handler signature uses
the base class `EventArgs` as the second parameter.

Inside the button class, the code that raises the event is simple.  An event is
raised by _Invoking_ the event as if it were a delegate:

```C#
public void MonitorTheButton(){
    if (WeSenseTheButtonWasClicked()){
        this.Click?.Invoke(this, new EventArgs());
    }
}
```
Note that the `Click` event is checked for `null` before it is invoked.  This is done
using the 
[_null conditional_ operator (`?.`)](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/member-access-operators#null-conditional-operators--and-) 
rather than directly calling `Invoke`.  It will be null if there are no subscribers.

_**On the Subscriber/Event Handler Side**_ 

If an application wants to subscribe to a click event on a particular button on the
from, it adds an event handler subscription at startup time:

```C#
button1.Click += button1_Click;
```

What this is saying is that when a user clicks on Button1, the code for that button 
class will raise the button's Click event.  Because the form's code (which is the 
application) subscribed to that event with the `button1_Click` method, that method 
will be called the click happens.


Inside the application, the form's code for handling the button click looks like:

```C#
private void button1_Click(object sender, EventArgs e){
    DoWhateverNeedsToBeDoneOnAClick();
}
```

If information from the event source needs to be passed to the event handler, a 
subclass of `EventArgs` is used.  Consider a handler for the `MouseMove` event
on the form.  The MouseMove event is declared this way:

```C#
public event System.Windows.Forms.MouseEventHandler? MouseMove;
```
The MouseEventHandler delegate is defined as:
```C#
public delegate void MouseEventHandler(object? sender, MouseEventArgs e);
```
And the MouseEventArgs class is defined this way:

```C#
public class MouseEventArgs : EventArgs { /* code and properties */ }
```
It has two integer-valued public properties named `X` and `Y`

A possible handler for this event, in the application's code could look like:

```C#
private void Form1_MouseMove(object sender, MouseEventArgs e)
{
    Debug.WriteLine ($"X: {e.X} - Y: {e.Y}");
}
```
Note how the information passed from the event source is passed to the event 
handler(s) by way of properties on the EventArgs.  This pattern is repeated throughout
the framework.
