using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TaxiService
{
    class Passengers : IEnumerable<IPassenger>
    {
        private readonly byte _maxAmount;
        private readonly List<IPassenger> _passengers;

        public Passengers(byte maxAmount)
        {
            _maxAmount = maxAmount;
            _passengers = new List<IPassenger>();
        }

        public IEnumerable<IPassenger> DropPassengersOffAt(ILocation address)
        {
            var leavingPassengers = _passengers.Where(p => p.DropOffLocation.Equals(address)).ToList();
            foreach (IPassenger passenger in leavingPassengers)
            {
                _passengers.Remove(passenger);
            }
            return leavingPassengers;
        }

        public void Add(IEnumerable<IPotentialPassenger> potentialPassengers)
        {
            _passengers.AddRange(potentialPassengers.Select(p => p.ToPassenger()));
        }

        public bool DoNewPassengersFitIn(ILocation pickUpLocation, int newPassengerCount)
        {
            int numberOfPassengersLeavingBeforePickUp = _passengers.Count(p => p.DropOffLocation.IsCloserThan(pickUpLocation));
            return _passengers.Count + newPassengerCount - numberOfPassengersLeavingBeforePickUp <= _maxAmount;
        }

        public IEnumerator<IPassenger> GetEnumerator()
        {
            return _passengers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
