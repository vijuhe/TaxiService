using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using TaxiService;

namespace TaxiServiceTests
{
    public class TaxiTests
    {
        private ICarRouteService _routeService;
        private Taxi _sut;

        [SetUp]
        public void Setup()
        {
            _routeService = Substitute.For<ICarRouteService>();
            _sut = new Taxi(4, _routeService);
        }

        [Test]
        public void ThereMustBeAtLeastOnePassengerSeat()
        {
            Assert.Throws<ArgumentException>(() => new Taxi(0, _routeService));
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

        private static IPotentialPassenger CreatePotentialPassengerFake(ILocation location)
        {
            var potentialPassenger = Substitute.For<IPotentialPassenger>();
            potentialPassenger.PickUpLocation.Returns(location);
            return potentialPassenger;
        }
    }
}