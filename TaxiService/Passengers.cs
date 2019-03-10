using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TaxiService
{
    class Passengers : IEnumerable<IPassenger>
    {
        private readonly IPassenger[] _passengers;
        private byte _nextFreePassengerSeat;

        public Passengers(byte maxAmount)
        {
            _passengers = new IPassenger[maxAmount];
            _nextFreePassengerSeat = 0;
        }

        public int Count()
        {
            return _passengers.Count(p => p != null);
        }

        public IEnumerable<IPassenger> DropPassengersOffAt(ILocation address)
        {
            var leavingPassengers = _passengers.Where(p => p != null && p.DropOffLocation.Equals(address)).ToList();
            for(int i = 0; i < _passengers.Length; i++)
            {
                foreach (IPassenger leavingPassenger in leavingPassengers)
                {
                    if (leavingPassenger == _passengers[i])
                    {
                        _passengers[i] = null;
                    }
                }
            }
            MovePassengersFirst();
            return leavingPassengers;
        }

        public bool Any()
        {
            return _passengers.Any(p => p != null);
        }

        public void Add(IEnumerable<IPotentialPassenger> potentialPassengers)
        {
            foreach (IPotentialPassenger potentialPassenger in potentialPassengers)
            {
                _passengers[_nextFreePassengerSeat++] = potentialPassenger.ToPassenger();
            }
        }

        public bool DoNewPassengersFitIn(ILocation pickUpLocation, int newPassengerCount)
        {
            int numberOfPassengersLeavingBeforePickUp = _passengers.Count(p => p != null && p.DropOffLocation.IsCloserThan(pickUpLocation));
            return newPassengerCount + _nextFreePassengerSeat - numberOfPassengersLeavingBeforePickUp <= _passengers.Length;
        }

        public IEnumerator<IPassenger> GetEnumerator()
        {
            return _passengers.Where(p => p != null).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void MovePassengersFirst()
        {
            for (int i = 0; i < _passengers.Length; i++)
            {
                if (_passengers[i] == null && i + 1 < _passengers.Length)
                {
                    _passengers[i] = _passengers[i + 1];
                    _passengers[i + 1] = null;
                }
            }
        }
    }
}
