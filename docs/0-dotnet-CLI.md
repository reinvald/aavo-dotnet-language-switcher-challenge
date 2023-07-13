# Dotnet CLI

dotnet commands will run against a `.csproj` in the current directory. If you are at the solution level then you'll need to specify a project with the `--project` argument:

    dotnet run --project DotNetWebApi

These are the commands you'll need for this tech challenge, you can find the full list on Microsoft's [.NET CLI Overview](https://learn.microsoft.com/en-us/dotnet/core/tools/)

We'll also be using two `dotnet` CLI commands to scaffold both the project and the 
database access:

    dotnet new webapi ---command options
    dotnet ef dbcontext scaffold ---command options

## Build a project

    dotnet build
In VS Code, you can invoke a build with `<ctrl>+<shift>+B`

## Run project

    dotnet run
In VS Code, you can run the current project by pressing `F5` and stop a running project (started by 
VS Code) by pressing `<shift>+F5`

## Test

    dotnet test