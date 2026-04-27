using CoreFitnessClub.Application.DTOs;
using CoreFitnessClub.Application.Services;
using CoreFitnessClub.Domain.Entities;
using CoreFitnessClub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoreFitnessClub.Tests;

// ── MembershipService Tests ───────────────────────────────────────────────────

public class MembershipServiceTests
{
    private readonly Mock<IMembershipPlanRepository> _planRepoMock;
    private readonly Mock<IUserMembershipRepository> _userMemberRepoMock;
    private readonly MembershipService _sut;

    public MembershipServiceTests()
    {
        _planRepoMock       = new Mock<IMembershipPlanRepository>();
        _userMemberRepoMock = new Mock<IUserMembershipRepository>();
        _sut = new MembershipService(_planRepoMock.Object, _userMemberRepoMock.Object);
    }

    [Fact]
    public async Task GetActivePlansAsync_ReturnsAllActivePlans()
    {
        // Arrange
        var plans = new List<MembershipPlan>
        {
            new() { Id = 1, Name = "Standard", Price = 495, MaxClasses = 20, Features = [] },
            new() { Id = 2, Name = "Premium",  Price = 595, MaxClasses = 20, Features = [] }
        };
        _planRepoMock.Setup(r => r.GetActiveWithFeaturesAsync()).ReturnsAsync(plans);

        // Act
        var result = (await _sut.GetActivePlansAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Standard");
        result[1].Name.Should().Be("Premium");
    }

    [Fact]
    public async Task GetActivePlansAsync_MapsPriceCorrectly()
    {
        // Arrange
        var plans = new List<MembershipPlan>
        {
            new() { Id = 1, Name = "Standard", Price = 495.00m, MaxClasses = 20, Features = [] }
        };
        _planRepoMock.Setup(r => r.GetActiveWithFeaturesAsync()).ReturnsAsync(plans);

        // Act
        var result = (await _sut.GetActivePlansAsync()).First();

        // Assert
        result.Price.Should().Be(495.00m);
    }

    [Fact]
    public async Task GetActivePlansAsync_MapsFeaturesCorrectly()
    {
        // Arrange
        var plans = new List<MembershipPlan>
        {
            new()
            {
                Id = 1, Name = "Standard", Price = 495, MaxClasses = 20,
                Features =
                [
                    new PlanFeature { Description = "Standard Locker" },
                    new PlanFeature { Description = "Group Classes" }
                ]
            }
        };
        _planRepoMock.Setup(r => r.GetActiveWithFeaturesAsync()).ReturnsAsync(plans);

        // Act
        var result = (await _sut.GetActivePlansAsync()).First();

        // Assert
        result.Features.Should().HaveCount(2);
        result.Features.Should().Contain("Standard Locker");
        result.Features.Should().Contain("Group Classes");
    }

    [Fact]
    public async Task GetActivePlansAsync_ReturnsEmpty_WhenNoPlans()
    {
        // Arrange
        _planRepoMock.Setup(r => r.GetActiveWithFeaturesAsync())
                     .ReturnsAsync(new List<MembershipPlan>());

        // Act
        var result = await _sut.GetActivePlansAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task JoinPlanAsync_CancelsExistingMembership_BeforeJoiningNew()
    {
        // Arrange
        var existingMembership = new UserMembership
        {
            Id = 10, ApplicationUserId = "user1",
            MembershipPlanId = 1, IsActive = true
        };
        _userMemberRepoMock.Setup(r => r.GetByUserIdAsync("user1"))
                           .ReturnsAsync(existingMembership);

        // Act
        await _sut.JoinPlanAsync("user1", 2);

        // Assert — old membership should be deactivated
        existingMembership.IsActive.Should().BeFalse();
        existingMembership.EndDate.Should().NotBeNull();
        _userMemberRepoMock.Verify(r => r.Update(existingMembership), Times.Once);
        _userMemberRepoMock.Verify(r => r.AddAsync(It.Is<UserMembership>(
            m => m.MembershipPlanId == 2 && m.ApplicationUserId == "user1")), Times.Once);
        _userMemberRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task JoinPlanAsync_AddsNewMembership_WhenNoExisting()
    {
        // Arrange
        _userMemberRepoMock.Setup(r => r.GetByUserIdAsync("user1"))
                           .ReturnsAsync((UserMembership?)null);

        // Act
        await _sut.JoinPlanAsync("user1", 1);

        // Assert
        _userMemberRepoMock.Verify(r => r.Update(It.IsAny<UserMembership>()), Times.Never);
        _userMemberRepoMock.Verify(r => r.AddAsync(It.Is<UserMembership>(
            m => m.ApplicationUserId == "user1" && m.MembershipPlanId == 1)), Times.Once);
    }

    [Fact]
    public async Task CancelMembershipAsync_DeactivatesUserMembership()
    {
        // Arrange
        var membership = new UserMembership
        {
            Id = 5, ApplicationUserId = "user1",
            IsActive = true, MembershipPlanId = 1
        };
        _userMemberRepoMock.Setup(r => r.GetByUserIdAsync("user1")).ReturnsAsync(membership);

        // Act
        await _sut.CancelMembershipAsync("user1");

        // Assert
        membership.IsActive.Should().BeFalse();
        membership.EndDate.Should().NotBeNull();
        _userMemberRepoMock.Verify(r => r.Update(membership), Times.Once);
        _userMemberRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CancelMembershipAsync_DoesNothing_WhenNoActiveMembership()
    {
        // Arrange
        _userMemberRepoMock.Setup(r => r.GetByUserIdAsync("user1"))
                           .ReturnsAsync((UserMembership?)null);

        // Act
        await _sut.CancelMembershipAsync("user1");

        // Assert
        _userMemberRepoMock.Verify(r => r.Update(It.IsAny<UserMembership>()), Times.Never);
        _userMemberRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetUserCurrentPlanAsync_ReturnsNull_WhenNoMembership()
    {
        // Arrange
        _userMemberRepoMock.Setup(r => r.GetByUserIdAsync("user1"))
                           .ReturnsAsync((UserMembership?)null);

        // Act
        var result = await _sut.GetUserCurrentPlanAsync("user1");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserCurrentPlanAsync_ReturnsPlan_WhenMembershipExists()
    {
        // Arrange
        var membership = new UserMembership
        {
            Id = 1, ApplicationUserId = "user1", IsActive = true,
            MembershipPlan = new MembershipPlan
            {
                Id = 2, Name = "Premium", Price = 595, MaxClasses = 20, Features = []
            }
        };
        _userMemberRepoMock.Setup(r => r.GetByUserIdAsync("user1")).ReturnsAsync(membership);

        // Act
        var result = await _sut.GetUserCurrentPlanAsync("user1");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Premium");
        result.Price.Should().Be(595);
    }
}

// ── BookingService Tests ──────────────────────────────────────────────────────

public class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _bookingRepoMock;
    private readonly BookingService _sut;

    public BookingServiceTests()
    {
        _bookingRepoMock = new Mock<IBookingRepository>();
        _sut = new BookingService(_bookingRepoMock.Object);
    }

    [Fact]
    public async Task CreateBookingAsync_ReturnsTrue_WhenNoExistingBooking()
    {
        // Arrange
        var dto = new CreateBookingDto
        {
            FitnessClassId = 1,
            ClassDateTime  = DateTime.UtcNow.AddDays(1)
        };
        _bookingRepoMock.Setup(r => r.UserHasBookingAsync("user1", 1, dto.ClassDateTime))
                        .ReturnsAsync(false);

        // Act
        var result = await _sut.CreateBookingAsync("user1", dto);

        // Assert
        result.Should().BeTrue();
        _bookingRepoMock.Verify(r => r.AddAsync(It.Is<Booking>(
            b => b.ApplicationUserId == "user1" &&
                 b.FitnessClassId    == 1 &&
                 b.Status            == BookingStatus.Confirmed)), Times.Once);
        _bookingRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateBookingAsync_ReturnsFalse_WhenBookingAlreadyExists()
    {
        // Arrange
        var dto = new CreateBookingDto { FitnessClassId = 1, ClassDateTime = DateTime.UtcNow.AddDays(1) };
        _bookingRepoMock.Setup(r => r.UserHasBookingAsync("user1", 1, dto.ClassDateTime))
                        .ReturnsAsync(true);

        // Act
        var result = await _sut.CreateBookingAsync("user1", dto);

        // Assert
        result.Should().BeFalse();
        _bookingRepoMock.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public async Task CancelBookingAsync_ReturnsFalse_WhenBookingNotFound()
    {
        // Arrange
        _bookingRepoMock.Setup(r => r.GetByIdAsync(99))
                        .ReturnsAsync((Booking?)null);

        // Act
        var result = await _sut.CancelBookingAsync("user1", 99);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CancelBookingAsync_ReturnsFalse_WhenBookingBelongsToDifferentUser()
    {
        // Arrange
        var booking = new Booking { Id = 1, ApplicationUserId = "other-user", Status = BookingStatus.Confirmed };
        _bookingRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);

        // Act
        var result = await _sut.CancelBookingAsync("user1", 1);

        // Assert
        result.Should().BeFalse();
        _bookingRepoMock.Verify(r => r.Update(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public async Task CancelBookingAsync_SetsStatusCancelled_WhenValid()
    {
        // Arrange
        var booking = new Booking { Id = 1, ApplicationUserId = "user1", Status = BookingStatus.Confirmed };
        _bookingRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);

        // Act
        var result = await _sut.CancelBookingAsync("user1", 1);

        // Assert
        result.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Cancelled);
        _bookingRepoMock.Verify(r => r.Update(booking), Times.Once);
        _bookingRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetUserBookingsAsync_MapsBookingToDto()
    {
        // Arrange
        var bookings = new List<Booking>
        {
            new()
            {
                Id = 1, ApplicationUserId = "user1",
                ClassDateTime = new DateTime(2026, 6, 1, 10, 0, 0),
                Status = BookingStatus.Confirmed,
                FitnessClass = new FitnessClass { Name = "CrossFit", Category = "Strength" }
            }
        };
        _bookingRepoMock.Setup(r => r.GetByUserIdAsync("user1")).ReturnsAsync(bookings);

        // Act
        var result = (await _sut.GetUserBookingsAsync("user1")).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].ClassName.Should().Be("CrossFit");
        result[0].Status.Should().Be("Confirmed");
    }
}

// ── DTO Validation Tests ──────────────────────────────────────────────────────

public class DtoValidationTests
{
    private static IList<System.ComponentModel.DataAnnotations.ValidationResult> Validate(object dto)
    {
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var ctx     = new System.ComponentModel.DataAnnotations.ValidationContext(dto);
        System.ComponentModel.DataAnnotations.Validator.TryValidateObject(dto, ctx, results, true);
        return results;
    }

    [Fact]
    public void RegisterDto_IsValid_WithCorrectData()
    {
        var dto = new RegisterDto
        {
            FirstName       = "John",
            LastName        = "Doe",
            Email           = "john@example.com",
            Password        = "Passw0rd!",
            ConfirmPassword = "Passw0rd!"
        };
        Validate(dto).Should().BeEmpty();
    }

    [Fact]
    public void RegisterDto_IsInvalid_WithMissingEmail()
    {
        var dto = new RegisterDto
        {
            FirstName       = "John",
            LastName        = "Doe",
            Email           = "",
            Password        = "Passw0rd!",
            ConfirmPassword = "Passw0rd!"
        };
        Validate(dto).Should().NotBeEmpty();
    }

    [Fact]
    public void RegisterDto_IsInvalid_WithShortPassword()
    {
        var dto = new RegisterDto
        {
            FirstName = "John", LastName = "Doe",
            Email = "j@j.com", Password = "abc", ConfirmPassword = "abc"
        };
        Validate(dto).Should().NotBeEmpty();
    }

    [Fact]
    public void ContactDto_IsInvalid_WithEmptyMessage()
    {
        var dto = new ContactDto
        {
            FirstName = "Jane", LastName = "Doe",
            Email = "jane@example.com", Message = ""
        };
        Validate(dto).Should().NotBeEmpty();
    }

    [Fact]
    public void LoginDto_IsInvalid_WithBadEmail()
    {
        var dto = new LoginDto { Email = "not-an-email", Password = "Password1!" };
        Validate(dto).Should().NotBeEmpty();
    }
}
