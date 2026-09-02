using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Note.Api.Handlers;
using Note.Application.Services;
using NoteV1Controller = Note.Api.Controllers.v1.NotesController;

namespace Note.Api.Controllers.v2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/notes")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = HmacAuthenticationHandler.Scheme)]
public sealed class NotesController : NoteV1Controller
{
    public NotesController(INoteService service) : base(service)
    {
    }
}