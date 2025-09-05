using System.ComponentModel.DataAnnotations;

namespace Saithis.TestApi.Db.Entities;

public class DummyItem
{
    public int Id { get; set; }

    [StringLength(100)]
    public string? Name { get; set; }
}
