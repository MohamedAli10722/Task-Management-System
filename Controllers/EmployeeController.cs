using Area.DTOs;
using Area.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Area.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly AreaContext _context;

        public EmployeeController(AreaContext context)
        {
            _context = context;
        }
        
        #region AddEmployee
        [HttpPost]
        public IActionResult AddEmployee([FromBody] EmployeeDTO employeeDto)
        {
            if (employeeDto == null)
            {
                return BadRequest("Invalid employee data.");
            }

            string roleId = GetRoleIdFromRoleName(employeeDto.Discriminator);
            if (string.IsNullOrEmpty(roleId))
            {
                return BadRequest("Invalid role.");
            }

            var existingEmployee = _context.Persons.FirstOrDefault(p => p.NationalNumber == employeeDto.NationalNumber);
            if (existingEmployee != null)
            {
                return BadRequest("Employee with this National ID already exists.");
            }

            string generatedUsername = employeeDto.FirstName + " " + employeeDto.LastName;

            var existingUsername = _context.Persons
                .FirstOrDefault(p => p.UserName == generatedUsername);
            if (existingUsername != null)
            {
                return BadRequest($"An employee with the username '{generatedUsername}' already exists.");
            }

            if (employeeDto.Discriminator == "Developer_Employee")
            {
                var allowedJobs = new List<string>
                {
                    "Backend (.Net)",
                    "Backend (Java Spring)",
                    "Backend (Node.js)",
                    "Backend (php)",
                    "Frontend (React.js)",
                    "Frontend (Angular.js)",
                    "Frontend (Vue.js)" ,
                    "Mobile (Flutter)" ,
                    "Mobile (Android)" ,
                    "Mobile (ios)"
                };
                if (!allowedJobs.Contains(employeeDto.job))
                {
                    return BadRequest("Invalid job title for Developer.");
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(employeeDto.job))
                {
                    return BadRequest("Job title is only allowed for Developers.");
                }
            }

            var newEmployee = new Person
            {
                NationalNumber = employeeDto.NationalNumber,
                FirstName = employeeDto.FirstName,
                LastName = employeeDto.LastName,
                UserName = generatedUsername,
                MobileNumber = employeeDto.Phone,
                Email = employeeDto.Email,
                Password = employeeDto.Password,
                DateOfBirth = employeeDto.Date_Of_Birth,
                Nationality = employeeDto.Nationality,
                Location = employeeDto.Address,
                Roleid = roleId,
                Discriminator = employeeDto.Discriminator,
                jobtitle = employeeDto.job
            };

            _context.Persons.Add(newEmployee);
            _context.SaveChanges();

            var response = new
            {
                employeeDto.NationalNumber,
                employeeDto.FirstName,
                employeeDto.LastName,
                employeeDto.Phone,
                employeeDto.Email,
                employeeDto.Password,
                employeeDto.Date_Of_Birth,
                employeeDto.Nationality,
                employeeDto.Address,
                employeeDto.Discriminator,
                employeeDto.job
            };

            return Ok(response);
        }
        #endregion

        #region GetRoleIdFromRoleName
        private string GetRoleIdFromRoleName(string roleName)
        {
            var role = _context.Roles.FirstOrDefault(r => r.Role_name.ToLower() == roleName.ToLower());
            if (role != null)
            {
                return role?.Role_id ?? string.Empty;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region GetRolesWithEmployees
        [HttpGet]
        [Route("Roles")]
        public IActionResult GetRolesWithEmployees()
        {
            var rolesWithEmployees = _context.Roles
                .Select(role => new
                {
                    RoleId = role.Role_id,
                    RoleName = role.Role_name,
                    Employees = _context.Persons
                        .Where(p => p.Roleid == role.Role_id)
                        .Select(emp => new
                        {
                            emp.UserName,
                            emp.NationalNumber,
                            emp.Email,
                            emp.Password,
                            emp.MobileNumber,
                            emp.jobtitle,
                            emp.Discriminator,
                            emp.ImagePath,
                            emp.DateOfBirth,
                            emp.Location,
                            emp.Nationality
                        }).ToList()
                }).ToList();

            return Ok(rolesWithEmployees);
        }
        #endregion

        #region GetbyName
        [HttpGet("{name}")]
        public ActionResult getbyname(string name)
        {
            Person s = _context.Persons.Where(n => n.UserName == name).FirstOrDefault();
            if (s == null) return NotFound();
            else return Ok(s);
        }
        #endregion

        #region Employee Evaluation
        [HttpGet("Evaluation")]
        public async Task<IActionResult> GetAllEmployeesWithEvaluations()
        {
            var groupedResults = new List<EmployeeEvaluationGroupDto>();

            // 1. Product Owners
            var productOwners = await _context.Persons
                .Where(p => p.Discriminator == "Product_Owner")
                .ToListAsync();

            var poGroup = new EmployeeEvaluationGroupDto { Role = "Product_Owner" };

            foreach (var po in productOwners)
            {
                var ownedProjects = await _context.Projects
                    .Where(pr => pr.Product_Id == po.NationalNumber)
                    .Select(pr => pr.Title)
                    .ToListAsync();

                var kpiEvaluations = await _context.KPISEvaluation
                    .Where(k => k.EmployeeId == po.NationalNumber)
                    .Select(k => new KPIEvaluationSummaryDto
                    {
                        FinalScore = k.FinalScore,
                        EvaluationDate = k.EvaluationDate
                    }).ToListAsync();

                var poDto = new ProductOwnerEvaluationDto
                {
                    UserName = po.UserName,
                    Discriminator = po.Discriminator,
                    OwnedProjects = ownedProjects,
                    KPIEvaluations = kpiEvaluations
                };

                poGroup.Employees.Add(poDto);
            }

            if (poGroup.Employees.Any())
                groupedResults.Add(poGroup);

            // 2. Other employees
            var roles = new[] { "Team_Leader", "Developer_Employee", "Test_Employee", "UI_Employee" };

            foreach (var role in roles)
            {
                var employees = await _context.Persons
                    .Where(p => p.Discriminator == role)
                    .ToListAsync();

                var group = new EmployeeEvaluationGroupDto { Role = role };

                foreach (var emp in employees)
                {
                    List<TaskReviewSummaryDto> taskEvaluations = new();

                    if (emp.Discriminator == "Developer_Employee" || emp.Discriminator == "UI_Employee")
                    {
                        var tasks = await _context.Tasks
                            .Where(t => t.Developer_EmployeeId == emp.NationalNumber || t.UI_UXId == emp.NationalNumber)
                            .Include(t => t.ChecklistReviews)
                            .ToListAsync();

                        taskEvaluations = tasks
                            .SelectMany(t => t.ChecklistReviews.Select(r => new TaskReviewSummaryDto
                            {
                                TaskTitle = t.Title,
                                TotalScore = $"{r.TotalScore}%",
                                ReviewDate = r.ReviewDate
                            }))
                            .ToList();
                    }

                    var kpiEvaluations = await _context.KPISEvaluation
                        .Where(k => k.EmployeeId == emp.NationalNumber)
                        .Select(k => new KPIEvaluationSummaryDto
                        {
                            FinalScore = k.FinalScore,
                            EvaluationDate = k.EvaluationDate
                        }).ToListAsync();

                    var empDto = new EmployeeTaskEvaluationDto
                    {
                        UserName = emp.UserName,
                        Discriminator = emp.Discriminator,
                        TaskEvaluations = taskEvaluations,
                        KPIEvaluations = kpiEvaluations
                    };

                    group.Employees.Add(empDto);
                }

                if (group.Employees.Any())
                    groupedResults.Add(group);
            }

            return Ok(groupedResults);
        }
        #endregion
    }
}
    