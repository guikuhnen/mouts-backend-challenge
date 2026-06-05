using Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class AuthenticateUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly AuthenticateUserHandler _handler;

    public AuthenticateUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _handler = new AuthenticateUserHandler(_userRepository, _passwordHasher, _jwtTokenGenerator);
    }

    [Fact(DisplayName = "Given valid credentials for active user When authenticating Then returns token")]
    public async Task Handle_ValidCredentials_ReturnsToken()
    {
        // Given
        var user = UserTestData.GenerateValidUser();
        user.Id = Guid.NewGuid();
        user.Status = UserStatus.Active;
        var password = "Test@123";
        var expectedToken = "jwt.token.here";

        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(password, user.Password).Returns(true);
        _jwtTokenGenerator.GenerateToken(user).Returns(expectedToken);

        var command = new AuthenticateUserCommand { Email = user.Email, Password = password };

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.Token.Should().Be(expectedToken);
        result.Email.Should().Be(user.Email);
        result.Name.Should().Be(user.Username);
    }

    [Fact(DisplayName = "Given non-existing email When authenticating Then throws UnauthorizedAccessException")]
    public async Task Handle_NonExistingEmail_ThrowsUnauthorizedException()
    {
        // Given
        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var command = new AuthenticateUserCommand { Email = "notfound@test.com", Password = "Test@123" };

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid credentials*");
    }

    [Fact(DisplayName = "Given wrong password When authenticating Then throws UnauthorizedAccessException")]
    public async Task Handle_WrongPassword_ThrowsUnauthorizedException()
    {
        // Given
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Active;

        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(Arg.Any<string>(), user.Password).Returns(false);

        var command = new AuthenticateUserCommand { Email = user.Email, Password = "WrongPassword" };

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid credentials*");
    }

    [Fact(DisplayName = "Given inactive user When authenticating Then throws UnauthorizedAccessException")]
    public async Task Handle_InactiveUser_ThrowsUnauthorizedException()
    {
        // Given
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Inactive;

        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(Arg.Any<string>(), user.Password).Returns(true);

        var command = new AuthenticateUserCommand { Email = user.Email, Password = "Test@123" };

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not active*");
    }

    [Fact(DisplayName = "Given suspended user When authenticating Then throws UnauthorizedAccessException")]
    public async Task Handle_SuspendedUser_ThrowsUnauthorizedException()
    {
        // Given
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Suspended;

        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(Arg.Any<string>(), user.Password).Returns(true);

        var command = new AuthenticateUserCommand { Email = user.Email, Password = "Test@123" };

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "Given valid authentication When handling Then JWT generator is called once")]
    public async Task Handle_ValidAuth_CallsJwtGeneratorOnce()
    {
        // Given
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Active;

        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(Arg.Any<string>(), user.Password).Returns(true);
        _jwtTokenGenerator.GenerateToken(user).Returns("token");

        var command = new AuthenticateUserCommand { Email = user.Email, Password = "Test@123" };

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        _jwtTokenGenerator.Received(1).GenerateToken(Arg.Is<User>(u => u.Email == user.Email));
    }
}
