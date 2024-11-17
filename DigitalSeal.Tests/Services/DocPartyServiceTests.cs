using DigitalSeal.Core.Exceptions;
using DigitalSeal.Core.Models.DocParty;
using DigitalSeal.Core.Services;
using DigitalSeal.Data.Models;
using DigitalSeal.Data.Repositories;
using FakeItEasy;
using FluentAssertions;
using LanguageExt.Common;

namespace DigitalSeal.Tests.Services
{
    public class DocPartyServiceTests
    {
        private readonly IDocPartyService _docPartyService;
        private readonly IPartyRepository _partyRepository;
        private readonly IDocPartyRepository _docPartyRepository;
        private readonly IDocService _docService;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IDocPartyValidator _docPartyValidator;
        private readonly IDocPartyNotificationService _docPartyNotificationService;
        public DocPartyServiceTests()
        {
            _partyRepository = A.Fake<IPartyRepository>();
            _docPartyRepository = A.Fake<IDocPartyRepository>();
            _docService = A.Fake<IDocService>();
            _currentUserProvider = A.Fake<ICurrentUserProvider>();
            _docPartyValidator = A.Fake<IDocPartyValidator>();
            _docPartyNotificationService = A.Fake<IDocPartyNotificationService>();

            _docPartyService = new DocPartyService(
                _partyRepository,
                _docPartyRepository,
                _docService,
                _currentUserProvider,
                _docPartyValidator,
                _docPartyNotificationService);

            A.CallTo(() => _currentUserProvider.CurrentUserId).Returns(CurrentUserId);
        }

        private const int CurrentUserId = 1;

        // Scenarios of Add:
        // No modification right.
        // All added.
        // Some added, some are already added.


        [Theory]
        [InlineData(1)]
        public async Task GetModifiableDoc_ReturnsModifiableDoc(int docId)
        {
            // Arrange
            var doc = A.Fake<Document>();

            A.CallTo(() => _docService.GetByIdAsync(docId, A<bool>.Ignored, A<bool>.Ignored))
                .Returns(doc);

            ValidationException? expectedEx;
            A.CallTo(() => _docPartyValidator.CanModify(doc, CurrentUserId, out expectedEx, A<bool>.Ignored))
                .Returns(true);

            // Act
            Result<Document> result = await _docPartyService.GetModifiableDocAsync(docId);

            // Assert
            result.AssertSuccess(success => success.Should().BeEquivalentTo(doc));
        }

        [Theory]
        [InlineData(1)]
        public async Task GetModifiableDoc_ReturnsValidationExceptionWhenDocNotExist(int docId)
        {
            // Arrange
            A.CallTo(() => _docService.GetByIdAsync(docId, A<bool>.Ignored, A<bool>.Ignored))
                .Returns<Document?>(null);

            // Act
            Result<Document> result = await _docPartyService.GetModifiableDocAsync(docId);

            ValidationException? ex;
            A.CallTo(() => _docPartyValidator.CanModify(A<Document>.Ignored, A<int>.Ignored, out ex, A<bool>.Ignored))
                .MustNotHaveHappened();

            result.AssertFailure(
                exception => exception.Should().BeOfType<ValidationException>());
        }

        [Theory]
        [InlineData(1)]
        public async Task GetModifiableDoc_ReturnsValidationExceptionWhenPermissionDenied(int docId)
        {
            // Arrange
            var doc = A.Fake<Document>();

            A.CallTo(() => _docService.GetByIdAsync(docId, A<bool>.Ignored, A<bool>.Ignored))
                .Returns(doc);

            ValidationException? expectedEx = ValidationException.Error("No permission");
            A.CallTo(() => _docPartyValidator.CanModify(doc, CurrentUserId, out expectedEx, A<bool>.Ignored))
                .Returns(false)
                .AssignsOutAndRefParameters(expectedEx);

            // Act
            Result<Document> result = await _docPartyService.GetModifiableDocAsync(docId);

            result.AssertFailure(
                exception => exception.Should().BeEquivalentTo(expectedEx));
        }


        [Fact]
        public async Task AddAsync_PreventsModification()
        {
            // Arrange
            var model = A.Fake<AddDocPartyModel>();
            var doc = A.Fake<Document>();

            //var getDocResult = new Result<Document>(ValidationException.Error("No permission"));
            //A.CallTo(() => _docPartyService.GetModifiableDocAsync(model.DocId))
            //    .Returns(getDocResult);

            //A.CallTo(() => _docService.GetByIdAsync(model.DocId, true, false))
            //    .Returns(doc);

            //ValidationException? expectedEx = ValidationException.Error("Test error");
            //A.CallTo(() => _docPartyValidator.CanModify(doc, CurrentUserId, out expectedEx, true))
            //    .Returns(false)
            //    .AssignsOutAndRefParameters(expectedEx);

            // Act
            Result<bool> result = await _docPartyService.AddAsync(model);

            //A.CallTo(() => _docPartyService.GetModifiableDocAsync(A<int>.Ignored))
            //    .MustHaveHappenedOnceExactly();

            A.CallTo(() => _docPartyValidator.CanAddAsync(A<IEnumerable<Party>>.Ignored, A<int>.Ignored, A<int>.Ignored))
                .MustNotHaveHappened();

            A.CallTo(() => _docPartyRepository.Add(A<DocumentParty>.Ignored))
                .MustNotHaveHappened();

            A.CallTo(() => _docPartyNotificationService.InformAddedParty(
                A<Document>.Ignored, A<int>.Ignored, A<int>.Ignored, A<int>.Ignored))
                .MustNotHaveHappened();

            A.CallTo(() => _docPartyRepository.SaveChangesAsync())
                .MustNotHaveHappened();

            A.CallTo(() => _docPartyNotificationService.SendDocPartyAddedEmailsAsync(A<Document>.Ignored, A<List<string>>.Ignored))
                .MustNotHaveHappened();

            result.AssertFailure(
                exception => exception.Should().BeEquivalentTo(getDocResult.GetException()));
        }
        
        [Theory]
        [InlineData(3)]
        public async Task AddAsync_AddsPartiesSuccessfully(int partyIdsCount)
        {
            // Arrange
            var model = A.Fake<AddDocPartyModel>();
            model.PartyIds = [.. A.CollectionOfDummy<int>(partyIdsCount)];

            var doc = A.Fake<Document>();

            A.CallTo(() => _docService.GetByIdAsync(model.DocId, A<bool>.Ignored, A<bool>.Ignored))
                .Returns(doc);

            ValidationException? ex = null;
            A.CallTo(() => _docPartyValidator.CanModify(doc, CurrentUserId, out ex, true))
                .Returns(true)
                .AssignsOutAndRefParameters([null]);

            var currParty = A.Fake<Party>();
            A.CallTo(() => _currentUserProvider.GetCurrentPartyAsync()).Returns(currParty);

            List<Party> partiesToAdd = [.. A.CollectionOfFake<Party>(model.PartyIds.Length)];
            foreach (Party party in partiesToAdd)
            {
                party.User = A.Fake<User>();
            }

            int orgId = currParty.OrganizationId;

            A.CallTo(() => _partyRepository.GetAsync(model.PartyIds, orgId))
                .Returns(partiesToAdd);

            A.CallTo(() => _docPartyValidator.CanAddAsync(partiesToAdd, doc.Id, orgId))
                .Returns((ValidationException?)null);

            // Act
            Result<bool> result = await _docPartyService.AddAsync(model);

            // Assert
            A.CallTo(() => _docService.GetByIdAsync(model.DocId, true, false))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _docPartyValidator.CanModify(doc, CurrentUserId, out ex, true))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _docPartyValidator.CanAddAsync(partiesToAdd, doc.Id, orgId))
                .MustHaveHappenedOnceExactly();

            int partiesToAddCount = model.PartyIds.Length;

            A.CallTo(() => _docPartyRepository.Add(A<DocumentParty>.Ignored))
                .MustHaveHappened(partiesToAddCount, Times.Exactly);

            A.CallTo(() => _docPartyNotificationService.InformAddedParty(
                A<Document>.Ignored, A<int>.Ignored, A<int>.Ignored, A<int>.Ignored))
                .MustHaveHappened(partiesToAddCount, Times.Exactly);

            A.CallTo(() => _docPartyRepository.SaveChangesAsync())
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _docPartyNotificationService.SendDocPartyAddedEmailsAsync(A<Document>.Ignored, A<List<string>>.Ignored))
                .MustHaveHappenedOnceExactly();

            //result.IsSuccess.Should().BeTrue();
            //result.IsFaulted.Should().BeFalse();
            //_ = result.Match(
            //    success => success.Should().BeTrue(),
            //    exception => throw TestExceptionHelper.ExpectedSuccess(exception));
            result.AssertSuccess(success => success.Should().BeTrue());
        }

        [Theory]
        [InlineData(3)]
        public async Task AddAsync_PreventsInvalidPartyAddition(int partyIdsCount)
        {
            // Arrange
            var model = A.Fake<AddDocPartyModel>();
            model.PartyIds = [.. A.CollectionOfDummy<int>(partyIdsCount)];

            var doc = A.Fake<Document>();

            A.CallTo(() => _docService.GetByIdAsync(model.DocId, A<bool>.Ignored, A<bool>.Ignored))
                .Returns(doc);

            ValidationException? ex = null;
            A.CallTo(() => _docPartyValidator.CanModify(doc, CurrentUserId, out ex, true))
                .Returns(true)
                .AssignsOutAndRefParameters([null]);

            var currParty = A.Fake<Party>();
            A.CallTo(() => _currentUserProvider.GetCurrentPartyAsync()).Returns(currParty);

            List<Party> partiesToAdd = [.. A.CollectionOfFake<Party>(model.PartyIds.Length)];

            int orgId = currParty.OrganizationId;

            A.CallTo(() => _partyRepository.GetAsync(model.PartyIds, orgId))
                .Returns(partiesToAdd);

            var invalidPartyEx = ValidationException.Error("Invalid party");
            A.CallTo(() => _docPartyValidator.CanAddAsync(partiesToAdd, doc.Id, orgId))
                .Returns(invalidPartyEx);

            // Act
            Result<bool> result = await _docPartyService.AddAsync(model);

            // Assert
            A.CallTo(() => _docService.GetByIdAsync(model.DocId, true, false))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _docPartyValidator.CanModify(doc, CurrentUserId, out ex, true))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _docPartyValidator.CanAddAsync(partiesToAdd, doc.Id, orgId))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _docPartyRepository.Add(A<DocumentParty>.Ignored))
                .MustNotHaveHappened();

            A.CallTo(() => _docPartyNotificationService.InformAddedParty(
                A<Document>.Ignored, A<int>.Ignored, A<int>.Ignored, A<int>.Ignored))
                .MustNotHaveHappened();

            A.CallTo(() => _docPartyRepository.SaveChangesAsync())
                .MustNotHaveHappened();

            A.CallTo(() => _docPartyNotificationService.SendDocPartyAddedEmailsAsync(A<Document>.Ignored, A<List<string>>.Ignored))
                .MustNotHaveHappened();

            //result.IsSuccess.Should().BeFalse();
            //result.IsFaulted.Should().BeTrue();
            //_ = result.Match(
            //    success => throw TestExceptionHelper.ExpectedFailure(),
            //    exception =>
            //    {
            //        exception.Should().BeOfType<ValidationException>();
            //        exception.Should().BeEquivalentTo(invalidPartyEx);
            //        return true;
            //    });

            result.AssertFailure(
                exception => exception.Should().BeEquivalentTo(invalidPartyEx));
        }

        [Fact]
        public async Task RemoveAsync_PreventsModification()
        {
            // Arrange
            var model = A.Fake<AddDocPartyModel>();
            var doc = A.Fake<Document>();

            A.CallTo(() => _docService.GetByIdAsync(model.DocId, true, false))
                .Returns(doc);

            ValidationException? expectedEx = ValidationException.Error("Test error");
            A.CallTo(() => _docPartyValidator.CanModify(doc, CurrentUserId, out expectedEx, true))
                .Returns(false)
                .AssignsOutAndRefParameters(expectedEx);

            // Act
            Result<bool> result = await _docPartyService.AddAsync(model);

            // Assert
            //result.IsSuccess.Should().BeFalse();
            //result.IsFaulted.Should().BeTrue();
            //_ = result.Match(
            //    success => throw TestExceptionHelper.ExpectedFailure(),
            //    exception =>
            //    {
            //        exception.Should().BeOfType<ValidationException>();
            //        exception.Should().BeEquivalentTo(expectedEx);
            //        return true;
            //    });

            result.AssertFailure(
                exception => exception.Should().BeEquivalentTo(expectedEx));
        }
    }
}
