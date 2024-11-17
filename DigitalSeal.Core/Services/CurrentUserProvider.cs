using DigitalSeal.Core.Utilities;
using DigitalSeal.Data.Models;
using DigitalSeal.Data.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace DigitalSeal.Core.Services
{
    public class CurrentUserProvider : ICurrentUserProvider
    {
        private readonly IPartyRepository _partyRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;
        public CurrentUserProvider(
            IPartyRepository partyRepository, 
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager)
        {
            _partyRepository = partyRepository;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public Task<int> GetCurrentPartyIdAsync()
            => _partyRepository.GetIdOfCurrentAsync(CurrentUserId);

        public Task<Party> GetCurrentPartyAsync()
            => _partyRepository.GetCurrentAsync(CurrentUserId);

        public async Task<User> GetCurrentUserAsync()
        {
            return await _userManager.FindByIdAsync(CurrentUserId.ToString()) ?? throw new Exception("User not logged in");
        }

        public HttpContext? HttpContext => _httpContextAccessor.HttpContext;

        private int? _currentUserID;
        public int CurrentUserId => HttpContext == null ? 0 : 
            (_currentUserID ??= AuthHelper.GetCurrentUserID(HttpContext)) ?? 0;
    }
}
