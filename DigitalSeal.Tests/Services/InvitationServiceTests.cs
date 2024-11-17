using DigitalSeal.Core.Exceptions;
using DigitalSeal.Core.Models.Invitation;
using DigitalSeal.Core.Services;
using DigitalSeal.Data.Models;
using DigitalSeal.Data.Repositories;
using FakeItEasy;
using FluentAssertions;
using LanguageExt.Common;
using Microsoft.Extensions.Localization;

namespace DigitalSeal.Tests.Services
{
    public class InvitationServiceTests
    {
        private readonly IInvitationService _invitationService;
        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly IStringLocalizer<InvitationService> _localizer;
        public InvitationServiceTests()
        {
            _userNotificationRepository = A.Fake<IUserNotificationRepository>();
            _localizer = A.Fake<IStringLocalizer<InvitationService>>();

            _invitationService = new InvitationService(_userNotificationRepository, _localizer);
        }

        [Fact]
        public async Task InvitationService_InviteToOrganizationAsync_CreatesInvites()
        {
            // Arrange
            var model = A.Fake<InviteToOrgModel>();

            // Act
            await _invitationService.InviteToOrganizationAsync(model);

            // Assert
            foreach (int userId in model.InvitedUserIds)
            {
                A.CallTo(() => _userNotificationRepository.Add(A<UserNotification>.That.Matches(u =>
                    u.OrganizationId == model.OrgId &&
                    u.SenderId == userId &&
                    u.ReceiverId == userId &&
                    //u.Title == _localizer["InvitedToOrg"] &&
                    u.InviteNotification.Type == (int)InviteType.Organization
                ))).MustHaveHappenedOnceExactly();
            }

            A.CallTo(() => _userNotificationRepository.SaveChangesAsync()).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData(true, InviteType.Organization)]
        [InlineData(false, InviteType.Organization)]
        [InlineData(true, InviteType.Document)]
        [InlineData(false, InviteType.Document)]
        public async Task InvitationService_RespondToInvitationAsync_ReturnsTrue(bool isAccept, InviteType type)
        {
            // Arrange
            var userNotification = A.Fake<UserNotification>();
            userNotification.InviteNotification = new InviteNotification
            {
                State = (int)InviteNotificationState.Pending,
                Type = (int)type,
            };
            var model = new RespondToInviteModel(userNotification, isAccept);

            // Act
            Result<bool> result = await _invitationService.RespondToInvitationAsync(model);

            // Assert
            A.CallTo(() => _userNotificationRepository.SaveChangesAsync()).MustHaveHappenedOnceExactly();
            result.IsSuccess.Should().BeTrue();
            result.IsFaulted.Should().BeFalse();
            _ = result.Match(
                success => success.Should().BeTrue(),
                exception => throw TestExceptionHelper.ExpectedSuccess(exception));
        }

        [Theory]
        [InlineData(InviteType.Organization)]
        [InlineData(InviteType.Document)]
        public async Task InvitationService_GetInvitationAsync_ReturnsValidInvitation(InviteType inviteType)
        {
            // Arrange
            //int inviteId = 1001;
            //int receiverId = 2001;
            var model = A.Fake<GetInvitationModel>(); //new GetInvitationModel(inviteId, receiverId, inviteType);
            var inviteNotification = A.Fake<UserNotification>();
            inviteNotification.InviteNotification = new InviteNotification
            {
                State = (int)InviteNotificationState.Pending,
                Type = (int)inviteType,
            };
            A.CallTo(() => _userNotificationRepository.GetByInviteIdAsync(model.InviteId)).Returns(inviteNotification);

            // Act
            Result<UserNotification> result = await _invitationService.GetInvitationAsync(model);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IsFaulted.Should().BeFalse();
            _ = result.Match(
                userNotif => userNotif.Should().BeEquivalentTo(inviteNotification),
                exception => throw TestExceptionHelper.ExpectedSuccess(exception));
        }

        [Theory]
        [InlineData(InviteType.Organization)]
        [InlineData(InviteType.Document)]
        public async Task InvitationService_GetInvitationAsync_ReturnsValidationException(InviteType inviteType)
        {
            // Arrange
            //int inviteId = 1001;
            //int receiverId = 2001;
            var model = A.Fake<GetInvitationModel>(); //new GetInvitationModel(inviteId, receiverId, inviteType);
            var inviteNotification = A.Fake<UserNotification>();
            inviteNotification.InviteNotification = new InviteNotification
            {
                State = (int)InviteNotificationState.Pending,
                Type = (int)inviteType,
            };

            //var inviteNotification = new UserNotification
            //{
            //    Id = inviteId,
            //    OrganizationId = 3001,
            //    SenderId = 4001,
            //    ReceiverId = receiverId,
            //    Created = DateTime.UtcNow,
            //    Title = "Test",
            //    InviteNotification = new InviteNotification
            //    {
            //        State = (int)InviteNotificationState.Pending,
            //        Type = (int)inviteType
            //    }
            //};

            A.CallTo(() => _userNotificationRepository.GetByInviteIdAsync(model.InviteId)).Returns((UserNotification?)null);

            // Act
            Result<UserNotification> result = await _invitationService.GetInvitationAsync(model);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsFaulted.Should().BeTrue();
            _ = result.Match(
                _ => throw TestExceptionHelper.ExpectedFailure(),
                TestExceptionHelper.AssertValidationEx);
        }
    }
}
