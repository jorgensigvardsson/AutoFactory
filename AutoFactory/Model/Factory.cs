namespace AutoFactory.Model
{
    public class Factory
    {
        public Factory(ConstructedClass constructedClass, FactoryInterface @interface)
        {
            ConstructedClass = constructedClass;
            Interface = @interface;
        }

        public ConstructedClass ConstructedClass { get; }
        public FactoryInterface Interface { get; }
    }
}