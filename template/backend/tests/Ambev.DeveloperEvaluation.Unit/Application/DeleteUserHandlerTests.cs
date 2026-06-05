using Ambev.DeveloperEvaluation.Application.Users.DeleteUser;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class DeleteUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly DeleteUserHandler _handler;

    public DeleteUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _handler = new DeleteUserHandler(_userRepository);
    }

    [Fact(DisplayName = "Given existing user ID When deleting Then returns success")]
    public async Task Handle_ExistingUser_ReturnsSuccess()
    {
        // Given
        var id = Guid.NewGuid();
        _userRepository.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(true);

        // When
        var result = await _handler.Handle(new DeleteUserCommand(id), CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        await _userRepository.Received(1).DeleteAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given non-existing user ID When deleting Then throws KeyNotFoundException")]
    public async Task Handle_NonExistingUser_ThrowsKeyNotFoundException()
    {
        // Given
        var id = Guid.NewGuid();
        _userRepository.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(false);

        // When
        var act = () => _handler.Handle(new DeleteUserCommand(id), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact(DisplayName = "Given empty GUID When deleting Then throws ValidationException")]
    public async Task Handle_EmptyGuid_ThrowsValidationException()
    {
        // When
        var act = () => _handler.Handle(new DeleteUserCommand(Guid.Empty), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given valid ID When deleting Then repository is called exactly once")]
    public async Task Handle_ValidId_CallsRepositoryOnce()
    {
        // Given
        var id = Guid.NewGuid();
        _userRepository.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(true);

        // When
        await _handler.Handle(new DeleteUserCommand(id), CancellationToken.None);

        // Then
        await _userRepository.Received(1).DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
