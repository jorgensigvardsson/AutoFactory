namespace AutoFactory.Model
{
    public class FactoryMethodDeclaration
    {
        public FactoryMethodDeclaration(Constructor constructor)
        {
            Constructor = constructor;
        }

        public Constructor Constructor { get; }
    }
}