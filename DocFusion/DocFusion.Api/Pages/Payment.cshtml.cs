using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocFusion.Web.Pages;

public class Payment: PageModel
{
    public string DocumentNumber { get; set; } = $"NQNB{Guid.NewGuid().ToString()[..6].ToUpper()}";
    public DateTime ValuationDate { get; set; } = DateTime.Today;

    public void OnGet() { }
}