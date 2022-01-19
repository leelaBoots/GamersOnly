using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    // this will force EF to name the table "Photos" in the database
    [Table("Photos")]
    public class Photo
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public bool IsMain { get; set; }
        public string PublicId { get; set; }

        // this "defines" the relationship between AppUser and Photo
        public AppUser AppUser { get; set; }
        public int AppUserId { get; set; }

    }
}