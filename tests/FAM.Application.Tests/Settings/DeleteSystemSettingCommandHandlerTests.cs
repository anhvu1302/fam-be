using FAM.Application.Settings.Commands.DeleteSystemSetting;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.Common.Entities;

using Moq;

namespace FAM.Application.Tests.Settings;

public class DeleteSystemSettingCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISystemSettingRepository> _repositoryMock;
    private readonly DeleteSystemSettingCommandHandler _handler;

    public DeleteSystemSettingCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<ISystemSettingRepository>();
        _unitOfWorkMock.Setup(x => x.SystemSettings).Returns(_repositoryMock.Object);
        _handler = new DeleteSystemSettingCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingSetting_ShouldDeleteSetting()
    {
        // Arrange
        var setting = SystemSetting.Create("test_key", "Test Setting");
        var command = new DeleteSystemSettingCommand(1);

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, default))
            .ReturnsAsync(setting);

        // Act
        await _handler.Handle(command, default);

        // Assert
        _repositoryMock.Verify(x => x.Delete(setting), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentSetting_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DeleteSystemSettingCommand(999);

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, default))
            .ReturnsAsync((SystemSetting?)null);

        // Act & Assert
        NotFoundException exception =
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, default));

        Assert.Equal(ErrorCodes.SETTING_NOT_FOUND, exception.ErrorCode);
        _repositoryMock.Verify(x => x.Delete(It.IsAny<SystemSetting>()), Times.Never);
    }
}
