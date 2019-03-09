using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public IEnumerable<IPassenger> WhoDropOffAt(ILocation address)
        {
            return _passengers.Where(p => p != null && p.DropOffLocation.Equals(address));
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

        public bool NewPassengersFitIn(int newPassengerCount)
        {
            return newPassengerCount + _nextFreePassengerSeat <= _passengers.Length;
        }

        public IEnumerator<IPassenger> GetEnumerator()
        {
            return _passengers.Where(p => p != null).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
