namespace TaxiService
{
    public interface IPotentialPassenger
    {
        IPassenger ToPassenger();
        ILocation PickUpLocation { get; }
    }
}