using System.ComponentModel.DataAnnotations.Schema;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace MediaWiz.Forums.Models
{
    [Table("ForumPostHitCounter")]
    public class ViewCounter
    {

        [Column("Node_Id")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public int NodeId { get; set; }

        [Column("Views")]
        public int Views { get; set; }

    }
}