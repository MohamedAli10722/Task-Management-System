using Area.DTOs;
using Area.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Area.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KPISController : ControllerBase
    {
        private readonly AreaContext _context;

        public KPISController(AreaContext context)
        {
            _context = context;
        }

        #region Get All KPIs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<KPISDto>>> GetAllKPIs()
        {
            var kpis = await _context.KPIS
                .Select(k => new KPISDto
                {
                    Id = k.Id,
                    Title = k.Title,
                    Score = k.Score
                })
                .ToListAsync();

            return Ok(kpis);
        }
        #endregion

        #region Add KPI
        [HttpPost("Add")]
        public async Task<IActionResult> AddKPI([FromBody] KPISDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Invalid KPI data.");

            var kpi = new KPIS
            {
                Id = Guid.NewGuid().ToString(),
                Title = dto.Title,
                Score = dto.Score
            };

            _context.KPIS.Add(kpi);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "KPI added successfully", KPIId = kpi.Id });
        }
        #endregion

        #region Get KPIS With Name
        [HttpPost]
        public async Task<IActionResult> GetKPIsForEmployee([FromBody] KPISGetDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserName))
                return BadRequest("Employee username is required.");

            var employee = await _context.Persons.FirstOrDefaultAsync(p => p.UserName == dto.UserName);
            if (employee == null)
                return NotFound("Employee not found.");

            var kpis = await _context.KPIS
                .Select(k => new
                {
                    k.Id,
                    k.Title
                })
         .ToListAsync();

            return Ok(new
            {
                Employee = employee.UserName,
                KPIs = kpis
            });
        }
        #endregion

        #region Submit KPIs Check List
        [HttpPost("Submit")]
        public async Task<IActionResult> SubmitEvaluation([FromBody] KPIEvaluationDto dto)
        {
            if (dto == null || dto.Selections == null || !dto.Selections.Any())
                return BadRequest("Invalid evaluation data.");

            //var EvaluatorUsername = User.Identity?.Name;
            //if (string.IsNullOrEmpty(EvaluatorUsername))
            //    return Unauthorized("Evaluator Username not found in token.");

            //var Evaluator = await _context.Persons.FirstOrDefaultAsync(p => p.UserName == EvaluatorUsername);
            //if (Evaluator == null)
            //    return NotFound("Evaluator username not found.");

            var employee = await _context.Persons.FirstOrDefaultAsync(p => p.UserName == dto.Employee);
            if (employee == null)
                return NotFound("Employee username not found.");

            var kpiIds = dto.Selections.Select(s => s.KPIId).ToList();
            var kpis = await _context.KPIS
                .Where(k => kpiIds.Contains(k.Id))
                .ToListAsync();

            if (kpis.Count != dto.Selections.Count)
                return BadRequest("Some KPI IDs are invalid.");

            // Score
            int totalPossibleScore = kpis.Sum(k => k.Score);
            int totalAchievedScore = 0;

            var selectionEntities = dto.Selections.Select(sel =>
            {
                var kpi = kpis.First(k => k.Id == sel.KPIId);
                var score = sel.IsSelected ? kpi.Score : 0;
                if (sel.IsSelected)
                    totalAchievedScore += score;

                return new KPISelection
                {
                    KPIId = sel.KPIId,
                    IsSelected = sel.IsSelected,
                    Score = score
                };
            }).ToList();

            string finalPercentage = "0.00 %";
            if (totalPossibleScore > 0)
            {
                double percent = (double)totalAchievedScore / totalPossibleScore * 100;
                finalPercentage = percent.ToString("0.00") + " %";
            }

            // Evaluation
            var evaluation = new KPISEvaluation
            {
                //ManagerId = Evaluator.NationalNumber,
                EmployeeId = employee.NationalNumber,
                EvaluationDate = DateTime.UtcNow,
                FinalScore = finalPercentage,
                KPISelections = selectionEntities
            };

            _context.KPISEvaluation.Add(evaluation);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Evaluation submitted",
                FinalScore = finalPercentage
            });
        }
        #endregion
    }
}
