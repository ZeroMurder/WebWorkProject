using System.Collections.Generic;

public class ProjectCreateViewModel
{
    public WebWorkNew.Models.Project Project { get; set; } = new();
    public List<WebWorkNew.Models.Customer> Customers { get; set; } = new();
    public bool CanEditMargin { get; set; }
}
