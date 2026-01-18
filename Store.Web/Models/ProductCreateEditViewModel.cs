using System.ComponentModel.DataAnnotations;

namespace Store.Web.Models;

public class ProductCreateViewModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    [Display(Name = "Price")]
    public decimal Price { get; set; }

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")]
    [Url(ErrorMessage = "Please enter a valid URL.")]
    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }
}

public class ProductEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    [Display(Name = "Price")]
    public decimal Price { get; set; }

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")]
    [Url(ErrorMessage = "Please enter a valid URL.")]
    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }
}
