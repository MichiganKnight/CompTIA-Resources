using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ExamObjectives.Models;
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