using DigitalSeal.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security;

namespace DigitalSeal.Data.Models
{
    [Flags]
    public enum DocPermission
    {
        // Full access
        Owner = 1 << 0,
        Read = 1 << 1,
        Sign = 1 << 2,
        Invite = 1 << 3,
        Update = 1 << 4,
        Delete = 1 << 5,
    }

    public class DocumentParty : WithPermission<DocPermission>
    {
        //public int Id { get; set; }

        [ForeignKey(nameof(Document))]
        public int DocId { get; set; }
        public Document Document { get; set; } = null!;

        //[ForeignKey(nameof(User))]
        //public int UserId { get; set; }
        //public User? User { get; set; }

        [ForeignKey(nameof(Party))]
        public int PartyId { get; set; }
        public Party Party { get; set; } = null!;

        [NotMapped]
        public bool IsAuthor => Has(DocPermission.Owner);

        //public int Permissions { get; set; }

        public ICollection<SignatureInfo> SignatureInfos { get; set; } = [];
        public ICollection<DocumentNote> DocumentNotes { get; set; } = [];
    }


    public class WithPermission<TPermissions> : BaseEntity
        where TPermissions : Enum
    {
        public int Permission { get; set; }

        public bool Has(TPermissions flag)
            => ((TPermissions)Enum.ToObject(typeof(TPermissions), Permission)).HasFlag(flag);
    }

    public static class WithPermissionExtenstion 
    {
        public static Expression<Func<WithPermission<TPermission>, bool>> HasPermission<TPermission>(
            this WithPermission<TPermission> obj, TPermission permission) 
            where TPermission : Enum
        {
            return obj => ((TPermission)Enum.ToObject(typeof(TPermission), obj.Permission)).HasFlag(permission);
        }
    }

    public class PermissionHelper
    {
        public static bool Has<TPermissions>(int permission, TPermissions flag) where TPermissions : Enum
            => ((TPermissions)Enum.ToObject(typeof(TPermissions), permission)).HasFlag(flag);
    }
}
