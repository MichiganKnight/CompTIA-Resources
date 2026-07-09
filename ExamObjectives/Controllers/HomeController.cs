using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ExamObjectives.Models;
using Newtonsoft.Json;

namespace ExamObjectives.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        
        public HomeController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Core1()
        {
            return View("~/Views/Home/Exam/220-1201.cshtml");
        }
        
        public IActionResult Core2()
        {
            return View("~/Views/Home/Exam/220-1202.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> AddChecklistItem([FromBody] ChecklistEditRequest request)
        {
            CertificationModel? certification = await LoadCertificationAsync(request.ExamCode);

            if (certification == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid Exam JSON"
                });
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
                        return NotFound(new
                        {
                            success = false,
                            message = "Domain Not Found"
                        });
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
                        return NotFound(new
                        {
                            success = false,
                            message = "Objective Not Found"
                        });
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
                        return NotFound(new
                        {
                            success = false,
                            message = "Objective Not Found"
                        });
                    }

                    if (request.BulletIndex < 0 || request.BulletIndex >= objective.Bullets.Count)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid Bullet Index"
                        });
                    }
                    
                    objective.Bullets[request.BulletIndex].Children.Add(new Bullet
                    {
                        Text = request.Text,
                        Completed = false,
                        Children = request.Children
                    });
                    break;
                
                default:
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid Item Type"
                    });
            }
            
            await SaveCertificationAsync(request.ExamCode, certification);
            
            return Ok(new
            {
                success = true,
                message = "Checklist Item Added"
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditChecklistItem([FromBody] ChecklistEditRequest request)
        {
            CertificationModel? certification = await LoadCertificationAsync(request.ExamCode);
            
            if (certification == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid Exam JSON"
                });
            }
            
            Domain? domain = certification.Domains.FirstOrDefault(d => d.Id == request.DomainId);
            Objective? objective = domain?.Objectives.FirstOrDefault(o => o.Id == request.ObjectiveId);
            
            switch (request.ItemType)
            {
                case "domain":
                    if (domain == null)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "Domain Not Found"
                        });
                    }
                    
                    domain.Number = request.Number;
                    domain.Title = request.Title;
                    domain.Weight = request.Weight;
                    domain.Objectives = request.Objectives;
                    break;
                
                case "objective":
                    if (objective == null)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "Objective Not Found"
                        });
                    }
                    
                    objective.Number = request.Number;
                    objective.Title = request.Title;
                    objective.Bullets = request.Bullets;
                    break;
                
                case "bullet":
                    if (objective == null)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "Objective Not Found"
                        });
                    }

                    if (request.BulletIndex < 0 || request.BulletIndex >= objective.Bullets.Count)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid Bullet Index"
                        });
                    }
                    
                    objective.Bullets[request.BulletIndex].Text = request.Text;
                    objective.Bullets[request.BulletIndex].Children = request.Children;
                    break;
                
                case "child-bullet":
                    if (objective == null)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "Objective Not Found"
                        });
                    }
                    
                    if (request.BulletIndex < 0 || request.BulletIndex >= objective.Bullets.Count)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid Bullet Index"
                        });
                    }
                    
                    Bullet parentBullet = objective.Bullets[request.BulletIndex];

                    if (request.ChildIndex < 0 || request.ChildIndex >= objective.Bullets[request.BulletIndex].Children.Count)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid Child Index"
                        });
                    }
                    
                    parentBullet.Children[request.ChildIndex].Text = request.Text;
                    parentBullet.Children[request.ChildIndex].Children = request.Children;
                    break;
                
                default:
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid Item Type"
                    });
            }
            
            await SaveCertificationAsync(request.ExamCode, certification);
            
            return Ok(new
            {
                success = true,
                message = "Checklist Item Updated"
            });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteChecklistItem([FromBody] ChecklistEditRequest request)
        {
            CertificationModel? certification = await LoadCertificationAsync(request.ExamCode);
            
            if (certification == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid Exam JSON"
                });
            }
            
            Domain? domain = certification.Domains.FirstOrDefault(d => d.Id == request.DomainId);
            Objective? objective = domain?.Objectives.FirstOrDefault(o => o.Id == request.ObjectiveId);
            
            switch (request.ItemType)
            {
                case "domain":
                    if (domain == null)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "Domain Not Found"
                        });
                    }
                    
                    certification.Domains.Remove(domain);
                    break;
                
                case "objective":
                    if (domain == null || objective == null)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "Objective Not Found"
                        });
                    }
                    
                    domain.Objectives.Remove(objective);
                    break;
                
                case "bullet":
                    if (objective == null)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "Objective Not Found"
                        });
                    }

                    if (request.BulletIndex < 0 || request.BulletIndex >= objective.Bullets.Count)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid Bullet Index"
                        });
                    }
                    
                    objective.Bullets.RemoveAt(request.BulletIndex);
                    break;
                
                case "child-bullet":
                    if (objective == null)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "Objective Not Found"
                        });
                    }
                    
                    if (request.BulletIndex < 0 || request.BulletIndex >= objective.Bullets.Count)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid Bullet Index"
                        });
                    }
                    
                    Bullet parentBullet = objective.Bullets[request.BulletIndex];
                    
                    if (request.ChildIndex < 0 || request.ChildIndex >= parentBullet.Children.Count)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid Child Index"
                        });
                    }
                    
                    parentBullet.Children.RemoveAt(request.ChildIndex);
                    break;
                
                default:
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid Item Type"
                    });
            }
            
            await SaveCertificationAsync(request.ExamCode, certification);
            
            return Ok(new
            {
                success = true,
                message = "Checklist Item Deleted"
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProgress([FromBody] ProgressUpdateRequest request)
        {
            string? jsonFileName = GetJsonFileName(request.ExamCode);

            if (jsonFileName == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid Exam Code"
                });
            }
            
            string jsonPath = Path.Combine(_environment.WebRootPath, "data", jsonFileName);

            if (!System.IO.File.Exists(jsonPath))
            {
                return NotFound(new
                {
                    success = false,
                    message = $"{jsonFileName} Not Found"
                });
            }
            
            string json = await System.IO.File.ReadAllTextAsync(jsonPath);
            CertificationModel? certification = JsonConvert.DeserializeObject<CertificationModel>(json);

            if (certification == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Unable to Parse {jsonFileName}"
                });
            }
            
            Domain? domain = certification.Domains.FirstOrDefault(d => d.Id == request.DomainId);
            
            if (domain == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Domain Not Found"
                });
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
                        return NotFound(new
                        {
                            success = false,
                            message = "Objective Not Found"
                        });
                    }
                
                    objective.Completed = request.Completed;
                    break;
                }
                case "bullet":
                {
                    Objective? objective = domain.Objectives.FirstOrDefault(o => o.Id == request.ObjectiveId);
                
                    if (objective == null)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "Objective Not Found"
                        });
                    }

                    if (request.BulletIndex < 0 || request.BulletIndex >= objective.Bullets.Count)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid Bullet Index"
                        });
                    }
                
                    objective.Bullets[request.BulletIndex].Completed = request.Completed;
                    break;
                }
                case "child-bullet":
                {
                    Objective? objective = domain.Objectives.FirstOrDefault(o => o.Id == request.ObjectiveId);
                
                    if (objective == null)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "Objective Not Found"
                        });
                    }
                
                    if (request.BulletIndex < 0 || request.BulletIndex >= objective.Bullets.Count)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid Bullet Index"
                        });
                    }
                
                    Bullet parentBullet = objective.Bullets[request.BulletIndex];

                    if (request.ChildIndex < 0 || request.ChildIndex >= parentBullet.Children.Count)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid Child Index"
                        });
                    }
                
                    parentBullet.Children[request.ChildIndex].Completed = request.Completed;
                    break;
                }
                default:
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid Item Type"
                    });
            }
            
            string updatedJson = JsonConvert.SerializeObject(certification, Formatting.Indented);
            await System.IO.File.WriteAllTextAsync(jsonPath, updatedJson);
            
            return Ok(new
            {
                success = true,
                message = "Progress Updated"
            });
        }

        private async Task<CertificationModel?> LoadCertificationAsync(string examCode)
        {
            string? jsonFileName = GetJsonFileName(examCode);
            
            if (jsonFileName == null)
            {
                return null;
            }
            
            string jsonPath = Path.Combine(_environment.WebRootPath, "data", jsonFileName);
            
            if (!System.IO.File.Exists(jsonPath))
            {
                return null;
            }
            
            string json = await System.IO.File.ReadAllTextAsync(jsonPath);
            return JsonConvert.DeserializeObject<CertificationModel>(json);
        }

        private async Task SaveCertificationAsync(string examCode, CertificationModel certification)
        {
            string? jsonFileName = GetJsonFileName(examCode);
            
            if (jsonFileName == null)
            {
                return;
            }
            
            string jsonPath = Path.Combine(_environment.WebRootPath, "data", jsonFileName);
            string updatedJson = JsonConvert.SerializeObject(certification, Formatting.Indented);
            
            await System.IO.File.WriteAllTextAsync(jsonPath, updatedJson);
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
    
    public class ProgressUpdateRequest
    {
        public string ExamCode { get; set; } = "";
        public string ItemType { get; set; } = "";
        public int DomainId { get; set; }
        public int ObjectiveId { get; set; }
        public int BulletIndex { get; set; } = -1;
        public int ChildIndex { get; set; } = -1;
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
        public string Number { get; set; } = "";
        public string Title { get; set; } = "";
        public string Text { get; set; } = "";
        public int Weight { get; set; } = 0;
        public List<Objective> Objectives { get; set; } = [];
        public List<Bullet> Bullets { get; set; } = [];
        public List<Bullet> Children { get; set; } = [];
    }
}