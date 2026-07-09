namespace ExamObjectives.Models
{
    public class CertificationModel
    {
        public string Exam { get; set; } = "";
        public string ExamCode { get; set; } = "";
        public List<Domain> Domains { get; set; } = [];
    }
    
    public class Domain
    {
        public int Id { get; set; }
        public string Number { get; set; } = "";
        public string Title { get; set; } = "";
        public int Weight { get; set; }
        public bool Completed { get; set; }
        public List<Objective> Objectives { get; set; } = [];
    }
    
    public class Objective
    {
        public int Id { get; set; }
        public string Number { get; set; } = "";
        public string Title { get; set; } = "";
        public bool Completed { get; set; }
        public List<Bullet> Bullets { get; set; } = [];
    }
    
    public class Bullet
    {
        public string Text { get; set; } = "";
        public bool Completed { get; set; }
        public List<Bullet> Children { get; set; } = [];
    }
}