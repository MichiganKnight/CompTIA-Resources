namespace ExamObjectives.Models.ViewModels
{
    public class BulletCardViewModel
    {
        public required string ExamCode { get; init; }
        public required Domain Domain { get; init; }
        public required Objective Objective { get; init; }
        public required Bullet Bullet { get; init; }
        public required int BulletIndex { get; init; }
    }
}