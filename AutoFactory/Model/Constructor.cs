namespace AutoFactory.Model
{
    public class Constructor
    {
        public Constructor(ConstructorParameters constructorParameters)
        {
            ConstructorParameters = constructorParameters;
        }

        public ConstructorParameters ConstructorParameters { get; }
    }
}