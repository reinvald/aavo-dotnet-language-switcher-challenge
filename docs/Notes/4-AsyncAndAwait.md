## Tasks, Async and Await

C# includes a unified pattern for performing asynchronous operations, whether they are:
* Asynchronous I/O (also known as _overlapped_ I/O)
* Database operations
* Web service calls
* Work done on a background thread
* Any other work programmed to be done asynchronously

### C# Tasks

Tasks represent work to be done (or that is already completed).  Consider a task to be like 
the token you might get from a service shop when you bring your lawnmower in for repair. 
If it's early Spring, your plan for the next week or so might look like:

1. Bring lawnmower in for repair
2. When it's ready, mow the lawn.

Programmatically, those two steps could be represented by:

```C#
public async Task<LawnMower> RepairMowerAsync(LawnMower mower);
public async Task MowLawnAsync(LawnMower mower);
```

and you would invoke them within a function that looked like:

```C#
public async Task MowMyLawnTheFirstTimeAsync(LawnMower myMower) {
  var mower = await RepairMowerAsync(myMower);
  return await MowLawnAsync(mower);
}
```

Note that although the function is written as if it was a set of synchronous activities,
it is truly asynchronous.  It's possible for you to clip the hedge, trim the trees and
drink a well-earned beer while you were awaiting lawn mower repairs.

The important thing is to remember that Tasks represent a token for an activity that has
started and may or may not yet be completed.  When you `await` a Task, the current 
activity (`MowMyLawnTheFirstTimeAsync` in this example) is suspended until the task
you are awaiting signals its completion.

### Task States

Tasks may be in any of the following states:
* Created  
  The task has been initialized but has not yet been scheduled.
* WaitingToRun  
  The task has been scheduled for execution but has not yet begun executing.
* Running  
  The task is running but has not yet completed.
* RanToCompletion  
  The task completed execution successfully.
* Faulted  
  The task completed due to an unhandled exception.
* Canceled  
  The task started and then was cancelled (by way of a cancellation token)
* WaitingForActivation  
  The task is waiting to be activated and scheduled internally by the .NET infrastructure.
* WaitingForChildrenToComplete  
  The task has finished executing and is implicitly waiting for attached child tasks to complete.

In general, programs don't need to consider very many of these states; in fact, you very rarely
see explicit checking of a Task's status in C# code.  The only thing that's generally done
with Tasks is to `await` them, and use `try`/`catch` around them to handle any errors that
occur.

### An Aside - JavaScript and XMLHttpRequest

JavaScript has had a mechanism for doing asynchronous web operations since the year 2000: 
`XMLHttpRequest`.  A typical web operation using XMLHttpRequest looks like:

```JavaScript
let req = new XMLHttpRequest();
req.open("GET", new URL("http://domain.com/api/g"), true);
req.send();
req.onload = function() {
    alert("Loaded");
};
req.error = function() {
    alert("Error");
}
// more code
```
In that code, the request is sent to the web server when the `.send` method is called. 
If the request succeeds, then the function associated with the `onload` event will be run.
If the request has an error, then the `error` function will run.

If you were to put breakpoints on the `.send()` statement and the two `alert` calls, 
and on the line after the `//more code` comment and step through, you'd see the `send` 
happen, then the `onload` and `error` assignments and then the
`more code` code would continue.  At some point, when the operation was complete, the 
`alert("Loaded");` statement would execute.  Then whatever code was running at the time
would continue.

The function assigned to `.onload` is considered a _Continuation_.  It runs after the 
asynchronous operation completes.  The standard JavaScript asynchronous event loop handles
this (just like it would for an `onfocus` or `onblur` event).

More modern JavaScript includes the `fetch` statement that uses _Promises_.  The behavior
of the code is much clearer using this syntax, with the continuation(s) much easier to see.

```JavaScript
fetch("http://domain.com/api/g", {
    method: "GET"
}).then(
    () => alert("Loaded")
).then(
    () => alert("Complete")
).catch(
    () => alert("There was an error")
);
```
This can be read: _Do the fetch, and then, when it is complete, run the continuation (_Loaded_), and 
it completes, run the next continuation (_Complete_). If there is an error, catch it and run the 
`catch` code._

## An Async GET with C#'s Task

Writing something similar to the JavaScript code (above) in C# looks something like:

```C#
public async Task<string> GetSomeStringFromUrl(string url){
  try {
      HttpResponseMessage response = await _httpClient.GetAsync(url);
      string responseAsString = await response.ReadAsStringAsync();
      var final = VerifyResponse(responseAsString);
      return final;
  } catch (Exception ex) {
      ReportError (ex);
      return string.Empty;
  }
}
```

Deconstructing this code:

* The method is marked `async`  
  This informs the compiler that at least one call within the method is `await`ed, and that an 
  _async/await_ state machine must be created within the method to handle the tasks and their
  continuations.
* The method returns a `Task<string>`.  
  All `async` methods should return either a plain 
  `Task` or a generic `Task<T>` (it's also possible to declare a method `async void`, 
  [see below](#what-about-async-void) for a discussion of async void).  Only a
  `Task` (or a `Task<T>`) can be `await`ed.  A `Task` is like a _promise_
  to do some work (now or in the future), possibly returning a result of type `T`.
* Though an `async` method returns a `Task<T>`, after awaiting, the awaited
  expression is of type `T`.  So, the `response` variable in the code above is of 
  type `HttpResponseMessage`, and the `responseAsString` variable is a string.  If
  you await a (non-generic) `Task`, it's like you called a `void` method, nothing is returned.
* There are two awaited calls in that method (one to `HttpClient.GetAsync`, the other to
  `HttpResponseMessage.ReadAsStringAsync`).  The first returns a 
  `Task<HttpResponseMessage>`, the second a `Task<string>`.
* Though the function is declared to return a `Task<string>`, each path through the method
  only returns a `string`.  In general, when you are in a function declared to return a 
  `Task<T>`, your code only needs to return a `T`.  A `Task<T>` will be constructed from
  the returned `T` when the method returns
* Errors in the `async` code are caught in the standard fashion; by `catch`ing an exception.

### Step-by-Step: How that `async` Method is Executed

When the method starts, a call is made to `HttpClient.GetAsync`.  That method is marked `async`,
so it will return a `Task<HttpResponseMessage>` as soon as the first  `await`ed call is made 
within `GetAsync`.  That Task will represent a promise to finish the `GetAsync` call sometime.

When that task (the one returned by `GetAsync`) transitions to complete, then the outer function 
(`GetSomeStringFromUrl`) will be re-awakened and the _continuation_ associated with `GetAsync`
will be started started.  At that point, the assignment of the returned response will occur 
and then the async call to `ReadAsStringAsync` will happen.  

`ReadAsStringAsync` is also an awaited async call, 
so there will be another _continuation_ created
by the compiler for that call.  When the first `awaited` call happens within `ReadAsStringAsync`,
it will return to `GetSomeStringFromUrl`.  That function has already returned a `Task<string>` 
back to its caller (in `Running` status).  As a result, not much changes.

When the task associated with `ReadAsStringAsync` transitions to completed, its continuation will 
fire up.  At this point, there's nothing more that's awaited in the method, so execution continues
through the continuation, eventually _returning_ from the call - in this case, _returning_ means
marking the task that it had already returned as `Completed`.  This will satisfy the `await`ed 
call that was made into `GetSomeStringFromUrl`.

All this happens automatically.  When you read the text of the method, it appears **_as-if_** the
method was completely synchronous:

* A call is made to `GetAsync` and the code waits for it to complete
* A call is made to `ReadAsStringAsync` and the code waits for it to complete
* Finally, the rest of the method runs.

Semantically, that's the desired outcome.  However, under the covers, there's a complex dance of
Tasks, Task statuses and continuations that allows this to run completely asynchronously _without ever blocking_.

### Async - All the Way Down

Once you decide that you are going to make and `await` async call, you need to make sure that
all of the async calls in your stack are `await`able

> ---
> **A Fable**
> 
> ---
> A child once climbed the village hill asked the wise man in the cave: "Master, why is it 
> that when I stand atop this hill and look out over the horizon, the world seems to curve?"
> "My Child", he said, "that is because the the world is built atop of a large turtle's shell"
> The boy accepted the answer and went back to the village.
>
> The next day, the boy climbed back up and asked the wise man: "And what does the turtle
> stand upon?"  The wise man answered: "Another turtle, of course".  The boy looked
> quizzical and before he could ask the next question, the wise man interrupted with:
> "Son, [it's turtles all the way down](https://en.wikipedia.org/wiki/Turtles_all_the_way_down)"
> 
> ---

Async calls are like that turtle, you need to have all of your `async` calls, all the way down
your stack, `await`able.  Resist the (understandable) urge to make a blocking/synchronous
call from within an `async` method - for example, by accessing a Task's `Result` property 
(reading that property is essentially a blocking call). 
[You may end up in an unintentional deadlock.](https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html)

### What About `async void`?

Async methods should nearly always return a `Task` or a `Task<T>`.  However, if your 
app includes top-level event handlers, their call signature will likely look like:

```C#
void MyEventHandler (EventArgs eventArgs, object sender)
```
_(where `EventArgs` can be either the standard `System.EventArgs` or any of 
its subclasses)_

Event handlers can be made to call `async` methods by changing the signature to:

```C#
async void AsyncEventHandler (EventArgs eventArgs, object sender)
```

A handler declared that way (with the `async` keyword) is await-aware.  It 
can call async methods (using the standard `await` 
syntax).  However, the handler itself is a _fire and forget_ method; it cannot be
`await`ed.  A stack of turtles (`async Task` calls) can eventually be supported 
by an `async void`.

## Wait a Minute - What about Threads?

If you re-read this document, you will notice that the word _thread_ has only occurred
twice, in the introduction and just now.

None of the code in this document (including the lawn mover repair example) relies on 
more than a single thread running.  You can use Task, Async and Await with threads, but mostly
you don't need the overhead of extra threads.

The reason this works is that computers and operating systems are naturally 
asynchronous.  If code opens a file and reads in a megabyte of text, the operating
system will do that asynchronously, calling back into the program when the work is
complete (Windows calls this 
[Overlapped I/O](https://learn.microsoft.com/en-us/windows/win32/sync/synchronization-and-overlapped-input-and-output)).
Similarly, the OS will provide asynchronous mechanisms for making web service
calls or database accesses.  Each of these mechanisms uses the natural 
asynchrony of the operating system and 
[does not require any extra threads](https://blog.stephencleary.com/2013/11/there-is-no-thread.html).

The Task library does provide `Task.Run`.  It is `await`able and it runs the work
associated with the delegate you pass in as a parameter on a separate thread-pool 
thread, completing the Task when the work is complete.  Otherwise, just about everything
that can be done with Tasks requires no extra thread(s).