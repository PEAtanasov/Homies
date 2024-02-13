using Homies.Data;
using Homies.Data.Models;
using Homies.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Globalization;
using System.Security.Claims;

namespace Homies.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        private readonly HomiesDbContext data;
        public EventController(HomiesDbContext context)
        {
            this.data = context;
        }

        [HttpGet]
        public async Task<IActionResult> All()
        {

            var events = await data.Events
                .AsNoTracking()
                .Select(e => new EventViewModel()
                {
                    Id = e.Id,
                    Organiser = e.Organiser.UserName,
                    Type = e.Type.Name,
                    Name = e.Name,
                    Start = e.Start.ToString(DataConstants.DateFormat),
                })
                .ToListAsync();

            return View(events);
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var types = await GetTypesAsync();

            var eventForm = new EventFormModel()
            {
                Types = types
            };

            return View(eventForm);
        }

        [HttpPost]
        public async Task<IActionResult> Add(EventFormModel model)
        {
            DateTime parsedStart = DateTime.Now;
            DateTime parsedEnd = DateTime.Now;

            if (!DateTime.TryParseExact(
                model.Start,
                DataConstants.DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out parsedStart))
            {
                ModelState.AddModelError(nameof(model.Start), $"Invalid Date. Format must be: {DataConstants.DateFormat}");
            }

            if (!DateTime.TryParseExact(
                model.End,
                DataConstants.DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out parsedEnd))
            {
                ModelState.AddModelError(nameof(model.End), $"Invalid Date. Format must be: {DataConstants.DateFormat}");
            }

            if (!ModelState.IsValid)
            {
                model.Types = await GetTypesAsync();
                return View(model);
            }

            var entity = new Event()
            {
                Name = model.Name,
                Description = model.Description,
                CreatedOn = DateTime.Now,
                Start = parsedStart,
                End = parsedEnd,
                TypeId = model.TypeId,
                OrganiserId = GetUserId()
            };

            await data.AddAsync(entity);
            await data.SaveChangesAsync();

            return RedirectToAction(nameof(All));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetUserId();
          
            var eventToEdit = await data.Events.FindAsync(id);

            if (eventToEdit == null)
            {
                return BadRequest("The event does not exist");
            }

            if (userId != eventToEdit.OrganiserId)
            {
                return Unauthorized("You are not allowed to edit this event");
            }

            var eventForm = new EventFormModel()
            {
                Id = eventToEdit.Id,
                Name = eventToEdit.Name,
                Description = eventToEdit.Description,
                OrganiserId = eventToEdit.OrganiserId,
                TypeId = eventToEdit.TypeId,
                Start = eventToEdit.Start.ToString(DataConstants.DateFormat),
                End = eventToEdit.End.ToString(DataConstants.DateFormat),
                Types = await GetTypesAsync()
        };

            return View(eventForm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EventFormModel model)
        {
            DateTime parsedStart = DateTime.Now;
            DateTime parsedEnd = DateTime.Now;

            if (!DateTime.TryParseExact(
                model.Start,
                DataConstants.DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out parsedStart))
            {
                ModelState.AddModelError(nameof(model.Start), $"Invalid Date. Format must be: {DataConstants.DateFormat}");
            }

            if (!DateTime.TryParseExact(
                model.End,
                DataConstants.DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out parsedEnd))
            {
                ModelState.AddModelError(nameof(model.End), $"Invalid Date. Format must be: {DataConstants.DateFormat}");
            }

            if (!ModelState.IsValid)
            {
                model.Types = await GetTypesAsync();
                return View(model);
            }

            var eventToEdit = await data.Events.FindAsync(model.Id);

            if (eventToEdit == null)
            {
                return BadRequest("The event does not exist");
            }

            if (GetUserId() != eventToEdit.OrganiserId)
            {
                return Unauthorized("You are not allowed to edit this event");
            }

            eventToEdit.Id = model.Id;
            eventToEdit.Name = model.Name;
            eventToEdit.Description = model.Description;
            eventToEdit.Start= parsedStart;
            eventToEdit.End= parsedEnd;
            eventToEdit.TypeId= model.TypeId;

            await data.SaveChangesAsync();

            return RedirectToAction(nameof(All));
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private async Task<IList<TypeViewModel>> GetTypesAsync()
        {
            var types = await data.Types
                .AsNoTracking()
                .Select(e => new TypeViewModel()
                {
                    Id = e.Id,
                    Name = e.Name
                })
                .ToListAsync();

            return types;
        }
    }
}