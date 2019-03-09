using System.Collections.Generic;

namespace TaxiService
{
    public interface IPassenger
    {
        bool AcceptNewPassengers(IReadOnlyCollection<IPotentialPassenger> potentialPassengers);
        ILocation DropOffLocation { get; }
    }
}