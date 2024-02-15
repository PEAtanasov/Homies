using Homies.Data;
using Homies.Data.Models;
using Homies.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
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

        [HttpPost]
        public async Task<IActionResult> Join(int id)
        {
            var eventToJoin = await data.Events
                .Where(e => e.Id == id)
                .Include(e => e.EventsParticipants)
                .FirstOrDefaultAsync();

            if (eventToJoin == null)
            {
                return BadRequest();
            }

            if (eventToJoin.EventsParticipants.Any(e=>e.HelperId==GetUserId()))
            {
                return RedirectToAction("Joined");
            }

            eventToJoin.EventsParticipants.Add(new EventParticipant() 
            {
                EventId=id,
                HelperId=GetUserId(),
            });
            await data.SaveChangesAsync();

            return RedirectToAction("Joined");
        }

        [HttpGet]
        public async Task<IActionResult> Joined()
        {
            var userId= GetUserId();

            var ep = await data.EventsParticipants
                .AsNoTracking()
                .Where(ep=>ep.HelperId==userId)
                .Select(ep=> new EventViewModel()
                {
                    Id=ep.EventId,
                    Name=ep.Event.Name,
                    Start=ep.Event.Start.ToString(DataConstants.DateFormat),
                    Type=ep.Event.Type.Name,
                    Organiser=ep.Event.Organiser.UserName

                })
                .ToListAsync();

            var myOwnEvents = await data.Events
                .AsNoTracking()
                .Where(e=>e.OrganiserId==userId)
                .Select(e=>new EventViewModel()
                {
                    Id = e.Id,
                    Name = e.Name,
                    Start = e.Start.ToString(DataConstants.DateFormat),
                    Type = e.Type.Name,
                    Organiser = e.Organiser.UserName
                })
                .ToListAsync();

            foreach (var item in myOwnEvents)
            {
                ep.Add(item);
            }

            return View(ep);
        }

        [HttpPost]
        public async Task<IActionResult> Leave(int id)
        {
            var eventToLeave = await data.Events
                .Where(e=>e.Id==id)
                .Include(e=>e.EventsParticipants)
                .FirstOrDefaultAsync();  

            if (eventToLeave==null)
            {
                return BadRequest();
            }

            if (!eventToLeave.EventsParticipants.Any(ep => ep.HelperId==GetUserId()))
            {
                return BadRequest();
            }

            var entityToLeave = eventToLeave.EventsParticipants.FirstOrDefault(ep => ep.HelperId == GetUserId());

            if (entityToLeave!=null)
            {
                data.EventsParticipants.Remove(entityToLeave);
                await data.SaveChangesAsync();
            }

            return RedirectToAction(nameof(All));         
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var eventToDisplay = await data.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e=>e.Id==id);

            if (eventToDisplay==null)
            {
                return BadRequest();
            }

            var type = await data.Types
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == eventToDisplay.TypeId);

            if (type == null)
            {
                return BadRequest();
            }

            var organiser = await data.Users.FindAsync(eventToDisplay.OrganiserId);

            if (organiser == null)
            {
                return BadRequest();
            }

            var model = new EventDetailsViewModel()
            {
                Id = eventToDisplay.Id,
                Name = eventToDisplay.Name,
                Description = eventToDisplay.Description,
                Start = eventToDisplay.Start.ToString(DataConstants.DateFormat),
                End = eventToDisplay.End.ToString(DataConstants.DateFormat),
                Organiser = organiser.UserName,
                CreatedOn=eventToDisplay.CreatedOn.ToString(DataConstants.DateFormat),
                Type= type.Name
            };

            return View(model);
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