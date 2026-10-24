namespace Gradebooks
{
    public class Grade
    {
        public int Id { get; set; }
        public string StudentUsername { get; set; } = "";
        public string Subject { get; set; } = "";
        public int Score { get; set; }
    }
}

