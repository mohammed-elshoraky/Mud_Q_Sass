using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mud.Shared.DTOs;
using Mud_Q_Sass.API.Services.Interfaces;

[Authorize(Policy = "CompanyAdmin")]
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? companyId = null)
    {
        var response = await _userService.GetAllAsync(companyId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _userService.GetByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var response = await _userService.CreateAsync(dto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        var response = await _userService.UpdateAsync(id, dto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _userService.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("{id}/assign-role")]
    public async Task<IActionResult> AssignRole(int id, [FromBody] AssignRoleRequest request)
    {
        var response = await _userService.AssignRoleAsync(id, request.RoleName, request.CompanyId, request.BranchId);
        return StatusCode(response.StatusCode, response);
    }
}

public class AssignRoleRequest
{
    public string RoleName { get; set; } = null!;
    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }
}