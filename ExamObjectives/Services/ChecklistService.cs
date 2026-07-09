using ExamObjectives.Models;
using Newtonsoft.Json;

namespace ExamObjectives.Services
{
    public class ChecklistService
    {
        private readonly IWebHostEnvironment _environment;
        
        public ChecklistService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<CertificationModel?> LoadCertificationAsync(string examCode)
        {
            string? jsonFileName = GetJsonFileName(examCode);
            
            if (jsonFileName == null)
            {
                return null;
            }
            
            string jsonPath = Path.Combine(_environment.WebRootPath, "data", jsonFileName);
            
            if (!File.Exists(jsonPath))
            {
                return null;
            }
            
            string json = await File.ReadAllTextAsync(jsonPath);
            return JsonConvert.DeserializeObject<CertificationModel>(json);
        }
        
        public async Task SaveCertificationAsync(string examCode, CertificationModel certification)
        {
            string? jsonFileName = GetJsonFileName(examCode);
            
            if (jsonFileName == null)
            {
                return;
            }
            
            string jsonPath = Path.Combine(_environment.WebRootPath, "data", jsonFileName);
            string updatedJson = JsonConvert.SerializeObject(certification, Formatting.Indented);
            
            await File.WriteAllTextAsync(jsonPath, updatedJson);
        }

        public async Task<ChecklistServiceResult> AddChecklistItemAsync(ChecklistEditRequest request)
        {
            CertificationModel? certification = await LoadCertificationAsync(request.ExamCode);

            if (certification == null)
            {
                return ChecklistServiceResult.BadRequest("Invalid Exam JSON");
            }

            Domain? domain = certification.Domains.FirstOrDefault(d => d.Id == request.DomainId);
            Objective? objective = domain?.Objectives.FirstOrDefault(o => o.Id == request.ObjectiveId);

            switch (request.ItemType)
            {
                case "domain":
                    certification.Domains.Add(new Domain
                    {
                        Id = certification.Domains.Any() ? certification.Domains.Max(d => d.Id) + 1 : 1,
                        Number = request.Number,
                        Title = request.Title,
                        Weight = request.Weight,
                        Completed = false,
                        Objectives = request.Objectives
                    });
                    break;

                case "objective":
                    if (domain == null)
                    {
                        return ChecklistServiceResult.NotFound("Domain Not Found");
                    }

                    domain.Objectives.Add(new Objective
                    {
                        Id = domain.Objectives.Any() ? domain.Objectives.Max(o => o.Id) + 1 : 1,
                        Number = request.Number,
                        Title = request.Title,
                        Completed = false,
                        Bullets = request.Bullets
                    });
                    break;

                case "bullet":
                    if (objective == null)
                    {
                        return ChecklistServiceResult.NotFound("Objective Not Found");
                    }

                    objective.Bullets.Add(new Bullet
                    {
                        Text = request.Text,
                        Completed = false,
                        Children = request.Children
                    });
                    break;

                case "child-bullet":
                    if (objective == null)
                    {
                        return ChecklistServiceResult.NotFound("Objective Not Found");
                    }

                    List<Bullet>? children = GetChildrenCollectionByPath(objective, request.BulletIndex, request.ChildPath);

                    if (children == null)
                    {
                        return ChecklistServiceResult.BadRequest("Invalid Bullet Path");
                    }

                    children.Add(new Bullet
                    {
                        Text = request.Text,
                        Completed = false,
                        Children = request.Children
                    });
                    break;

                default:
                    return ChecklistServiceResult.BadRequest("Invalid Item Type");
            }

            await SaveCertificationAsync(request.ExamCode, certification);

            return ChecklistServiceResult.Ok("Checklist Item Added");
        }

        public async Task<ChecklistServiceResult> EditChecklistItemAsync(ChecklistEditRequest request)
        {
            CertificationModel? certification = await LoadCertificationAsync(request.ExamCode);

            if (certification == null)
            {
                return ChecklistServiceResult.BadRequest("Invalid Exam JSON");
            }

            Domain? domain = certification.Domains.FirstOrDefault(d => d.Id == request.DomainId);
            Objective? objective = domain?.Objectives.FirstOrDefault(o => o.Id == request.ObjectiveId);

            switch (request.ItemType)
            {
                case "domain":
                    if (domain == null)
                    {
                        return ChecklistServiceResult.NotFound("Domain Not Found");
                    }

                    domain.Number = request.Number;
                    domain.Title = request.Title;
                    domain.Weight = request.Weight;
                    domain.Objectives = request.Objectives;
                    break;

                case "objective":
                    if (objective == null)
                    {
                        return ChecklistServiceResult.NotFound("Objective Not Found");
                    }

                    objective.Number = request.Number;
                    objective.Title = request.Title;
                    objective.Bullets = request.Bullets;
                    break;

                case "bullet":
                    if (objective == null)
                    {
                        return ChecklistServiceResult.NotFound("Objective Not Found");
                    }

                    Bullet? bullet = GetBulletByPath(objective, request.BulletIndex, []);

                    if (bullet == null)
                    {
                        return ChecklistServiceResult.BadRequest("Invalid Bullet Index");
                    }

                    bullet.Text = request.Text;
                    bullet.Children = request.Children;
                    break;

                case "child-bullet":
                    if (objective == null)
                    {
                        return ChecklistServiceResult.NotFound("Objective Not Found");
                    }

                    Bullet? childBullet = GetBulletByPath(objective, request.BulletIndex, request.ChildPath);

                    if (childBullet == null)
                    {
                        return ChecklistServiceResult.BadRequest("Invalid Bullet Path");
                    }

                    childBullet.Text = request.Text;
                    childBullet.Children = request.Children;
                    break;

                default:
                    return ChecklistServiceResult.BadRequest("Invalid Item Type");
            }

            await SaveCertificationAsync(request.ExamCode, certification);

            return ChecklistServiceResult.Ok("Checklist Item Updated");
        }

        public async Task<ChecklistServiceResult> DeleteChecklistItemAsync(ChecklistEditRequest request)
        {
            CertificationModel? certification = await LoadCertificationAsync(request.ExamCode);

            if (certification == null)
            {
                return ChecklistServiceResult.BadRequest("Invalid Exam JSON");
            }

            Domain? domain = certification.Domains.FirstOrDefault(d => d.Id == request.DomainId);
            Objective? objective = domain?.Objectives.FirstOrDefault(o => o.Id == request.ObjectiveId);

            switch (request.ItemType)
            {
                case "domain":
                    if (domain == null)
                    {
                        return ChecklistServiceResult.NotFound("Domain Not Found");
                    }

                    certification.Domains.Remove(domain);
                    break;

                case "objective":
                    if (domain == null || objective == null)
                    {
                        return ChecklistServiceResult.NotFound("Objective Not Found");
                    }

                    domain.Objectives.Remove(objective);
                    break;

                case "bullet":
                    if (objective == null)
                    {
                        return ChecklistServiceResult.NotFound("Objective Not Found");
                    }

                    if (!RemoveBulletByPath(objective, request.BulletIndex, []))
                    {
                        return ChecklistServiceResult.BadRequest("Invalid Bullet Index");
                    }
                    break;

                case "child-bullet":
                    if (objective == null)
                    {
                        return ChecklistServiceResult.NotFound("Objective Not Found");
                    }

                    if (!RemoveBulletByPath(objective, request.BulletIndex, request.ChildPath))
                    {
                        return ChecklistServiceResult.BadRequest("Invalid Bullet Path");
                    }
                    break;

                default:
                    return ChecklistServiceResult.BadRequest("Invalid Item Type");
            }

            await SaveCertificationAsync(request.ExamCode, certification);

            return ChecklistServiceResult.Ok("Checklist Item Deleted");
        }

        public async Task<ChecklistServiceResult> UpdateProgressAsync(ProgressUpdateRequest request)
        {
            CertificationModel? certification = await LoadCertificationAsync(request.ExamCode);

            if (certification == null)
            {
                return ChecklistServiceResult.BadRequest("Invalid Exam JSON");
            }

            Domain? domain = certification.Domains.FirstOrDefault(d => d.Id == request.DomainId);

            if (domain == null)
            {
                return ChecklistServiceResult.NotFound("Domain Not Found");
            }

            switch (request.ItemType)
            {
                case "domain":
                    domain.Completed = request.Completed;
                    break;

                case "objective":
                {
                    Objective? objective = domain.Objectives.FirstOrDefault(o => o.Id == request.ObjectiveId);

                    if (objective == null)
                    {
                        return ChecklistServiceResult.NotFound("Objective Not Found");
                    }

                    objective.Completed = request.Completed;
                    break;
                }

                case "bullet":
                {
                    Objective? objective = domain.Objectives.FirstOrDefault(o => o.Id == request.ObjectiveId);

                    if (objective == null)
                    {
                        return ChecklistServiceResult.NotFound("Objective Not Found");
                    }

                    Bullet? bullet = GetBulletByPath(objective, request.BulletIndex, []);

                    if (bullet == null)
                    {
                        return ChecklistServiceResult.BadRequest("Invalid Bullet Index");
                    }

                    bullet.Completed = request.Completed;
                    break;
                }

                case "child-bullet":
                {
                    Objective? objective = domain.Objectives.FirstOrDefault(o => o.Id == request.ObjectiveId);

                    if (objective == null)
                    {
                        return ChecklistServiceResult.NotFound("Objective Not Found");
                    }

                    Bullet? childBullet = GetBulletByPath(objective, request.BulletIndex, request.ChildPath);

                    if (childBullet == null)
                    {
                        return ChecklistServiceResult.BadRequest("Invalid Bullet Path");
                    }

                    childBullet.Completed = request.Completed;
                    break;
                }

                default:
                    return ChecklistServiceResult.BadRequest("Invalid Item Type");
            }

            await SaveCertificationAsync(request.ExamCode, certification);

            return ChecklistServiceResult.Ok("Progress Updated");
        }

        private static Bullet? GetBulletByPath(Objective objective, int bulletIndex, List<int> childPath)
        {
            if (bulletIndex < 0 || bulletIndex >= objective.Bullets.Count)
            {
                return null;
            }

            Bullet currentBullet = objective.Bullets[bulletIndex];

            foreach (int childIndex in childPath)
            {
                if (childIndex < 0 || childIndex >= currentBullet.Children.Count)
                {
                    return null;
                }

                currentBullet = currentBullet.Children[childIndex];
            }

            return currentBullet;
        }

        private static List<Bullet>? GetChildrenCollectionByPath(Objective objective, int bulletIndex, List<int> parentPath)
        {
            if (parentPath.Count == 0)
            {
                if (bulletIndex < 0 || bulletIndex >= objective.Bullets.Count)
                {
                    return null;
                }

                return objective.Bullets[bulletIndex].Children;
            }

            Bullet? parentBullet = GetBulletByPath(objective, bulletIndex, parentPath);

            return parentBullet?.Children;
        }

        private static bool RemoveBulletByPath(Objective objective, int bulletIndex, List<int> childPath)
        {
            if (childPath.Count == 0)
            {
                if (bulletIndex < 0 || bulletIndex >= objective.Bullets.Count)
                {
                    return false;
                }

                objective.Bullets.RemoveAt(bulletIndex);
                return true;
            }

            List<int> parentPath = childPath.Take(childPath.Count - 1).ToList();
            int removeIndex = childPath.Last();

            List<Bullet>? siblings = GetChildrenCollectionByPath(objective, bulletIndex, parentPath);

            if (siblings == null || removeIndex < 0 || removeIndex >= siblings.Count)
            {
                return false;
            }

            siblings.RemoveAt(removeIndex);
            return true;
        }

        private static string? GetJsonFileName(string examCode)
        {
            return examCode switch
            {
                "220-1201" => "Core1.json",
                "220-1202" => "Core2.json",
                _ => null
            };
        }
    }
}