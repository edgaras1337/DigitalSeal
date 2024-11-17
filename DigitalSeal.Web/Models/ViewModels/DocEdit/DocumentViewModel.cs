using DigitalSeal.Core.Services;
using DigitalSeal.Web.Services;
using System.ComponentModel.DataAnnotations;

namespace DigitalSeal.Web.Models.ViewModels.DocEdit
{
    public class DocumentViewModel : BasePageModel
    {
        public int DocId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;

        public string CreatedDate { get; set; } = string.Empty;

        //[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? Deadline2 { get; set; }

        public bool IsAuthor { get; set; }
        public SignStatus SigningStatus { get; set; }
        public string? Deadline { get; set; }
        public bool HasDeadline => !string.IsNullOrEmpty(Deadline);
        public bool IsDeadlinePassed { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;

        public string PartyListColumnDefs { get; set; } = string.Empty;
        public string PossiblePartyListColumnDefs { get; set; } = string.Empty;
        public string SignaturesListColumnDefs { get; set; } = string.Empty;
    }
}
