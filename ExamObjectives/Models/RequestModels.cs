namespace ExamObjectives.Models
{
    public class ProgressUpdateRequest
    {
        public string ExamCode { get; set; } = "";
        public string ItemType { get; set; } = "";
        public int DomainId { get; set; }
        public int ObjectiveId { get; set; }
        public int BulletIndex { get; set; } = -1;
        public int ChildIndex { get; set; } = -1;
        public List<int> ChildPath { get; set; } = [];
        public bool Completed { get; set; }
    }

    public class ChecklistEditRequest
    {
        public string ExamCode { get; set; } = "";
        public string ItemType { get; set; } = "";
        public int DomainId { get; set; }
        public int ObjectiveId { get; set; }
        public int BulletIndex { get; set; } = -1;
        public int ChildIndex { get; set; } = -1;
        public List<int> ChildPath { get; set; } = [];
        public string Number { get; set; } = "";
        public string Title { get; set; } = "";
        public string Text { get; set; } = "";
        public int Weight { get; set; } = 0;
        public List<Objective> Objectives { get; set; } = [];
        public List<Bullet> Bullets { get; set; } = [];
        public List<Bullet> Children { get; set; } = [];
    }
}