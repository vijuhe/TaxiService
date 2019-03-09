using System;
using System.Collections.Generic;
using System.Linq;

namespace TaxiService
{
    public class Taxi
    {
        private readonly ICarRouteService _routeService;
        private readonly IPassenger[] _passengers;
        private byte _nextFreePassengerSeat;

        public Taxi(byte maxAmountOfPassengers, ICarRouteService routeService)
        {
            if (maxAmountOfPassengers < 1)
            {
                throw new ArgumentException("There must be at least one passenger seat in the taxi.", nameof(maxAmountOfPassengers));
            }
            _routeService = routeService ?? throw new ArgumentNullException(nameof(routeService));
            _passengers = new IPassenger[maxAmountOfPassengers];
            _nextFreePassengerSeat = 0;
        }

        public void DestinationReached(string address)
        {

        }

        public bool TakeNewPassengers(IReadOnlyCollection<IPotentialPassenger> potentialPassengers)
        {
            if (potentialPassengers == null || !potentialPassengers.Any())
            {
                throw new ArgumentException("No new passengers given.");
            }
            if (!NewPassengersHaveTheSamePickupLocation(potentialPassengers))
            {
                throw new ArgumentException("All new passengers must have the same pick up address.");
            }

            if (NewPassengersFitIn(potentialPassengers.Count) && AllPassengersAgreeOnTakingNewPassengers(potentialPassengers))
            {
                AddPassengers(potentialPassengers);
                return true;
            }

            return false;
        }

        private bool NewPassengersHaveTheSamePickupLocation(IReadOnlyCollection<IPotentialPassenger> potentialPassengers)
        {
            return potentialPassengers.Select(p => p.PickUpLocation).Distinct().Count() == 1;
        }

        private bool AllPassengersAgreeOnTakingNewPassengers(IReadOnlyCollection<IPotentialPassenger> potentialPassengers)
        {
            return _passengers.All(p => p == null || p.DropOffLocation.IsCloserThan(potentialPassengers.First().PickUpLocation) 
                                                  || p.AcceptNewPassengers(potentialPassengers));
        }

        private void AddPassengers(IEnumerable<IPotentialPassenger> potentialPassengers)
        {
            foreach (IPotentialPassenger potentialPassenger in potentialPassengers)
            {
                _passengers[_nextFreePassengerSeat] = potentialPassenger.ToPassenger();
                _nextFreePassengerSeat++;
            }
        }

        private bool NewPassengersFitIn(int newPassengerCount)
        {
            return newPassengerCount <= _passengers.Length - _nextFreePassengerSeat;
        }
    }
}
