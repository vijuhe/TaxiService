namespace TaxiService
{
    public interface ILocation
    {
        bool IsCloserThan(ILocation anotherLocation);
    }
}