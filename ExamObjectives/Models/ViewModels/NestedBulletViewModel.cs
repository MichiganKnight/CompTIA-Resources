namespace ExamObjectives.Models.ViewModels
{
    public class NestedBulletViewModel
    {
        public required Bullet Bullet { get; init; }
        public required string ExamCode { get; init; }
        public required int DomainId { get; init; }
        public required int ObjectiveId { get; init; }
        public required int BulletIndex { get; init; }
        public required List<int> ParentPath { get; init; }
        public required int Depth { get; init; }
    }
}