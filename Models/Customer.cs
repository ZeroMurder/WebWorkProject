using WebWorkNew.Enums;

namespace WebWorkNew.Models;

public class Customer
{
    public int Id { get; set; }
    public string Inn { get; set; } = "";
    public CustomerType Type { get; set; }
    public string? Name { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
}