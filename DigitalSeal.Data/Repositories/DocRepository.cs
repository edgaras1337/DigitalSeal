using DigitalSeal.Data.Models;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DigitalSeal.Data.Repositories
{
    public readonly record struct UserId(int Value);
    public readonly record struct PartyId(int Value);

    internal class DocRepository : Repository<Document>, IDocRepository
    {
        public DocRepository(AppDbContext context) 
            : base(context)
        {
        }

        public Task<Document?> GetAsync<TOwnerId>(int docId, TOwnerId ownerId, 
            bool includeRelatedData = false, bool includeFileContent = false) where TOwnerId : struct
        {
            return GetByIdAsyncInternal<TOwnerId>(docId, ownerId, includeRelatedData, includeFileContent);
        }

        public Task<List<Document>> GetByIdsAsync<TOwnerId>(IEnumerable<int> docIds, TOwnerId ownerId, 
            bool includeRelatedData = false, bool includeFileContent = false) where TOwnerId : struct
        {
            return GetByIdsAsyncInternal<TOwnerId>(docIds, ownerId, includeRelatedData, includeFileContent);
        }

        public Task<Document?> GetByIdAsync(int docId, bool includeRelatedData = false, bool includeFileContent = false)
        {
            return GetByIdAsyncInternal<UserId>(docId, null, includeRelatedData, includeFileContent);
        }

        public Task<List<Document>> GetByIdsAsync(IEnumerable<int> docIds, bool includeRelatedData = false, bool includeFileContent = false)
        {
            return GetByIdsAsyncInternal<UserId>(docIds, null, includeRelatedData, includeFileContent);
        }

        public async Task<List<Document>> GetOwnedAsync(List<PartyId> partyIds, bool includeRelatedData = false,
            bool includeFileContent = false)
        {
            return await Context.Documents
                //.Include(doc => doc.DocumentParties)
                .Where(doc => doc.DocumentParties.Any(dp => 
                    ((DocPermission)dp.Permission).HasFlag(DocPermission.Owner)))
                .ToListAsync();
        }

        public async Task<List<Document>> GetByOrgIds(List<int> orgIds)
        {
            return await Context.Documents
                .Where(doc => doc.DocumentParties.Any(dp => orgIds.Contains(dp.Party.OrganizationId)))
                .ToListAsync();
        }



        private async Task<Document?> GetByIdAsyncInternal<TOwnerId>(int docId, TOwnerId? ownerId = default,
            bool includeRelatedData = false, bool includeFileContent = false)
            where TOwnerId : struct
        {
            IQueryable<Document> query = GetDocByOwnerQuery(ownerId, includeRelatedData, includeFileContent);
            return await query.FirstOrDefaultAsync(doc => doc.Id == docId);
        }

        private async Task<List<Document>> GetByIdsAsyncInternal<TOwnerId>(IEnumerable<int> docIds, TOwnerId? ownerId = default,
            bool includeRelatedData = false, bool includeFileContent = false)
            where TOwnerId : struct
        {
            IQueryable<Document> query = GetDocByOwnerQuery(ownerId, includeRelatedData, includeFileContent)
                .Where(doc => docIds.Contains(doc.Id));

            return await query.ToListAsync();
        }


        private IQueryable<Document> GetDocByOwnerQuery<TOwnerId>(TOwnerId? ownerId = default,
            bool includeRelatedData = false, bool includeFileContent = false)
            where TOwnerId : struct
        {
            IQueryable<Document> docQuery = Context.Documents;
            if (includeFileContent)
            {
                docQuery = docQuery.Include(doc => doc.FileContent);
            }

            if (!includeRelatedData)
            {
                return FilterByOwner(docQuery, ownerId);
            }

            docQuery = docQuery.Include(doc => doc.FileContent)
                .Include(doc => doc.DocumentParties)
                    .ThenInclude(docParty => docParty.Party)
                        .ThenInclude(party => party.User)
                .Include(doc => doc.DocumentParties)
                    .ThenInclude(docParty => docParty.SignatureInfos);

            return FilterByOwner(docQuery, ownerId);
        }

        private static IQueryable<Document> FilterByOwner<TOwner>(IQueryable<Document> query, TOwner? ownerId)
            where TOwner : struct
        {
            if (!ownerId.HasValue)
            {
                return query;
            }

            return FilterByOwnerIternal(query, ownerId.Value);

            //if (ownerId is UserId userId)
            //{
            //    query = query.Where(doc => doc.DocumentParties.Any(docParty =>
            //        docParty.Party.UserId == userId.Value));
            //}
            //else if (ownerId is PartyId partyId)
            //{
            //    query = query.Where(doc => doc.DocumentParties.Any(docParty =>
            //        docParty.PartyId == partyId.Value));
            //}

            //return query;
        }

        private static IQueryable<Document> FilterByOwnerIternal<TOwner>(IQueryable<Document> query, params TOwner[] ownerIds)
            where TOwner : struct
        {
            if (ownerIds.Length == 0)
            {
                return query;
            }

            //TOwner firstOwner = ownerIds[0];

            int length = ownerIds.Length;


            if (typeof(TOwner) == typeof(UserId))
            {
                var userIds = ownerIds.Cast<UserId>().Select(x => x.Value).ToHashSet();
                query = query.Where(doc => doc.DocumentParties.Any(docParty =>
                    userIds.Contains(docParty.Party.UserId)));
            }

            else if (typeof(TOwner) == typeof(PartyId))
            {
                var partyIds = ownerIds.Cast<PartyId>().Select(x => x.Value).ToHashSet();
                query = query.Where(doc => doc.DocumentParties.Any(docParty =>
                    partyIds.Contains(docParty.PartyId)));
            }

            return query;
        }





        //public async Task<Document?> GetByIdWithRelatedDataAsync(int docId, int userId, bool includeContent = false)
        //{
        //    return await GetDocQuery(includeContent)
        //        .Where(doc => doc.Id == docId)
        //        .FirstOrDefaultAsync(GetDocAvailableForUserExpr(userId));
        //}

        //public async Task<List<Document>> GetByIdsWithRelatedDataAsync(IEnumerable<int> docIds,
        //    int userId, bool includeContent = false)
        //{
        //    return await GetDocQuery(includeContent)
        //        .Where(doc => docIds.Contains(doc.Id))
        //        .Where(GetDocAvailableForUserExpr(userId))
        //        .ToListAsync();
        //}

        //public async Task<List<Document>> GetByIdsAsync(IEnumerable<int> docIds)
        //{
        //    return await Context.Documents
        //        .Where(doc => docIds.Contains(doc.Id))
        //        .ToListAsync();
        //}

        //private IQueryable<Document> GetDocQuery(bool includeContent, bool includeOtherRelatedEntities)
        //{
        //    IQueryable<Document> docQuery = Context.Documents;
        //    if (includeContent)
        //        docQuery = docQuery.Include(doc => doc.FileContent);

        //    if (!includeOtherRelatedEntities)
        //        return docQuery;

        //    return docQuery.Include(doc => doc.FileContent)
        //        .Include(doc => doc.DocumentParties)
        //            .ThenInclude(docParty => docParty.Party)
        //                .ThenInclude(party => party.User)
        //        .Include(doc => doc.DocumentParties)
        //            .ThenInclude(docParty => docParty.SignatureInfos);
        //}

        //private static Expression<Func<Document, bool>> GetDocAvailableForUserExpr(int userId)
        //{
        //    return doc => doc.DocumentParties.Any(docParty => docParty.Party.UserId == userId);
        //}
    }

    public interface IDocRepository : IRepository<Document>
    {
        Task<Document?> GetAsync<TOwnerId>(int docId, TOwnerId ownerId,
            bool includeRelatedData = false, bool includeFileContent = false) where TOwnerId : struct;
        Task<List<Document>> GetByIdsAsync<TOwnerId>(IEnumerable<int> docIds, TOwnerId ownerId,
            bool includeRelatedData = false, bool includeFileContent = false) where TOwnerId : struct;

        Task<Document?> GetByIdAsync(int docId,
            bool includeRelatedData = false, bool includeFileContent = false);
        Task<List<Document>> GetByIdsAsync(IEnumerable<int> docIds,
            bool includeRelatedData = false, bool includeFileContent = false);

        Task<List<Document>> GetOwnedAsync(List<PartyId> partyIds, bool includeRelatedData = false,
            bool includeFileContent = false);

        Task<List<Document>> GetByOrgIds(List<int> orgIds);

        //Task<List<Document>> GetByIdsAsync(IEnumerable<int> docIds);
        //Task<Document?> GetByIdWithRelatedDataAsync(int docId, int userId, bool includeContent = false);
        //Task<List<Document>> GetByIdsWithRelatedDataAsync(IEnumerable<int> docIds, int userId, bool includeContent = false);
    }
}
