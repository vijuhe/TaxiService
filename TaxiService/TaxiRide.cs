using System;
using System.Collections.Generic;
using System.Linq;

namespace TaxiService
{
    public class TaxiRide
    {
        private readonly decimal _kilometerPrice;
        private readonly Passengers _passengers;

        public TaxiRide(byte maxAmountOfPassengers, decimal kilometerPrice)
        {
            if (maxAmountOfPassengers < 1)
            {
                throw new ArgumentException("There must be at least one passenger seat in the taxi.", nameof(maxAmountOfPassengers));
            }
            if (kilometerPrice <= 0)
            {
                throw new ArgumentException("Price per kilometer must be a positive number.");
            }

            _passengers = new Passengers(maxAmountOfPassengers);
            _kilometerPrice = kilometerPrice;
        }

        public void DestinationReached(ILocation address)
        {
            foreach (IPassenger passenger in _passengers.DropPassengersOffAt(address))
            {
                passenger.Pay();
            }
        }

        public bool TakeNewPassengers(IReadOnlyCollection<IPotentialPassenger> potentialPassengers)
        {
            if (potentialPassengers == null || !potentialPassengers.Any())
            {
                throw new ArgumentException("No new passengers given.");
            }
            if (!NewPassengersHaveTheSamePickUpLocation(potentialPassengers))
            {
                throw new ArgumentException("All new passengers must have the same pick up address.");
            }

            if (_passengers.DoNewPassengersFitIn(potentialPassengers.First().PickUpLocation, potentialPassengers.Count) 
                && AllPassengersAgreeOnTakingNewPassengers(potentialPassengers))
            {
                _passengers.Add(potentialPassengers);
                return true;
            }

            return false;
        }

        public void NewKilometerStarted()
        {
            if (!_passengers.Any())
            {
                throw new InvalidOperationException("There must be passengers when kilometers and costs are collected.");
            }

            decimal costPerPassenger = Math.Round(_kilometerPrice / _passengers.Count(), 2);
            foreach (IPassenger passenger in _passengers)
            {
                passenger.AddCost(costPerPassenger);
            }
        }

        private static bool NewPassengersHaveTheSamePickUpLocation(IReadOnlyCollection<IPotentialPassenger> potentialPassengers)
        {
            return potentialPassengers.Select(p => p.PickUpLocation).Distinct().Count() == 1;
        }

        private bool AllPassengersAgreeOnTakingNewPassengers(IReadOnlyCollection<IPotentialPassenger> potentialPassengers)
        {
            return _passengers.All(p => p == null || p.DropOffLocation.IsCloserThan(potentialPassengers.First().PickUpLocation) 
                                                  || p.AcceptNewPassengers(potentialPassengers));
        }
    }
}
