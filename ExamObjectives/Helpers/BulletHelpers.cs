using ExamObjectives.Models;

namespace ExamObjectives.Helpers
{
    public static class BulletHelpers
    {
        private static int CountBullets(IEnumerable<Bullet> bullets)
        {
            return bullets.Sum(bullet => 1 + CountBullets(bullet.Children));
        }

        private static int CountCompletedBullets(IEnumerable<Bullet> bullets)
        {
            return bullets.Sum(bullet => (bullet.Completed ? 1 : 0) + CountCompletedBullets(bullet.Children));
        }

        public static int GetDomainBulletCount(Domain domain)
        {
            return domain.Objectives.Sum(objective => CountBullets(objective.Bullets));
        }

        public static int GetTotalCompletedBulletCount(List<Domain> domains)
        {
            return domains.Sum(GetDomainCompletedBulletCount);
        }

        public static int GetDomainCompletedBulletCount(Domain domain)
        {
            return domain.Objectives.Sum(objective => CountCompletedBullets(objective.Bullets));
        }

        public static int GetTotalBulletCount(List<Domain> domains)
        {
            return domains.Sum(GetDomainBulletCount);
        }
        
        public static int GetTotalProgressPercentage(List<Domain> domains)
        {
            int total = GetTotalBulletCount(domains);
            
            if (total == 0)
            {
                return 0;
            }
            
            return (int)Math.Round((double)GetTotalCompletedBulletCount(domains) / total * 100);
        }

        public static int GetDomainProgressPercentage(Domain domain)
        {
            int total = GetDomainBulletCount(domain);

            if (total == 0)
            {
                return 0;
            }
            
            return (int)Math.Round((double)GetDomainCompletedBulletCount(domain) / total * 100);
        }

        public static string GetPathValue(IReadOnlyList<int> path)
        {
            return string.Join(".", path);
        }
    }
}