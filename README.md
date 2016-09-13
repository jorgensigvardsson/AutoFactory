# AutoFactory
A Roslyn code generation tool that enables automatic factory classes.

## Rationale

When writing testable code, it is important to use factories, so that it is possible to fake/mock object instances created inside the system under test. A common pattern is to do this:

    public interface ISomeClass
    {
        ...
    }

    public class SomeClass : ISomeClass
    {
        ...
    }

    public interface ISomeClassFactory
    {
        ISomeClass Create(...);
    } 

    public class SomeClassFactory : ISomeClassFactory
    {
        public ISomeClass Create(...)
        {
            return new SomeClass(...);
        }
    }

The idea is that one should not call the `new` operator, but rather call ISomeClassFactory.Create(). This makes it possible to intercept the creation of the object, and swap it with a fake/mock.

# Usage
## Remarks

Simply mark your classes with the `[AutoFactory]` attribute, and set the _Custom Tool_ for each C# file in which the class(es) reside to `MSBuild:GenerateCodeFromAttributes`. See File Properties in Visual Studio, or add the child element `<Generator>MSBuild:GenerateCodeFromAttributes</Generator>` to the corresponding `<Compile>` element in the project file.

Make sure that the class has an interface defined. This will tell the Roslyn code generator that a factory should be generated for this class. If the subject class is named `Class`, the interface must be named `IClass`. It does not need to reside in the same namespace as the class. This interface is in theory not required at all, but it kind of makes the factory pattern pretty useless.

AutoFactory will also generate the interface `IClassFactory` and its implementation `ClassFactory` in the _same_ namespace as `Class`. If those names already exist, the compiler will be upset. Each public constructor of `Class` will yield a corresponding `Create()` method on the factory.

For each constructor, you must make a choice about each constructor parameter, whether they may be cached in the factory object itself, or if they must be passed via the factory constructor method. If you require the parameter to be passed in the factory constructor method, tag it with the `[PerInstance]` attribute. All other parameters will be added to the constructor of the factory, and cached in the factory.

This is very useful when using factories with dependency injection containers. For instance, one could have global/ambient services passed into the constructor of the factory (of course, the factory is also registered in the DI container). The parameters that are strictly unique per operation are then attributed as `[PerInstance]`, allowing the client of the object only to be concerned with the relevant dependencies.
    
## Example
This is an example of how the class may be used. Given the types:

    public interface IService
    {
        string StringValue { get; }
        int X { get; }
        int Y { get; }
        ISubService SubService { get; }
    }

    public interface ISubService
    {
        ...
    }

    [AutoFactory]
    public class Service : IService
    {
        public Service(ISubService service, [PerInstance] int x, [PerInstance] int y, string stringValue)
        {
            SubService = service;
            X = x;
            Y = y;
            StringValue = stringValue;
        }

        public string StringValue { get; }
        public int X { get; }
        public int Y { get; }
        public ISubService SubService { get; }
    }

You can write code such as this:

    [Test]
    public void TestMethod()
    {
        var subservice = new SubService();
        IServiceFactory serviceFactory = new ServiceFactory(subservice, "test");
        var obj = serviceFactory.Create(1, 2);

        Assert.AreSame(subservice, obj.Subservice);
        Assert.AreEqual("test", obj.ServerName);
        Assert.AreEqual(1, obj.X);
        Assert.AreEqual(2, obj.Y);
    }

# Dependencies
This project depends on CodeGeneration.Roslyn and CodeGeneration.Roslyn.BuildTime.

# Caveats
Roslyn will pick up the Custom Tool attribute on the file, and the `[AutoFactory]` attribute when the file containing the class is saved, or during a build. When adding the `[AutoFactory]` for the first time, the Intellisense feature in Visual Studio often fails to update. It will not understand any of the generated factory constructs. In situations like these, you need to restart Visual Studio.