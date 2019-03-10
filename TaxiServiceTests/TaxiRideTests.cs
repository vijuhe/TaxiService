using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using TaxiService;

namespace TaxiServiceTests
{
    public class TaxiRideTests
    {
        private TaxiRide _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new TaxiRide(4, 2);
        }

        [Test]
        public void ThereMustBeAtLeastOnePassengerSeat()
        {
            Assert.Throws<ArgumentException>(() => new TaxiRide(0, 2));
        }

        [Test]
        public void KilometerPriceCannotBeZero()
        {
            Assert.Throws<ArgumentException>(() => new TaxiRide(1, 0));
        }

        [Test]
        public void KilometerPriceCannotBeNegative()
        {
            Assert.Throws<ArgumentException>(() => new TaxiRide(1, -2.50m));
        }

        [Test]
        public void NoPassengersAreTakenIfNoneAreComing()
        {
            Assert.Throws<ArgumentException>(() => _sut.TakeNewPassengers(new List<IPotentialPassenger>()));
        }

        [Test]
        public void NoPassengersAreTakenIfNullIsComing()
        {
            Assert.Throws<ArgumentException>(() => _sut.TakeNewPassengers(null));
        }

        [Test]
        public void NewPassengersAreTakenOnlyIfThereIsRoomForAllWhenTheCarIsEmpty()
        {
            var pickUpAddress = Substitute.For<ILocation>();
            var newPassengers = new List<IPotentialPassenger>
            {
                CreatePotentialPassengerFake(pickUpAddress),
                CreatePotentialPassengerFake(pickUpAddress),
                CreatePotentialPassengerFake(pickUpAddress),
                CreatePotentialPassengerFake(pickUpAddress),
                CreatePotentialPassengerFake(pickUpAddress)
            };

            Assert.That(_sut.TakeNewPassengers(newPassengers), Is.False);
        }

        [Test]
        public void NewPassengersAreTakenOnlyIfThereIsRoomForAllWhenTheCarHasPassengers()
        {
            var firstPickUpAddress = Substitute.For<ILocation>();
            var secondPickUpAddress = Substitute.For<ILocation>();
            var firstPassengers = new List<IPotentialPassenger>
            {
                CreatePotentialPassengerFake(firstPickUpAddress),
                CreatePotentialPassengerFake(firstPickUpAddress)
            };
            Assert.That(_sut.TakeNewPassengers(firstPassengers), Is.True);

            var secondPassengers = new List<IPotentialPassenger>
            {
                CreatePotentialPassengerFake(secondPickUpAddress),
                CreatePotentialPassengerFake(secondPickUpAddress),
                CreatePotentialPassengerFake(secondPickUpAddress)
            };
            Assert.That(_sut.TakeNewPassengers(secondPassengers), Is.False);
        }

        [Test]
        public void NewPassengersAreTakenOnlyIfThereIsRoomForAllWhenTheCarIsAtThePickUpLocation()
        {
            var firstPickUpAddress = Substitute.For<ILocation>();
            var firstDropOffAddress = Substitute.For<ILocation>();
            var firstPotentialPassenger = CreatePotentialPassengerFake(firstPickUpAddress);
            var secondPotentialPassenger = CreatePotentialPassengerFake(firstPickUpAddress);
            var firstPassenger = Substitute.For<IPassenger>();
            var secondPassenger = Substitute.For<IPassenger>();
            firstPotentialPassenger.ToPassenger().Returns(firstPassenger);
            secondPotentialPassenger.ToPassenger().Returns(secondPassenger);
            firstPassenger.DropOffLocation.Returns(firstDropOffAddress);
            secondPassenger.DropOffLocation.Returns(firstDropOffAddress);
            var firstPassengers = new List<IPotentialPassenger>
            {
                firstPotentialPassenger, secondPotentialPassenger
            };
            Assert.That(_sut.TakeNewPassengers(firstPassengers), Is.True);

            var secondPickUpAddress = Substitute.For<ILocation>();
            var secondPassengers = new List<IPotentialPassenger>
            {
                CreatePotentialPassengerFake(secondPickUpAddress),
                CreatePotentialPassengerFake(secondPickUpAddress),
                CreatePotentialPassengerFake(secondPickUpAddress)
            };
            firstDropOffAddress.IsCloserThan(secondPickUpAddress).Returns(true);
            Assert.That(_sut.TakeNewPassengers(secondPassengers), Is.True);
        }

        [Test]
        public void NewPassengersAreTakenOnlyIfAllTheCurrentPassengersAgreeOnIt()
        {
            var firstPotentialPassenger = Substitute.For<IPotentialPassenger>();
            var firstPassenger = Substitute.For<IPassenger>();
            var secondPotentialPassenger = Substitute.For<IPotentialPassenger>();
            var secondPassenger = Substitute.For<IPassenger>();
            var thirdPotentialPassenger = Substitute.For<IPotentialPassenger>();
            var thirdPassenger = Substitute.For<IPassenger>();
            var fourthPotentialPassenger = Substitute.For<IPotentialPassenger>();
            var firstPickUpAddress = Substitute.For<ILocation>();

            firstPotentialPassenger.ToPassenger().Returns(firstPassenger);
            firstPotentialPassenger.PickUpLocation.Returns(firstPickUpAddress);
            secondPotentialPassenger.ToPassenger().Returns(secondPassenger);
            secondPotentialPassenger.PickUpLocation.Returns(firstPickUpAddress);
            thirdPotentialPassenger.ToPassenger().Returns(thirdPassenger);

            firstPassenger.AcceptNewPassengers(Arg.Any<IReadOnlyCollection<IPotentialPassenger>>()).Returns(true);
            secondPassenger.AcceptNewPassengers(Arg.Any<IReadOnlyCollection<IPotentialPassenger>>()).Returns(true);
            thirdPassenger.AcceptNewPassengers(Arg.Any<IReadOnlyCollection<IPotentialPassenger>>()).Returns(false);

            _sut.TakeNewPassengers(new List<IPotentialPassenger> { firstPotentialPassenger, secondPotentialPassenger });

            Assert.That(_sut.TakeNewPassengers(new List<IPotentialPassenger> { thirdPotentialPassenger }), Is.True);
            Assert.That(_sut.TakeNewPassengers(new List<IPotentialPassenger> { fourthPotentialPassenger }), Is.False);
        }

        [Test]
        public void OnlyThePassengersThatAreStillRidingWhenNewPassengersArePickedUpAreAllowedToVoteOnThem()
        {
            var firstPotentialPassenger = Substitute.For<IPotentialPassenger>();
            var firstPassenger = Substitute.For<IPassenger>();
            var firstPassengerDropOffLocation = Substitute.For<ILocation>();
            var secondPotentialPassenger = Substitute.For<IPotentialPassenger>();
            var secondPassengerPickUpLocation = Substitute.For<ILocation>();

            firstPotentialPassenger.ToPassenger().Returns(firstPassenger);
            firstPassenger.DropOffLocation.Returns(firstPassengerDropOffLocation);
            secondPotentialPassenger.PickUpLocation.Returns(secondPassengerPickUpLocation);
            firstPassengerDropOffLocation.IsCloserThan(secondPassengerPickUpLocation).Returns(true);
            firstPassenger.AcceptNewPassengers(Arg.Any<IReadOnlyCollection<IPotentialPassenger>>()).Returns(false);

            _sut.TakeNewPassengers(new List<IPotentialPassenger> { firstPotentialPassenger });

            Assert.That(_sut.TakeNewPassengers(new List<IPotentialPassenger> { secondPotentialPassenger }), Is.True);
        }

        [Test]
        public void AllNewPassengersMustHaveTheSamePickUpAddress()
        {
            var firstPotentialPassenger = Substitute.For<IPotentialPassenger>();
            var secondPotentialPassenger = Substitute.For<IPotentialPassenger>();

            Assert.Throws<ArgumentException>(() => 
                _sut.TakeNewPassengers(new List<IPotentialPassenger> { firstPotentialPassenger, secondPotentialPassenger }));
        }

        [Test]
        public void ThereMustBeAtLeastOnePassengerBeforeKilometersAreCounted()
        {
            Assert.Throws<InvalidOperationException>(() => _sut.NewKilometerStarted());
        }

        [Test]
        public void OnePassengerPaysTheWholeRide()
        {
            var potentialPassenger = Substitute.For<IPotentialPassenger>();
            var passenger = Substitute.For<IPassenger>();
            var destination = Substitute.For<ILocation>();
            potentialPassenger.ToPassenger().Returns(passenger);
            passenger.DropOffLocation.Returns(destination);

            _sut.TakeNewPassengers(new List<IPotentialPassenger> { potentialPassenger });

            _sut.NewKilometerStarted();
            _sut.NewKilometerStarted();
            _sut.NewKilometerStarted();
            _sut.NewKilometerStarted();

            _sut.DestinationReached(destination);
            passenger.Received(4).AddCost(2);
            passenger.Received(1).Pay();
        }

        [Test]
        public void PassengersWithTheSamePickUpAndDropOffLocationsShareTheCostsEqually()
        {
            var firstPotentialPassenger = Substitute.For<IPotentialPassenger>();
            var firstPassenger = Substitute.For<IPassenger>();
            var secondPotentialPassenger = Substitute.For<IPotentialPassenger>();
            var secondPassenger = Substitute.For<IPassenger>();
            var pickUpLocation = Substitute.For<ILocation>();
            var dropOffLocation = Substitute.For<ILocation>();
            firstPotentialPassenger.ToPassenger().Returns(firstPassenger);
            firstPotentialPassenger.PickUpLocation.Returns(pickUpLocation);
            firstPassenger.DropOffLocation.Returns(dropOffLocation);
            secondPotentialPassenger.ToPassenger().Returns(secondPassenger);
            secondPotentialPassenger.PickUpLocation.Returns(pickUpLocation);
            secondPassenger.DropOffLocation.Returns(dropOffLocation);

            _sut.TakeNewPassengers(new List<IPotentialPassenger> { firstPotentialPassenger, secondPotentialPassenger });

            _sut.NewKilometerStarted();
            _sut.NewKilometerStarted();
            _sut.NewKilometerStarted();

            _sut.DestinationReached(dropOffLocation);
            firstPassenger.Received(3).AddCost(1);
            secondPassenger.Received(3).AddCost(1);
            firstPassenger.Received(1).Pay();
            secondPassenger.Received(1).Pay();
        }

        [Test]
        public void PassengersStartingFromTheSameLocationShareCostsOnlyOfTheirKilometers()
        {
            var firstPotentialPassenger = Substitute.For<IPotentialPassenger>();
            var firstPassenger = Substitute.For<IPassenger>();
            var secondPotentialPassenger = Substitute.For<IPotentialPassenger>();
            var secondPassenger = Substitute.For<IPassenger>();
            var thirdPotentialPassenger = Substitute.For<IPotentialPassenger>();
            var thirdPassenger = Substitute.For<IPassenger>();
            var firstPickUpLocation = Substitute.For<ILocation>();
            var firstDropOffLocation = Substitute.For<ILocation>();
            var secondDropOffLocation = Substitute.For<ILocation>();
            firstPotentialPassenger.ToPassenger().Returns(firstPassenger);
            firstPotentialPassenger.PickUpLocation.Returns(firstPickUpLocation);
            firstPassenger.DropOffLocation.Returns(firstDropOffLocation);
            secondPotentialPassenger.ToPassenger().Returns(secondPassenger);
            secondPotentialPassenger.PickUpLocation.Returns(firstPickUpLocation);
            secondPassenger.DropOffLocation.Returns(firstDropOffLocation);
            thirdPotentialPassenger.ToPassenger().Returns(thirdPassenger);
            thirdPotentialPassenger.PickUpLocation.Returns(firstPickUpLocation);
            thirdPassenger.DropOffLocation.Returns(secondDropOffLocation);

            _sut.TakeNewPassengers(new List<IPotentialPassenger> { firstPotentialPassenger, secondPotentialPassenger, thirdPotentialPassenger });

            _sut.NewKilometerStarted();
            _sut.NewKilometerStarted();
            _sut.NewKilometerStarted();
            _sut.DestinationReached(firstDropOffLocation);
            _sut.NewKilometerStarted();
            _sut.DestinationReached(secondDropOffLocation);

            firstPassenger.Received(3).AddCost(0.67m);
            secondPassenger.Received(3).AddCost(0.67m);
            thirdPassenger.Received(3).AddCost(0.67m);
            thirdPassenger.Received(1).AddCost(2);
            firstPassenger.Received(1).Pay();
            secondPassenger.Received(1).Pay();
            thirdPassenger.Received(1).Pay();
        }

        [Test]
        public void PassengersStartingFromDifferentLocationShareCostsOnlyOfTheirKilometers()
        {
            var firstPotentialPassenger = Substitute.For<IPotentialPassenger>();
            var firstPassenger = Substitute.For<IPassenger>();
            var secondPotentialPassenger = Substitute.For<IPotentialPassenger>();
            var secondPassenger = Substitute.For<IPassenger>();
            var thirdPotentialPassenger = Substitute.For<IPotentialPassenger>();
            var thirdPassenger = Substitute.For<IPassenger>();
            var firstPickUpLocation = Substitute.For<ILocation>();
            var secondPickUpLocation = Substitute.For<ILocation>();
            var firstDropOffLocation = Substitute.For<ILocation>();
            var secondDropOffLocation = Substitute.For<ILocation>();
            firstPotentialPassenger.ToPassenger().Returns(firstPassenger);
            firstPotentialPassenger.PickUpLocation.Returns(firstPickUpLocation);
            firstPassenger.DropOffLocation.Returns(firstDropOffLocation);
            secondPotentialPassenger.ToPassenger().Returns(secondPassenger);
            secondPotentialPassenger.PickUpLocation.Returns(firstPickUpLocation);
            secondPassenger.DropOffLocation.Returns(firstDropOffLocation);
            thirdPotentialPassenger.ToPassenger().Returns(thirdPassenger);
            thirdPotentialPassenger.PickUpLocation.Returns(secondPickUpLocation);
            thirdPassenger.DropOffLocation.Returns(secondDropOffLocation);
            firstPassenger.AcceptNewPassengers(Arg.Any<IReadOnlyCollection<IPotentialPassenger>>()).Returns(true);
            secondPassenger.AcceptNewPassengers(Arg.Any<IReadOnlyCollection<IPotentialPassenger>>()).Returns(true);

            _sut.TakeNewPassengers(new List<IPotentialPassenger> { firstPotentialPassenger, secondPotentialPassenger });
            _sut.NewKilometerStarted();
            _sut.TakeNewPassengers(new List<IPotentialPassenger> { thirdPotentialPassenger });
            _sut.NewKilometerStarted();
            _sut.DestinationReached(firstDropOffLocation);
            _sut.NewKilometerStarted();
            _sut.DestinationReached(secondDropOffLocation);

            firstPassenger.Received(1).AddCost(1);
            secondPassenger.Received(1).AddCost(1);
            firstPassenger.Received(1).AddCost(0.67m);
            secondPassenger.Received(1).AddCost(0.67m);
            thirdPassenger.Received(1).AddCost(0.67m);
            thirdPassenger.Received(1).AddCost(2);
            firstPassenger.Received(1).Pay();
            secondPassenger.Received(1).Pay();
            thirdPassenger.Received(1).Pay();
        }

        private static IPotentialPassenger CreatePotentialPassengerFake(ILocation location)
        {
            var potentialPassenger = Substitute.For<IPotentialPassenger>();
            potentialPassenger.PickUpLocation.Returns(location);
            return potentialPassenger;
        }
    }
}