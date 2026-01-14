using System;

namespace MyAPIv3.DTOs
{
    public class CategoryDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class CreateCategoryDto
    {
        public string Title { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }

    public class UpdateCategoryDto
    {
        public string Title { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}

