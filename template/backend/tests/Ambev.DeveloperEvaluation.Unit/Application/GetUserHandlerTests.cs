using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class GetUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly GetUserHandler _handler;

    public GetUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetUserHandler(_userRepository, _mapper);
    }

    [Fact(DisplayName = "Given existing user ID When getting user Then returns user data")]
    public async Task Handle_ExistingUser_ReturnsUserResult()
    {
        // Given
        var user = UserTestData.GenerateValidUser();
        user.Id = Guid.NewGuid();

        var result = new GetUserResult { Id = user.Id, Name = user.Username, Email = user.Email };

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _mapper.Map<GetUserResult>(user).Returns(result);

        // When
        var response = await _handler.Handle(new GetUserCommand(user.Id), CancellationToken.None);

        // Then
        response.Should().NotBeNull();
        response.Id.Should().Be(user.Id);
        response.Name.Should().Be(user.Username);
        await _userRepository.Received(1).GetByIdAsync(user.Id, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given non-existing user ID When getting user Then throws KeyNotFoundException")]
    public async Task Handle_NonExistingUser_ThrowsKeyNotFoundException()
    {
        // Given
        var id = Guid.NewGuid();
        _userRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((User?)null);

        // When
        var act = () => _handler.Handle(new GetUserCommand(id), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact(DisplayName = "Given empty GUID When getting user Then throws ValidationException")]
    public async Task Handle_EmptyGuid_ThrowsValidationException()
    {
        // Given
        var command = new GetUserCommand(Guid.Empty);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given existing user When getting Then mapper is called with correct user")]
    public async Task Handle_ExistingUser_CallsMapperWithUser()
    {
        // Given
        var user = UserTestData.GenerateValidUser();
        user.Id = Guid.NewGuid();
        var result = new GetUserResult { Id = user.Id };

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _mapper.Map<GetUserResult>(user).Returns(result);

        // When
        await _handler.Handle(new GetUserCommand(user.Id), CancellationToken.None);

        // Then
        _mapper.Received(1).Map<GetUserResult>(Arg.Is<User>(u => u.Id == user.Id));
    }
}
