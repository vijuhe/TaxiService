using System.Collections.Generic;

namespace TaxiService
{
    public interface IPassenger
    {
        bool AcceptNewPassengers(IReadOnlyCollection<IPotentialPassenger> potentialPassengers);
        ILocation DropOffLocation { get; }
        void Pay();
        void AddCost(decimal amount);
    }
}