namespace Domain.Entities;

public class Sport
{
    public int Id { get; set; }
    public string Key { get; set; }
    public string Group { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int HasOutrights { get; set; }
}