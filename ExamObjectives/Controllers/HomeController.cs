using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ExamObjectives.Models;
using ExamObjectives.Models.ViewModels;
using ExamObjectives.Services;

namespace ExamObjectives.Controllers
{
    public class HomeController : Controller
    {
        private readonly ChecklistService _checklistService;
        
        public HomeController(ChecklistService checklistService)
        {
            _checklistService = checklistService;
        }
        
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Core1()
        {
            CertificationModel model = await _checklistService.LoadExamAsync("Core1.json");

            return View("~/Views/Home/Exam/_Checklist.cshtml", model);
        }
        
        public async Task<IActionResult> Core2()
        {
            CertificationModel model = await _checklistService.LoadExamAsync("Core2.json");

            return View("~/Views/Home/Exam/_Checklist.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> AddChecklistItem([FromBody] ChecklistEditRequest request)
        {
            ChecklistServiceResult result = await _checklistService.AddChecklistItemAsync(request);
            return ToActionResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> EditChecklistItem([FromBody] ChecklistEditRequest request)
        {
            ChecklistServiceResult result = await _checklistService.EditChecklistItemAsync(request);
            return ToActionResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteChecklistItem([FromBody] ChecklistEditRequest request)
        {
            ChecklistServiceResult result = await _checklistService.DeleteChecklistItemAsync(request);
            return ToActionResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProgress([FromBody] ProgressUpdateRequest request)
        {
            ChecklistServiceResult result = await _checklistService.UpdateProgressAsync(request);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult(ChecklistServiceResult result)
        {
            object response = new
            {
                success = result.Success,
                message = result.Message
            };

            return result.StatusCode switch
            {
                StatusCodes.Status200OK => Ok(response),
                StatusCodes.Status400BadRequest => BadRequest(response),
                StatusCodes.Status404NotFound => NotFound(response),
                _ => StatusCode(result.StatusCode, response)
            };
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}