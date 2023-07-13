# Unit Testing

## Solutions and Projects

In the Microsoft world of developer and build tools, solutions and projects play a key roll.
A *project* represent a buildable unit of functionality.  *Solutions* are basically bags full 
of projects and can also be built (building a solution builds all the projects in the solution).
A project may be part of multiple solutions.

Solutions are defined by _Solution Files_ (`*.sln`).  Projects are defined by project files.
Depending on the language or technology, project files have different file extensions (for 
example, C# project files are `*.csproj`, while VB project files are `*.vbproj`). The projects
in a solution may be of different languages or technology.  Within a project, there generally is
only one language or technology.

Through the first decades of the .NET Framework, project files were detailed; a file in a folder
wasn't part of the project unless it was listed in the project file.  Recent releases (".NET Core" and ".NET 5+") use leaner project file assumptions; now all files in the domain of a 
project are part of the project unless explicitly excluded. Convention now trumps configuration.

Visual Studio, Visual Studio Code and MSBuild (the tool that manages CI builds and build 
servers) are all based on the same solution and project conventions.

### Moving to a Solution

Up until now, we have been working with a single project (the `DotNetWebApi.csproj` file
in the `DotNetWebApi` folder).  The Unit Tests will be managed in a separate project, so
it makes sense to join them together in a solution.  We will create the solution file in the
parent folder of the folder we have been working in.

At the moment, our folder structure looks like:

```
├── DotNetWebApi
│   ├── DotNetWebApi
│   │   ├── DotNetWebApi.csproj
|   |   ├── [All other files]
|   |   ├── [All other subfolders (like Models, Services, etc.)]
```
We will create the solution file in that parent `DotNetWebApi` folder.

1. In the parent `DotNetWebApi` folder, open a console window and create the solution file
   ``` shell
   dotnet new sln --name DotNetWebApi
   ```
2. From the same console window, in the same folder, add the project we have been working on
   (`DotNetWebApi.csproj`) to that solution.  Either this on Windows:
   ```shell
   dotnet sln add DotNetWebApi\DotNetWebApi.csproj
   ```
   Or this on Macs:
   ```shell
   dotnet sln add DotNetWebApi/DotNetWebApi.csproj
   ```
When we finish (including the steps below), the directory structure will look like:

```
├── DotNetWebApi
│   ├── DotNetWebApi.sln (the solution file)
│   ├── DotNetWebApi
│   │   ├── DotNetWebApi.csproj
|   |   ├── [Other main project files]
|   |   ├── [Main project subfolders (like Models, Services, etc.)]
│   ├── DotNetWebApi.Tests
│   │   ├── DotNetWebApi.Tests.csproj
|   |   ├── [Other test project files]
|   |   ├── [Other test project subfolders (Controllers)]
```

If you are curious, open the solution file in a plain text editor like Notepad (don't make
any changes).  There's not a lot of readable information in there, but you can see the the 
reference to the `DotNetWebApi.csproj` project.

## Create the Test Project

It's important that test projects are different from normal projects - if for no other reason
than that they are not normally deployed to servers.  In .NET, test projects are normally named 
after the project under test, but with a `.Tests` suffix.

1. From the top level (solution) folder, run this dotnet command to create the new Tests 
   project:
    ```shell
    dotnet new xunit -o DotNetWebApi.Tests
    ```
2. Add this project to the newly created solution.  From the same console (in the same folder),
   run:
   ```shell
   dotnet sln add ./DotNetWebApi.Tests/DotNetWebApi.Tests.csproj
   ```
3. Our test project will be calling into the main project, so it needs to have a 
   `Project Reference` to that project (i.e., a reference to another project in the same
   solution).  Run this from the same location:
   ```shell
   dotnet add ./DotNetWebApi.Tests/DotNetWebApi.Tests.csproj reference ./DotNetWebApi/DotNetWebApi.csproj
   ```
4. Open a new VS Code window, and within that open the newly created `DotNetWebApi.Tests`
   folder.  When it asks if you want the "Required assets to build and debug", make sure to
   say "Yes".

> ---
> **Note**: `xUnit` vs `nUnit` vs `MSTest` - And, `[Fact]` versus `[Theory]`
>
> ---
> There are three well-known, primary unit testing frameworks available for .NET projects, 
> `xUnit`, `nUnit`, and `MSTest`.  MSTest is provided by Microsoft.  It works well in 
> pure Microsoft environments, but is usually considered the least flexible. xUnit was 
> developed as a successor to nUnit, but development on nUnit has continued on in parallel 
> to xUnit development.  We will be using `xUnit` here.  All three are equally available 
> within Visual Studio and VS Code as test tools.
>
> xUnit identifies tests using one of two attributes, either `[Fact]` or `[Theory]`.
> A [Fact] tests an invariant, and most typical tests are [Fact]s.  A [Theory] is a test
> that depends on input data.
>
> ---

## Basic Unit Tests

To start lets create a simple function that we can unit test with. Add this function to the `ExternalApiService.cs` file

```csharp
public bool IsEven(int value){
    return (value % 2) == 0;
}
```

Then in our new tests project create a new directory `Services`, and a new test file inside `ExternalApiServiceTest.cs` with the following:

```csharp
using Xunit;
using DotNetWebApi.Services;

namespace DotNetWebApi.Tests;

public class ExternalApiServiceTest
{
    [Fact]
    public void TestTwoIsEven()
    {
        // instantiating a ExternalApiService requires an injected HttpClient, 
        // so, just provide one
        var externalApiService = new ExternalApiService(new HttpClient());
        Assert.True(externalApiService.IsEven(2));
    }
}
```

This is a simple unit test that allows us to assert that the response will be true. Next we 
simply need to run the dotnet test command (in the Test project folder). This will run any 
test in the test project.

```
dotnet test
```
> ---
> **Note**: About Building Multiple Projects
>
> ---
> Up until now, we have had a single project.  VS Code is smart enough that when you tell
> it to run/debug a project, it will save all of the project's files, compile the project
> run the program and attach its debugger.
>
> When you have more than one project (like we have here), VS Code isn't quite as smart.
> When you make a change in one project, it may not realize that that it needs to build
> the project's dependencies as well.  It's a good idea to type:
> ```
> dotnet build
> ```
> in the folder that contains the solution (`*.sln`) before you run `dotnet test`. Visual
> Studio is _very_ solution aware (you almost always work within a solution in Visual
> Studio).  When you build, run or run tests, it will always build all relevant projects.
>
> ---

If you would like to see what happens when a test fails, simply change the parameter to a non-even number

There are many options for test cases, ie parameterized tests like this one:

```csharp
[Theory]
[InlineData(0)]
[InlineData(2)]
public void TestMultipleIsEven(int value)
{
    var externalApiService = new ExternalApiService(new HttpClient());
    Assert.True(externalApiService.IsEven(value));
}
```

## Using the `Moq` Framework

.NET has a couple of Mocking frameworks we can use, for this tutorial we'll use the `Moq` framework. This will let us test a specific function without instantiating the full program.

We need to add the `Moq` package to the project, as well as a few others.  In a console window, in the Test project folder, run the following set of commands:

```shell
dotnet add package moq
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.AspNetCore.OpenApi
```

Create a new `Controllers` directory in the Test project with the file 
`TeddyBearControllerTest.cs` and the following code

```csharp
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using DotNetWebApi.Models;
using DotNetWebApi.Controllers;

namespace DotNetWebApi.Tests;

public class TeddyBearControllerTest
{

    [Fact]
    public async void CreateTeddyBearShouldCallDBContextCorrectly()
    {
        var teddyBearContext = new Mock<TeddyBearsContext>(new DbContextOptionsBuilder<TeddyBearsContext>().Options);

        var mockTeddyBearDbSet = new Mock<DbSet<TeddyBear>>();
        teddyBearContext.Setup(m => m.TeddyBears).Returns(mockTeddyBearDbSet.Object);

        var controller = new TeddyBearController(teddyBearContext.Object);

        var newTeddyBear = new TeddyBear();
        await controller.CreateTeddyBear(newTeddyBear);

        teddyBearContext.Verify(m => m.TeddyBears, Times.Once());
        teddyBearContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        teddyBearContext.Verify(m => m.TeddyBears.Add(newTeddyBear), Times.Once());
    }
}
```

We can see the syntax for creating a Mock object is like so:

```csharp
var mockTeddyBearDbSet = new Mock<DbSet<TeddyBear>>();
```

And to validate interactions on the mock we can use code like:
```csharp
teddyBearContext.Verify(m => m.TeddyBears, Times.Once());
```

For more information on the Moq framework read Moq's repo readme [Moq4](https://github.com/moq/moq4)

## Next Steps

This was the last step in the tutorial!! Return to the Readme and complete the steps in the [Grading Criterial](/README.md#grading-criteria) to complete this challenge.